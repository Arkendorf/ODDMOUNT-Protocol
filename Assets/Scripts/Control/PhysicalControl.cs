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
    public float speedThreshold = .01f;
    public float moveVolumeScale = 1;
    public float movePitchScale = 1;
    public float moveBaseVolume = 0;
    [Space()]
    [Tooltip("Sound to play when the control stops moving, or null for no sound")]
    public AudioClip stopSound;
    public float stopBaseVolume = .5f;
    public float stopVolumeScale = 1;


    // Current move speed of this control
    protected float currentSpeed;
    // If stop sound is allowed
    protected bool stopAllowed = true;
    // Audio source playing the move sound
    private AudioSource moveSource;
    // Stop sound ids
    private Dictionary<int, bool> idPlayed;

    protected virtual void Start()
    {
        idPlayed = new Dictionary<int, bool>();
    }

    protected virtual void Update()
    {
        UpdateMoveAudio(audioManager, currentSpeed, ref moveSource);
    }

    protected AudioSource UpdateMoveAudio(AudioManager audioManager, float currentSpeed, ref AudioSource moveSource)
    {
        // Control audio
        if (audioManager)
        {
            // Check if control is moving
            if (currentSpeed > speedThreshold)
            {
                if (moveSound)
                {
                    // If move sound isn't yet playing, play it
                    if (!moveSource)
                        moveSource = audioManager.Play(moveSound, true);

                    // Set move sound's pitch and speed
                    moveSource.volume = moveBaseVolume + (currentSpeed - speedThreshold) * moveVolumeScale;
                    moveSource.pitch = (currentSpeed - speedThreshold) * movePitchScale;
                }
            }
            else if (moveSource) // If control just stopped moving
            {
                audioManager.Stop(moveSource);
                return null;
            }
        }

        return moveSource;
    }

    protected void PlayStopSound(int id)
    {
        PlayStopSound(id, currentSpeed);
    }

    protected void PlayStopSound(int id, float currentSpeed)
    {
        if (stopSound && (!idPlayed.ContainsKey(id) || !idPlayed[id]))
        {
            audioManager.Play(stopSound, false, stopBaseVolume + currentSpeed * stopVolumeScale, Random.Range(0.75f, 1.2f));
            idPlayed[id] = true;
        }
    }

    protected void AllowStopSound(int id)
    {
        idPlayed[id] = false;
    }
}


