using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MechController))]
public class EnemyController : MonoBehaviour
{
    // Local mech controller
    private MechController mechController;

    private void OnEnable()
    {
        // Get a reference to the mech controller
        mechController = GetComponent<MechController>();

        // Subscribe to the death event
        mechController.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        mechController.OnDeath -= OnDeath;
    }


    // On death, remove this enemy
    private void OnDeath()
    {
        EnemyManager.Instance.RemoveEnemy(mechController);
    }
}
