using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyController : MonoBehaviour
{
    [Tooltip("Transform to attempt to match")]
    public Transform target;
    [Space]
    public bool alignX = true;
    public bool alignY = true;
    public bool alignZ = true;
    public float moveSpeed = 1000;
    public float moveDamping = 50;
    public float maxForce = Mathf.Infinity;
    [Space]
    public bool alignForward = true;
    public bool alignUp = true;
    public bool alignRight = true;
    public float rotateSpeed = 1;
    public float rotateDamping = 5;
    public float maxTorque = Mathf.Infinity;
    public float maxAngularVelocity = 7;
    [Space]
    [Range(0, 1)]
    public float gravityResistance = 0;

    private new Rigidbody rigidbody;

    public delegate void CollisionEvents(Collision collision);
    public event CollisionEvents CollisionEnter;
    public event CollisionEvents CollisionExit;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = maxAngularVelocity;
    }

    private void FixedUpdate()
    {
        CorrectPosition();
        CorrectRotation();
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionExit?.Invoke(collision);
    }

    public void CorrectPosition()
    {
        // damping prevents overshooting the target position
        Vector3 force = -moveDamping * rigidbody.velocity;

        Vector3 delta = Vector3.zero;
        if (target)
        {
            delta = target.position - transform.position;

            if (!alignX)
            {
                delta.x = 0;
                force.x = 0; // Don't damp unaligned axes
            }
            if (!alignY)
            {
                delta.y = 0;
                force.y = 0;
            }
            if (!alignZ)
            {
                delta.z = 0;
                force.z = 0;
            }
        }          

        force += delta * moveSpeed;

        // Correct against gravity
        if (rigidbody.useGravity)
        {
            force -= rigidbody.mass * Physics.gravity * gravityResistance;
        }

        // Cap force
        if (force.sqrMagnitude > maxForce * maxForce)
        {
            force = force.normalized * maxForce;
        }

        rigidbody.AddForce(force);
    }

    public void CorrectRotation()
    {
        // damping prevents overshooting the target rotation
        Vector3 torque = -rotateDamping * rigidbody.angularVelocity;

        // Align directions
        if (target)
        {
            if (alignForward)
                torque += AlignVectors(transform.forward, target.forward);
            if (alignUp)
                torque += AlignVectors(transform.up, target.up);
            if (alignRight)
                torque += AlignVectors(transform.right, target.right);
        }

        // Cap torque
        if (torque.sqrMagnitude > maxTorque * maxTorque)
        {
            torque = torque.normalized * maxTorque;
        }

        // Apply torque
        rigidbody.AddTorque(torque);
    }

    private Vector3 AlignVectors(Vector3 current, Vector3 goal)
    {
        // Align y
        Quaternion delta = Quaternion.FromToRotation(current, goal);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        return axis.normalized * angle * rotateSpeed;
    }
}