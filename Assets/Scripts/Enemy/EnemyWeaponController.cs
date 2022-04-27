using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RigidbodyController))]
public class EnemyWeaponController : MonoBehaviour
{
    [Tooltip("The mech this belongs to")]
    public MechController mechController;
    [Tooltip("The input with the target for this weapon")]
    public MechNavMeshInput input;
    [Tooltip("How fast the weapon rotates")]
    public float rotateSpeed = 30;

    // Weapon's current angle
    private float currentAngle;

    // The local rigidbody controller
    private RigidbodyController rigidbodyController;

    void Start()
    {
        // Get the local rigidbody controller
        rigidbodyController = GetComponent<RigidbodyController>();

        currentAngle = transform.localEulerAngles.x;
    }

    void Update()
    {
        // Get height offset
        float height = input.target.position.y - mechController.mech.position.y;

        // Get distance
        float length = input.distance;

        // Get goal angle
        float goalAngle = -Mathf.Atan2(height, length) * Mathf.Rad2Deg;

        // Get rotation options
        float rightAngle = LoopAngle(goalAngle - currentAngle);
        float leftAngle = LoopAngle(goalAngle - currentAngle - 360);
        // Get best rotation
        float angle = Mathf.Abs(leftAngle) < Mathf.Abs(rightAngle) ? leftAngle : rightAngle;
        
        if (Mathf.Abs(angle) > rotateSpeed * Time.deltaTime)
        {
            currentAngle += Mathf.Sign(angle) * rotateSpeed * Time.deltaTime;
        }
        else
        {
            currentAngle = goalAngle;
        }

        // Set rotation of target
        rigidbodyController.target.localRotation = Quaternion.Euler(currentAngle, 0, 0);
    }

    // Loop an angle
    private float LoopAngle(float angle)
    {
        if (angle > 180)
            return angle - 360;
        else if (angle < -180)
            return angle + 360;
        else
            return angle;
    }
}
