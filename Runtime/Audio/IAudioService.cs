using UnityEngine;

namespace Air.UnityGameCore.Runtime.Audio
{
  public interface IAudioService
  {
    float MasterVolume { get; set; }
    float BgmVolume { get; set; }
    float SfxVolume { get; set; }

    void PlayBgm(AudioClip clip, bool loop = true);

    void StopBgm();

    void PlaySfx(AudioClip clip, float volumeScale = 1f);

    void PlaySfxAt(AudioClip clip, Vector3 position, float volumeScale = 1f);
  }
}
