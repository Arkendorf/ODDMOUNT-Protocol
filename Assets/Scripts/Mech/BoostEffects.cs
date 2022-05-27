using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostEffects : MonoBehaviour
{
    public MechController mechController;
    public AudioClip boostSound;
    public AudioManager audioManager;
    public List<ParticleSystem> systems;

    private AudioSource boostAudio;
    private float boostNoiseReduction = 16f;
    private float boostPitchScale = .5f;
    private float boostFadeSpeed = 8;
    private float goalBoostVolume;

    private void OnEnable()
    {
        if (!mechController)
            mechController = GetComponent<MechController>();

        if (!audioManager)
            audioManager = GetComponent<AudioManager>();

        mechController.OnBoostStart += OnBoostStart;
        mechController.OnBoostStop += OnBoostStop;
    }

    private void OnDisable()
    {
        mechController.OnBoostStart -= OnBoostStart;
        mechController.OnBoostStop -= OnBoostStop;
    }

    void Update()
    {
        if (boostAudio)
        {
            goalBoostVolume = mechController.mech.velocity.magnitude / boostNoiseReduction;

            boostAudio.volume += (goalBoostVolume - boostAudio.volume) * Time.deltaTime * boostFadeSpeed;
            boostAudio.pitch = 1 + boostAudio.volume * boostPitchScale;

            goalBoostVolume -= Time.deltaTime * boostFadeSpeed;
            if (!mechController.boosting && boostAudio.volume <= 0)
            {
                audioManager.Stop(boostAudio);
            }
        }
    }

    private void OnBoostStart()
    {
        if (!boostAudio)
        {
            boostAudio = audioManager.Play(boostSound, true, .1f);
            boostAudio.time = Random.Range(0, boostAudio.clip.length - .0001f);
        }         

        foreach (ParticleSystem system in systems)
        {
            system.Play();
        }
    }

    private void OnBoostStop()
    {
        foreach (ParticleSystem system in systems)
        {
            system.Stop();
        }
    }
}
