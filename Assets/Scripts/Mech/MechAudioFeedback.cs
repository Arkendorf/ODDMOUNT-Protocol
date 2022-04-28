using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioManager))]
public class MechAudioFeedback : MonoBehaviour
{
    public MechController mechController;
    public AudioClip dealDamageClip;
    public AudioClip takeDamageClip;

    private AudioManager audioManager;

    private void OnEnable()
    {
        audioManager = GetComponent<AudioManager>();

        mechController.OnDealDamage += OnDealDamage;
        mechController.OnTakeDamage += OnTakeDamage;
    }

    private void OnDisable()
    {
        mechController.OnDealDamage -= OnDealDamage;
        mechController.OnTakeDamage -= OnTakeDamage;
    }

    private void OnDealDamage()
    {
        audioManager.Play(dealDamageClip, false, .5f);
    }

    private void OnTakeDamage()
    {
        audioManager.Play(takeDamageClip, false, .25f, Random.Range(0.75f, 1.2f));
    }
}
