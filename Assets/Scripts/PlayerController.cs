using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MechController))]
public class PlayerController : MonoBehaviour
{
    public CounterController healthCounter;
    public CounterController boostCounter;
    public float warningThreshold = .25f;
    public LightingEffect lightingEffect;
    public GameObject alarm;
    public AudioSource alarmAudio;

    private MechController playerMech;
    private float warningInterval;
    private float warningDelay;

    private void Start()
    {
        if (healthCounter)
        {
            healthCounter.percent = true;
            healthCounter.SetValue(100);
        }

        if (boostCounter)
        {
            boostCounter.percent = true;
            boostCounter.SetValue(100);
        }

        warningInterval = alarmAudio.clip.length / 8;
    }

    private void OnEnable()
    {
        playerMech = GetComponent<MechController>();

        playerMech.OnDeath += OnDeath;
        playerMech.OnTakeDamage += OnTakeDamage;      
    }

    private void OnDisable()
    {
        playerMech.OnDeath -= OnDeath;
        playerMech.OnTakeDamage -= OnTakeDamage;
    }

    private void Update()
    {
        if (boostCounter && playerMech.boosting)
        {
            boostCounter.SetValue((int)(playerMech.fuel * 100 / playerMech.maxFuel));
        }

        // Flash lights when damaged
        if (playerMech.health / playerMech.maxHealth < warningThreshold)
        {
            if (!alarm.activeSelf)
                alarm.SetActive(true);

            if (warningDelay <= 0)
            {
                lightingEffect.ToggleLight();
                warningDelay = warningInterval;
            }
            warningDelay -= Time.deltaTime;
        }
    }

    private void OnDeath()
    {
        // Disable input
        GetComponent<MechPlayerInput>().enabled = false;

        // Format weapons 
        foreach (Weapon weapon in GetComponentsInChildren<Weapon>())
        {
            weapon.enabled = false;
        }

        // Format controls 
        foreach (PhysicalControl control in GetComponentsInChildren<PhysicalControl>())
        {
            control.enabled = false;
        }

        // Disable mech controller
        playerMech.enabled = false;

    }

    private void OnTakeDamage(MechController.MechDamageType damageType)
    {
        if (healthCounter)
            healthCounter.SetValue((int)(playerMech.health * 100 / playerMech.maxHealth));
    }
}
