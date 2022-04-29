using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MechController))]
public class PlayerController : MonoBehaviour
{
    public CounterController healthCounter;
    public CounterController boostCounter;

    private MechController playerMech;

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

    private void OnTakeDamage()
    {
        if (healthCounter)
            healthCounter.SetValue((int)(playerMech.health * 100 / playerMech.maxHealth));
    }
}
