using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Music
{
    public string TrackName;

    [Space(10)]
    public bool constant;                   //Constant lyder spiller hele tiden. Hvis en lyd ikke er constant er ideen at den byttes ut basert på rommet karakteren er i
    public AudioClip track;

    [Space(10)]
    [Range(0, 1)]
    public float volume;
    [Range(-3, 3)]
    public float pitch;

    [HideInInspector]
    public AudioSource _source;
}
