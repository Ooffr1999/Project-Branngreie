using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Audio[] soundClips;
    public AudioRandList[] audioClipLists;
    public MusicTracks[] musicList;

    AudioSource musicSource;

    [HideInInspector]
    public static AudioManager _instance;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (_instance == null)
            _instance = this;
        else Destroy(this.gameObject);
    }
    
    private void Start()
    {
        foreach(Audio s in soundClips)
        {
            s._source = gameObject.AddComponent<AudioSource>();

            s._source.clip = s.clip;

            s._source.volume = s.volume;
            s._source.pitch = s.pitch;

            s._source.loop = s.loop;
            s._source.playOnAwake = s.playOnAwake;

            if (s._source.playOnAwake)
                s._source.Play();
        }

        foreach(AudioRandList s in audioClipLists)
        {
            s._source = gameObject.AddComponent<AudioSource>();

            int clipNumber = UnityEngine.Random.Range(0, s.clipList.Length);
            s._source.clip = s.clipList[clipNumber];

            s._source.volume = s.volume;
            s._source.pitch = s.pitch;

            s._source.loop = s.loop;
            s._source.playOnAwake = s.playOnAwake;

            if (s._source.playOnAwake)
                s._source.Play();
        }

        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
    }

    public void playSound(string soundName)
    {
        Audio sound = Array.Find(soundClips, sound => sound.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning("AudioError. Could not find sound " + soundName + ".");
        }    
        sound._source.Play();
    }

    public void stopSound(string soundName)
    {
        Audio sound = Array.Find(soundClips, sound => sound.name == soundName);
        sound._source.Stop();
    }

    public void playListSound(string soundName)
    {
        AudioRandList sound = Array.Find(audioClipLists, sound => sound.name == soundName);

        int clipNumber = UnityEngine.Random.Range(0, sound.clipList.Length);
        sound._source.clip = sound.clipList[clipNumber];

        sound._source.Play();
    }

    public void stopPlayListSound(string soundName)
    {
        AudioRandList sound = Array.Find(audioClipLists, sound => sound.name == soundName);
        sound._source.Stop();
    }

    public void playSoundTrack(string soundName)
    {
        MusicTracks music = Array.Find(musicList, music => music.name == soundName);
        musicSource.clip = music.clip;
        musicSource.volume = music.volume;
        musicSource.pitch = music.pitch;

        musicSource.Play();
    }

    public void stopSoundTrack()
    {
        musicSource.Stop();
    }
}