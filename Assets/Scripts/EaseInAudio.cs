using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EaseInAudio : MonoBehaviour
{

    public AudioSource backgroundMusic;

    [Range(0f, 10f)] public float easeTime;


    private void Awake()
    {
        backgroundMusic.volume = 0;
    }

    void Start()
    {
        StartCoroutine(MoveToPosition(backgroundMusic, easeTime));
    }

    public IEnumerator MoveToPosition(AudioSource audiosource, float easetime)
    {
        var t = 0f;
        while(audiosource.volume <= 1)
        {
            t += Time.deltaTime / easetime;
            audiosource.volume = Mathf.Lerp(0, 1, t);
            yield return null;
        }
    }
}
