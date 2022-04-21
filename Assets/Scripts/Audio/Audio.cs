using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Audio
{
    public string name;
    public AudioClip clip;

    [Space(5)]
    [Range(0, 1)]
    public float volume;
    [Range(1, 3)]
    public float pitch;

    [Space(5)]
    public bool loop;
    public bool playOnAwake;

    [HideInInspector]
    public AudioSource _source;
}

[System.Serializable]
public class AudioRandList
{
    public string name;
    public AudioClip[] clipList;

    [Space(5)]
    [Range(0, 1)]
    public float volume;
    [Range(1, 3)]
    public float pitch;

    [Space(5)]
    public bool loop;
    public bool playOnAwake;

    [HideInInspector]
    public AudioSource _source;
}

[System.Serializable]
public class MusicTracks
{
    public string name;
    public AudioClip clip;

    [Range(0, 1)]
    public float volume;
    [Range(1, 3)]
    public float pitch;
}