using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ANut.Core.Save;
using Newtonsoft.Json;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace ANut.Core.Audio
{
    public sealed class AudioService : SaveModuleBase<AudioService.Save>, IAudioService, IInitializable, IDisposable
    {
        // Nested Save — exactly what goes to disk 

        public sealed class Save
        {
            [JsonProperty("master")] public float MasterVolume { get; set; } = 1f;
            [JsonProperty("music")] public float MusicVolume { get; set; } = 1f;
            [JsonProperty("sfx")] public float SfxVolume { get; set; } = 1f;
            [JsonProperty("muted")] public bool IsMuted { get; set; } = false;
        }

        // ISaveModule

        public override string Key => "audio";

        // Dependencies 

        private readonly AudioConfigSO _config;

        // Runtime objects 

        private GameObject _host;
        private AudioSource[] _sfxPool;
        private int _sfxPoolIndex;

        // Two music sources for A/B crossfade
        private AudioSource _musicA;
        private AudioSource _musicB;
        private bool _musicAIsActive; // which source is currently the "main" track
        private readonly CompositeDisposable _disposables = new();

        private MusicId _currentMusicId = MusicId.None;

        // Cancellation for any ongoing fade operation.
        // Cancel + replace whenever we start a new fade, so old fade doesn't interfere.
        private CancellationTokenSource _fadeCts = new();

        // Reactive properties 

        public ReactiveProperty<float> MasterVolumeProperty { get; } = new(1f);
        public ReactiveProperty<float> MusicVolumeProperty { get; } = new(1f);
        public ReactiveProperty<float> SfxVolumeProperty { get; } = new(1f);
        public ReactiveProperty<bool> IsMutedProperty { get; } = new(false);


        // Constructor

        public AudioService(AudioConfigSO config)
        {
            _config = config;

            MasterVolumeProperty.AddTo(_disposables);
            MusicVolumeProperty.AddTo(_disposables);
            SfxVolumeProperty.AddTo(_disposables);
            IsMutedProperty.AddTo(_disposables);

            Disposable.Create(() =>
            {
                _fadeCts.Cancel();
                _fadeCts.Dispose();
            }).AddTo(_disposables);

            Disposable.Create(() =>
            {
                if (_host == null) return;
                UnityEngine.Object.Destroy(_host);
            }).AddTo(_disposables);
        }

        // IInitializable

        public void Initialize()
        {
            _config.BuildLookups();
            CreateAudioHost();
            // Apply default settings immediately so audio works before save is loaded.
            // OnAfterDeserialize() will override with saved values once SaveService loads.
            SyncReactiveFromData();
            ApplyMixerVolumes();
        }

        // IAudioService — Playback 

        public void PlaySfx(SoundId id)
        {
            if (IsMutedProperty.Value) return;
            if (!_config.TryGetSound(id, out var entry)) return;

            var source = GetNextPooledSource();
            source.clip = entry.Clips[UnityEngine.Random.Range(0, entry.Clips.Length)];
            source.volume = entry.Volume;
            source.pitch = UnityEngine.Random.Range(entry.PitchMin, entry.PitchMax);
            source.Play();
        }

        public void PlayMusic(MusicId id, bool crossfade = true)
        {
            if (id == _currentMusicId) return;
            if (!_config.TryGetMusic(id, out var entry)) return;

            _currentMusicId = id;
            CancelActiveFade();

            if (crossfade && _config.CrossfadeDuration > 0f)
                CrossfadeAsync(entry, _fadeCts.Token).Forget();
            else
                HardSwapMusic(entry);
        }

        public void StopMusic(bool fade = true)
        {
            _currentMusicId = MusicId.None;
            CancelActiveFade();

            if (fade && _config.CrossfadeDuration > 0f)
                FadeOutActiveSourceAsync(_fadeCts.Token).Forget();
            else
                StopBothMusicSources();
        }

        public void StopAllSfx()
        {
            foreach (var src in _sfxPool)
                if (src.isPlaying)
                    src.Stop();
        }

        // IAudioService — Volume / Mute

        public float MasterVolume
        {
            get => MasterVolumeProperty.Value;
            set => SetVolume(MasterVolumeProperty, value, v => Data.MasterVolume = v);
        }

        public float MusicVolume
        {
            get => MusicVolumeProperty.Value;
            set => SetVolume(MusicVolumeProperty, value, v => Data.MusicVolume = v);
        }

        public float SfxVolume
        {
            get => SfxVolumeProperty.Value;
            set => SetVolume(SfxVolumeProperty, value, v => Data.SfxVolume = v);
        }

        public bool IsMuted
        {
            get => IsMutedProperty.Value;
            set
            {
                if (IsMutedProperty.Value == value) return;
                IsMutedProperty.Value = value;
                Data.IsMuted = value;
                MarkDirty();
                ApplyMixerVolumes();
            }
        }

        // SaveModuleBase override

        protected override void OnAfterDeserialize()
        {
            // Called by SaveService after the save file is loaded.
            // Re-sync reactive props and push saved values to the AudioMixer.
            SyncReactiveFromData();
            ApplyMixerVolumes();
        }

        // IDisposable

        public void Dispose() => _disposables.Dispose();

        // Private — Initialization 

        private void CreateAudioHost()
        {
            _host = new GameObject("[AudioService]");
            UnityEngine.Object.DontDestroyOnLoad(_host);

            // SFX pool
            _sfxPool = new AudioSource[_config.SfxPoolSize];
            for (int i = 0; i < _sfxPool.Length; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(_host.transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.outputAudioMixerGroup = _config.SfxGroup;
                _sfxPool[i] = src;
            }

            // Music A / B
            _musicA = CreateMusicSource("Music_A");
            _musicB = CreateMusicSource("Music_B");
            _musicAIsActive = true;
        }

        private AudioSource CreateMusicSource(string goName)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(_host.transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.outputAudioMixerGroup = _config.MusicGroup;
            return src;
        }

        // Private — Playback helpers

        private AudioSource GetNextPooledSource()
        {
            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;
            return source;
        }

        private void HardSwapMusic(MusicEntry entry)
        {
            StopBothMusicSources();
            var active = ActiveMusicSource();
            active.clip = entry.Clip;
            active.volume = entry.Volume;
            active.Play();
        }

        private void StopBothMusicSources()
        {
            _musicA.Stop();
            _musicB.Stop();
        }

        private AudioSource ActiveMusicSource() => _musicAIsActive ? _musicA : _musicB;
        private AudioSource InactiveMusicSource() => _musicAIsActive ? _musicB : _musicA;

        // Private — Async fades 

        private async UniTaskVoid CrossfadeAsync(MusicEntry entry, CancellationToken ct)
        {
            var fadeOut = ActiveMusicSource();
            // Swap which source is considered "active" before starting the fade.
            _musicAIsActive = !_musicAIsActive;
            var fadeIn = ActiveMusicSource();

            float outStartVol = fadeOut.volume;
            float inTargetVol = entry.Volume;

            fadeIn.clip = entry.Clip;
            fadeIn.volume = 0f;
            fadeIn.Play();

            float duration = _config.CrossfadeDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                fadeOut.volume = Mathf.Lerp(outStartVol, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, inTargetVol, t);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = inTargetVol;
        }

        private async UniTaskVoid FadeOutActiveSourceAsync(CancellationToken ct)
        {
            var source = ActiveMusicSource();
            float startVol = source.volume;
            float duration = _config.CrossfadeDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, Mathf.Clamp01(elapsed / duration));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            source.Stop();
            source.volume = 0f;
        }

        private void CancelActiveFade()
        {
            _fadeCts.Cancel();
            _fadeCts.Dispose();
            _fadeCts = new CancellationTokenSource();
        }

        // Private — Volume helpers 

        private void SetVolume(ReactiveProperty<float> property, float value, Action<float> persist)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(property.Value, value)) return;

            property.Value = value;
            persist(value);
            MarkDirty();
            ApplyMixerVolumes();
        }

        private void SyncReactiveFromData()
        {
            MasterVolumeProperty.Value = Data.MasterVolume;
            MusicVolumeProperty.Value = Data.MusicVolume;
            SfxVolumeProperty.Value = Data.SfxVolume;
            IsMutedProperty.Value = Data.IsMuted;
        }

        private void ApplyMixerVolumes()
        {
            if (_config.Mixer == null) return;

            if (IsMutedProperty.Value)
            {
                _config.Mixer.SetFloat("MasterVolume", -80f);
                return;
            }

            // Restore master first, then set individual channels
            _config.Mixer.SetFloat("MasterVolume", LinearToDb(MasterVolumeProperty.Value));
            _config.Mixer.SetFloat("MusicVolume", LinearToDb(MusicVolumeProperty.Value));
            _config.Mixer.SetFloat("SFXVolume", LinearToDb(SfxVolumeProperty.Value));
        }

        private static float LinearToDb(float linear)
            => Mathf.Log10(Mathf.Max(0.0001f, linear)) * 20f;
    }
}