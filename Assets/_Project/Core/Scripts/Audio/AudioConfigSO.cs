using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ANut.Core.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio")]
    public class AudioConfigSO : ScriptableObject
    {
        // Mixer 

        [Header("AudioMixer")] [SerializeField, Tooltip("Root AudioMixer asset.")]
        private AudioMixer _mixer;

        [SerializeField] private AudioMixerGroup _sfxGroup;

        [SerializeField] private AudioMixerGroup _musicGroup;

        // Pool & Crossfade 

        [Header("Runtime Settings")] [SerializeField, Range(4, 24)]
        private int _sfxPoolSize = 8;

        [SerializeField, Range(0f, 2f),
         Tooltip("Duration in seconds for music crossfade. Set to 0 for instant swap.")]
        private float _crossfadeDuration = 0.5f;

        // Catalog 

        [Header("Sound Effects")] [SerializeField]
        private SoundEntry[] _sounds = Array.Empty<SoundEntry>();

        [Header("Music Tracks")] [SerializeField]
        private MusicEntry[] _music = Array.Empty<MusicEntry>();

        // Public accessors 

        public AudioMixer Mixer => _mixer;
        public AudioMixerGroup SfxGroup => _sfxGroup;
        public AudioMixerGroup MusicGroup => _musicGroup;
        public int SfxPoolSize => _sfxPoolSize;
        public float CrossfadeDuration => _crossfadeDuration;

        // Lookup tables — built once at runtime 

        private Dictionary<SoundId, SoundEntry> _soundMap;
        private Dictionary<MusicId, MusicEntry> _musicMap;

        /// Called by AudioService.Initialize(). Builds O(1) lookup dictionaries.
        public void BuildLookups()
        {
            _soundMap = new Dictionary<SoundId, SoundEntry>(_sounds.Length);
            foreach (var e in _sounds)
            {
                if (e.Clips == null || e.Clips.Length == 0)
                {
                    Log.Warning("[AudioConfig] SoundEntry '{0}' has no clips — skipped.", e.Id);
                    continue;
                }

                if (_soundMap.ContainsKey(e.Id))
                {
                    Log.Warning("[AudioConfig] Duplicate SoundId '{0}' — second entry skipped.", e.Id);
                    continue;
                }

                _soundMap[e.Id] = e;
            }

            _musicMap = new Dictionary<MusicId, MusicEntry>(_music.Length);
            foreach (var e in _music)
            {
                if (e.Clip == null)
                {
                    Log.Warning("[AudioConfig] MusicEntry '{0}' has no clip — skipped.", e.Id);
                    continue;
                }

                _musicMap[e.Id] = e;
            }
        }

        public bool TryGetSound(SoundId id, out SoundEntry entry)
            => _soundMap.TryGetValue(id, out entry);

        public bool TryGetMusic(MusicId id, out MusicEntry entry)
            => _musicMap.TryGetValue(id, out entry);
    }

    // Data classes 

    [Serializable]
    public sealed class SoundEntry
    {
        public SoundId Id;

        [Tooltip("Multiple clips = random pick on each play. Great for hits, footsteps, etc.")]
        public AudioClip[] Clips;

        [Range(0f, 1f)] public float Volume = 1f;

        [Range(0.5f, 2f), Tooltip("Randomized pitch range. Set both to 1 for no variation.")]
        public float PitchMin = 1f;

        [Range(0.5f, 2f)] public float PitchMax = 1f;
    }

    [Serializable]
    public sealed class MusicEntry
    {
        public MusicId Id;
        public AudioClip Clip;

        [Range(0f, 1f)] public float Volume = 0.8f;
    }
}