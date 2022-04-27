using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MechController))]
public class EnemyController : MonoBehaviour
{
    [Tooltip("The base rigidbody of this mech")]
    public Rigidbody baseRigidbody;
    [Tooltip("Max distance between body and base when they ragdoll")]
    public float ragdollSprawl = 2;
    [Tooltip("Mass of the base rigidbody when it becomes a ragdoll")]
    public float ragdollHeaviness = 500;
    [Tooltip("Layer to move this mech to when it becomes a ragdoll")]
    public int ragdollLayer = 9;

    // Local mech controller
    private MechController mechController;

    public bool kill;

    private void Update()
    {
        if (kill & mechController.health > 0)
        {
            mechController.AddDamage(mechController.health);
        }
    }

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
        // Remove enemy
        EnemyManager.Instance.RemoveEnemy(mechController, false);

        // Recursively change things necessary for ragdoll 
        SetLayerRecursive(gameObject, ragdollLayer);

        // Format rigidbody controllers
        foreach (RigidbodyController rigidbodyController in GetComponentsInChildren<RigidbodyController>())
        {
            rigidbodyController.enabled = false;
        }

        // Format rigidbodies
        foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }
        // Configure base rigidbody
        baseRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        baseRigidbody.mass = ragdollHeaviness;

        // Adjust waist joint to add in funky movement
        ConfigurableJoint joint = mechController.mech.GetComponent<ConfigurableJoint>();
        // Allow joint to move in all directions
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        // Remove spring force on waist
        JointDrive drive = joint.yDrive;
        drive.positionSpring = 0;
        drive.positionDamper = 0;
        joint.yDrive = drive;
        // Set limit for how far body can stray from base
        SoftJointLimit limit = joint.linearLimit;
        limit.limit = ragdollSprawl;
        joint.linearLimit = limit;
        // Allow collision with base
        joint.enableCollision = true;

        // Disable nav agent
        GetComponent<NavMeshAgent>().enabled = false;

        // Disable mech input
        MechNavMeshInput input = GetComponent<MechNavMeshInput>();
        input.Stop();
        input.enabled = false;

        // Disable mech controller
        mechController.enabled = false;
    }

    private void SetLayerRecursive(GameObject gameObject, int layer)
    {
        // Change layer to ragdoll layer
        gameObject.layer = layer;

        // Call on children
        foreach (Transform child in gameObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}
