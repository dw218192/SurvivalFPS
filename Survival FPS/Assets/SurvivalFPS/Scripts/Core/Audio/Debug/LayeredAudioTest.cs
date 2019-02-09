using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurvivalFPS.Core.Audio;

public class LayeredAudioTest : MonoBehaviour
{
    public bool play1;
    public bool play2;
    public bool play3;
    public bool play4;
    public bool play5;
    public bool play6;
    public bool play6_2;

    public AudioCollection audioCollection;
    public AudioManager.ILayeredAudioSource layeredAudioSource; 

    private void Start()
    {
        layeredAudioSource = AudioManager.Instance.RegisterLayeredAudioSource(gameObject, 6, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (play1)
            layeredAudioSource.Play(audioCollection, 0, 0, false);
        if (play2) 
            layeredAudioSource.Play(audioCollection, 1, 1, false);
        if (play3) 
            layeredAudioSource.Play(audioCollection, 2, 2, false);
        if (play4) 
            layeredAudioSource.Play(audioCollection, 3, 3, false);
        if (play5) 
            layeredAudioSource.Play(audioCollection, 4, 4, false);
        if (play6) 
            layeredAudioSource.Play(audioCollection, 5, 5, false);
        if (play6_2) 
            layeredAudioSource.Play(audioCollection, 0, 5, false);

        play1 = false;
        play2 = false;
        play3 = false;
        play4 = false;
        play5 = false;
        play6 = false;
        play6_2 = false;
    }
}
