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
        // Delete input
        Destroy(GetComponent<MechPlayerInput>());

        // Disable mech controller
        playerMech.enabled = false;

    }
}
