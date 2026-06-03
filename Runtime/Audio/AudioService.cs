using UnityEngine;

namespace Air.UnityGameCore.Runtime.Audio
{
    public sealed class AudioService : IAudioService
    {
        const int SfxPoolSize = 8;

        GameObject _root;
        AudioSource _bgmSource;
        AudioSource[] _sfxSources;
        int _sfxIndex;

        public float MasterVolume { get; set; } = 1f;
        public float BgmVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;

        public void PlayBgm(AudioClip clip, bool loop = true)
        {
            EnsureRoot();
            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = MasterVolume * BgmVolume;
            if (clip != null)
                _bgmSource.Play();
            else
                _bgmSource.Stop();
        }

        public void StopBgm()
        {
            if (_bgmSource != null)
                _bgmSource.Stop();
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
                return;
            var source = RentSfxSource();
            source.transform.position = Vector3.zero;
            source.spatialBlend = 0f;
            source.volume = MasterVolume * SfxVolume * volumeScale;
            source.PlayOneShot(clip);
        }

        public void PlaySfxAt(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null)
                return;
            var source = RentSfxSource();
            source.transform.position = position;
            source.spatialBlend = 1f;
            source.volume = MasterVolume * SfxVolume * volumeScale;
            source.PlayOneShot(clip);
        }

        public void Dispose()
        {
            if (_root != null)
                Object.Destroy(_root);
            _root = null;
            _bgmSource = null;
            _sfxSources = null;
        }

        void EnsureRoot()
        {
            if (_root != null)
                return;

            _root = new GameObject("Air.Audio");
            Object.DontDestroyOnLoad(_root);

            _bgmSource = _root.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;

            _sfxSources = new AudioSource[SfxPoolSize];
            for (var i = 0; i < SfxPoolSize; i++)
            {
                _sfxSources[i] = _root.AddComponent<AudioSource>();
                _sfxSources[i].playOnAwake = false;
            }
        }

        AudioSource RentSfxSource()
        {
            EnsureRoot();
            var source = _sfxSources[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Length;
            return source;
        }
    }
}
