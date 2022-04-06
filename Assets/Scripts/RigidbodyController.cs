using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyController : MonoBehaviour
{
    [Tooltip("Transform to attempt to match")]
    public Transform target;
    [Space]
    public float maxForce;
    [Space]
    public float alignmentSpeed;
    public float alignmentDamping;
    public float maxTorque;
    public float maxAngularVelocity;
    [Space]
    public bool correctGravity;
    [Range(0, 1)]
    public float correctionPercent = 1;

    private new Rigidbody rigidbody;

    public delegate void CollisionEvents();
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
        CollisionEnter?.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionExit?.Invoke();
    }

    public void CorrectPosition()
    {
        Vector3 force = Vector3.zero;

        // Correct against gravity
        if (correctGravity && rigidbody.useGravity)
        {
            force -= rigidbody.mass * Physics.gravity * correctionPercent;
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
        // alignmentDamping prevents overshooting the target rotation
        Vector3 torque = -alignmentDamping * rigidbody.angularVelocity;

        // Align directions
        torque += AlignVectors(transform.forward, target.forward);
        torque += AlignVectors(transform.up, target.up);
        torque += AlignVectors(transform.right, target.right);

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
        return axis.normalized * angle * alignmentSpeed;
    }
}