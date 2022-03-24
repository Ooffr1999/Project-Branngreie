using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public Music[] tracks;

    [HideInInspector]
    public static MusicManager _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(this);
        else _instance = this;

        foreach(Music m in tracks)
        {
            m._source = gameObject.AddComponent<AudioSource>();

            m._source.clip = m.track;

            m._source.volume = m.volume;
            m._source.pitch = m.pitch;

            m._source.loop = true;

            if (m.constant)
                m._source.Play();
        }
    }

    public void PlayTrack(string trackName)
    {
        Music track = GetTrack(trackName);

        track._source.volume = track.volume;
        track._source.pitch = track.pitch;

        track._source.Play();
    }

    public void StopTempTrack()
    {
        Music track = GetPlayingTempTrack();

        if (track != null)
            track._source.Stop();
    }

    public Music GetPlayingTempTrack()
    {
        Music track = Array.Find(tracks, track => track._source.isPlaying && !track.constant);

        if (track == null)
        {
            Debug.LogError("Could not find track playing. Are any temporary tracks playing at all?");
            return null;
        }
        else return track;
    }

    #region MusicFade
    //Fade in music
    public void FadeInMusic(string trackName, float fadeTime)
    {
        StartCoroutine(FadeInMusic(GetTrack(trackName), fadeTime));
    }

    //Fade out music
    public void FadeOutMusic(string trackName, float fadeTime)
    {
         StartCoroutine(FadeToStop(GetTrack(trackName), fadeTime));
    }

    //FadeBetweenTracks
    public void FadeBetweenTracks(string oldTrackName, string newTrackName, float fadeTime)
    {
        StartCoroutine(FadeBetweenTracks(GetTrack(oldTrackName), GetTrack(newTrackName), fadeTime));    
    }
    #endregion

    #region MusicFade Couroutines
    IEnumerator FadeInMusic(Music track, float fadeTime)
    {
        track._source.volume = 0;
        track._source.Play();

        while(true)
        {
            if (track._source.volume >= 1)
                break;

            track._source.volume += Time.deltaTime / fadeTime;

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeToStop(Music track, float fadeTime)
    {
        while(true)
        {
            if (track._source.volume <= 0)
                break;

            track._source.volume -= Time.deltaTime / fadeTime;

            yield return new WaitForEndOfFrame();
        }

        track._source.Stop();
    }

    IEnumerator FadeBetweenTracks(Music oldTrack, Music newTrack, float fadeTime)
    {
        newTrack._source.volume = 0;
        newTrack._source.Play();

        while(true)
        {
            if (oldTrack._source.volume <= 0 && newTrack._source.volume >= 1)
                break;

            float timeStep = Time.deltaTime / fadeTime;

            oldTrack._source.volume -= timeStep;
            newTrack._source.volume += timeStep;

            yield return new WaitForEndOfFrame();
        }

        oldTrack._source.Stop();
    }
    #endregion

    Music GetTrack(string name)
    {
        Music track = Array.Find(tracks, track => track.TrackName == name);

        if (track == null)
        {
            Debug.LogError("Couldn't find Music track " + track + ". Are you sure you spelled it right");
            return null;
        }
        else return track;
    }
}
