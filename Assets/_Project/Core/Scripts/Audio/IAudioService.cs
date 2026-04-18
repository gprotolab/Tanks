using R3;

namespace ANut.Core.Audio
{
    public interface IAudioService
    {
        void PlaySfx(SoundId id);

        void PlayMusic(MusicId id, bool crossfade = true);

        void StopMusic(bool fade = true);

        void StopAllSfx();

        // Mutable properties are used intentionally here.
        ReactiveProperty<float> MasterVolumeProperty { get; }
        ReactiveProperty<float> MusicVolumeProperty { get; }
        ReactiveProperty<float> SfxVolumeProperty { get; }
        ReactiveProperty<bool> IsMutedProperty { get; }
    }
}