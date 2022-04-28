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
    [Header("Audio Properties")]
    [Tooltip("Audio manager")]
    public AudioManager audioManager;
    [Space()]
    [Tooltip("Sound to play while this control is moving, or null for no sound")]
    public AudioClip moveSound;
    public float moveVolumeScale = 1;
    public float movePitchScale = 1;
    [Space()]
    [Tooltip("Sound to play when the control stops moving, or null for no sound")]
    public AudioClip stopSound;
    public float stopVolumeScale = 1;

    // Current move speed of this control
    protected float currentSpeed;
    // Previous move speed of the control
    private float prevSpeed;
    // Audio source playing the move sound
    private AudioSource moveSource;

    protected virtual void Update()
    {
        prevSpeed = UpdateAudioManager(audioManager, currentSpeed, prevSpeed);
    }

    protected float UpdateAudioManager(AudioManager audioManager, float currentSpeed, float prevSpeed)
    {
        // Control audio
        if (audioManager)
        {
            // Check if control is moving
            if (currentSpeed > 0)
            {
                if (moveSound)
                {
                    // If move sound isn't yet playing, play it
                    if (!moveSource)
                        moveSource = audioManager.Play(moveSound, true);

                    // Set move sound's pitch and speed
                    moveSource.volume = currentSpeed * moveVolumeScale;
                    moveSource.pitch = currentSpeed * movePitchScale;
                }
            }
            else if (prevSpeed > 0) // If control just stopped moving
            {
                // Stop the move sound
                if (moveSource)
                    audioManager.Stop(moveSource);

                // Play the stop sound
                if (stopSound)
                    audioManager.Play(stopSound, false, prevSpeed * stopVolumeScale, Random.Range(0.75f, 1.2f));
            }
        }

        return currentSpeed;
    }

    //protected void PlayStopSound(float volume, AudioSource audio = null)
    //{
    //    if (!audio)
    //        audio = this.audio;

    //    audio.pitch = Random.Range(0.75f, 1.2f);
    //    audio.volume = Mathf.Min(volume, 1);
    //    audio.loop = false;
    //    audio.time = 0;
    //    audio.clip = stopSound;
    //    audio.Play();
    //}

    //protected void PlayMoveSound(AudioSource audio = null)
    //{
    //    if (!audio)
    //        audio = this.audio;

    //    if (audio.clip != moveSound || !audio.isPlaying)
    //    {
    //        audio.loop = true;
    //        audio.pitch = 1;
    //        audio.volume = 1;
    //        audio.time = 0;
    //        audio.clip = moveSound;
    //        audio.Play();
    //    }
    //}

    //protected void UpdateMoveSound(float moveAmount, AudioSource audio = null)
    //{
    //    if (!audio)
    //        audio = this.audio;

    //    if (audio.clip == moveSound && audio.isPlaying)
    //    { 
    //        audio.pitch = moveAmount;
    //        audio.volume = moveAmount;
    //    }
    //}

    //protected void StopMoveSound(AudioSource audio = null)
    //{
    //    if (!audio)
    //        audio = this.audio;

    //    if (audio.clip == moveSound && audio.isPlaying)
    //    {
    //        audio.Stop();
    //        audio.pitch = 1;
    //        audio.volume = 1;
    //        audio.loop = false;
    //    }
    //}
}


