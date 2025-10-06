using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource _sourceSFX; // sound effects
    [SerializeField] private AudioSource _sourceBg; // background music
    [SerializeField] private AudioClip _throwSFX;
    [SerializeField] private AudioClip _explosionSFX;
    [SerializeField] private AudioClip _gameOverMusic;
    [SerializeField] private AudioClip _introMusic;
    private Dictionary<AudioClipType, AudioClip> _audioClips;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _sourceBg.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
        _sourceSFX.volume = PlayerPrefs.GetFloat("sfxVolume", 1);

        _audioClips = new Dictionary<AudioClipType, AudioClip>(){
            {AudioClipType.IntroMusic, _introMusic },
            {AudioClipType.ThrowSFX, _throwSFX },
            {AudioClipType.ExplosionSFX, _explosionSFX },
            {AudioClipType.GameOverMusic, _gameOverMusic }
        };
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayAudioClipRpc(AudioClipType audioClipType, float minPitch = 0.95f, float maxPitch = 1.05f)
    {
        _sourceSFX.pitch = Random.Range(minPitch, maxPitch);
        _sourceSFX.PlayOneShot(_audioClips[audioClipType]);
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        _sourceBg.PlayOneShot(clip);
    }

    public void StopBackgroundMusic()
    {
        _sourceBg.Stop();
    }

    public void SetMusicVolume(float vol)
    {
        _sourceBg.volume = vol;
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }

    public void SetSFXVolume(float vol)
    {
        _sourceSFX.volume = vol;
        PlayerPrefs.SetFloat("sfxVolume", vol);
    }
}

public enum AudioClipType
{
    None,
    IntroMusic,
    ThrowSFX,
    ExplosionSFX,
    GameOverMusic
}
