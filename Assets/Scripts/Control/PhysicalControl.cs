using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalControl : MonoBehaviour
{
    [Header("Control Properties")]
    [Tooltip("Mech to control")]
    public MechController mechController;
    [Tooltip("How fast this control moves")]
    public float moveSpeed = 12;
    [Tooltip("Audio source")]
    public new AudioSource audio;

    public AudioClip moveSound;
    public AudioClip stopSound;

    protected void PlayStopSound(float volume, AudioSource audio = null)
    {
        if (!audio)
            audio = this.audio;

        audio.pitch = Random.Range(0.75f, 1.2f);
        audio.volume = Mathf.Min(volume, 1);
        audio.loop = false;
        audio.time = 0;
        audio.clip = stopSound;
        audio.Play();
    }

    protected void PlayMoveSound(AudioSource audio = null)
    {
        if (!audio)
            audio = this.audio;

        if (audio.clip != moveSound || !audio.isPlaying)
        {
            audio.loop = true;
            audio.pitch = 1;
            audio.volume = 1;
            audio.time = 0;
            audio.clip = moveSound;
            audio.Play();
        }
    }

    protected void UpdateMoveSound(float moveAmount, AudioSource audio = null)
    {
        if (!audio)
            audio = this.audio;

        if (audio.clip == moveSound && audio.isPlaying)
        { 
            audio.pitch = moveAmount;
            audio.volume = moveAmount;
        }
    }

    protected void StopMoveSound(AudioSource audio = null)
    {
        if (!audio)
            audio = this.audio;

        if (audio.clip == moveSound && audio.isPlaying)
        {
            audio.Stop();
            audio.pitch = 1;
            audio.volume = 1;
            audio.loop = false;
        }
    }
}


