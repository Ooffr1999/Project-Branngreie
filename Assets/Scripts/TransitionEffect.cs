using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TransitionEffect : MonoBehaviour
{
    public Image transitionImage;

    public void Play()
    {
        StartCoroutine(fadeIn(1));
    }

    public void Play(float fadeTime)
    {
        StartCoroutine(fadeIn(fadeTime));
    }

    public void Play(UnityEvent fadeEvent)
    {
        StartCoroutine(fadeIn(1, fadeEvent));
    }

    public void Play(float fadeTime, UnityEvent fadeEvent)
    {
        StartCoroutine(fadeIn(fadeTime, fadeEvent));
    }

    IEnumerator fadeIn(float fadeTime)
    {
        Color tempColor = transitionImage.color;

        while(true)
        {
            if (tempColor.a >= 1f)
                break;

            yield return new WaitForEndOfFrame();

            tempColor.a += Time.deltaTime / (fadeTime / 2);
            transitionImage.color = tempColor;
        }

        StartCoroutine(fadeOut(fadeTime));
    }

    IEnumerator fadeIn(float fadeTime, UnityEvent fadeEvent)
    {
        Color tempColor = transitionImage.color;

        while (true)
        {
            if (tempColor.a >= 1f)
                break;

            yield return new WaitForEndOfFrame();

            tempColor.a += Time.deltaTime / (fadeTime / 2);
            transitionImage.color = tempColor;
        }

        fadeEvent.Invoke();
        StartCoroutine(fadeOut(fadeTime));
    }

    IEnumerator fadeOut(float fadeTime)
    {
        Color tempColor = transitionImage.color;

        while (true)
        {
            if (tempColor.a <= 0)
                break;

            yield return new WaitForEndOfFrame();

            tempColor.a -= Time.deltaTime / (fadeTime / 2);
            transitionImage.color = tempColor;
        }
    }
}
