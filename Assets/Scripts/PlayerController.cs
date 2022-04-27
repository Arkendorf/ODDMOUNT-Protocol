using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MechController))]
public class PlayerController : MonoBehaviour
{
    private MechController playerMech;

    private void OnEnable()
    {
        playerMech = GetComponent<MechController>();

        playerMech.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        playerMech.OnDeath -= OnDeath;
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
}
