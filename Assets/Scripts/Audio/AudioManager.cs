using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private string _gameLoopName = "GameLoop";
    [SerializeField] private Sound[] _sounds;

    private bool _isSoundEnabled = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound sound in _sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.playOnAwake = sound.playOnAwake;
            sound.source.loop = sound.loop;
            sound.source.volume = sound.volume;
        }
    }

    private void Update()
    {
        if (!ReturnAudioSource(_gameLoopName).isPlaying)
        {
            Play(_gameLoopName);
        }
    }

    public void Play(string name)
    {
        Sound snd = Array.Find(_sounds, sound => sound.name == name);
        if (snd == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        snd.source.Play();
    }

    public void PlayOneShot(string name)
    {
        Sound snd = Array.Find(_sounds, sound => sound.name == name);
        if (snd == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        snd.source.PlayOneShot(snd.clip);
    }

    public AudioSource ReturnAudioSource(string name)
    {
        Sound snd = Array.Find(_sounds, sound => sound.name == name);
        return snd.source;
    }

    public void EnableDisableSoundVolume()
    {
        if (_isSoundEnabled)
        {
            foreach (Sound sound in _sounds)
            {
                sound.source.volume = 0;
            }

            _isSoundEnabled = false;
        }
        else if (!_isSoundEnabled)
        {
            foreach (Sound sound in _sounds)
            {
                sound.source.volume = sound.volume;
            }

            _isSoundEnabled = true;
        }
    }

    public bool ReturnSoundEnabled()
    {
        return _isSoundEnabled;
    }
}

