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
    private float totalMass;

    public delegate void CollisionEvents(Collision collision);
    public event CollisionEvents CollisionEnter;
    public event CollisionEvents CollisionExit;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = maxAngularVelocity;

        totalMass = GetTotalMass();
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

        Vector3 delta = target.position - transform.position;

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

        force += delta * moveSpeed;

        // Correct against gravity
        if (rigidbody.useGravity)
        {
            force -= totalMass * Physics.gravity * gravityResistance;
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
        if (alignForward)
            torque += AlignVectors(transform.forward, target.forward);
        if (alignUp)
            torque += AlignVectors(transform.up, target.up);
        if (alignRight)
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
        return axis.normalized * angle * rotateSpeed;
    }

    // Returns aggregate mass of this rigidbody and all rigidbodies attached to thos one via joints (as long as joints are in siblings or descendents)
    private float GetTotalMass()
    {
        float totalMass = 0;
        Dictionary<Rigidbody, bool> seen = new Dictionary<Rigidbody, bool>();

        if (transform.parent)
        {
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                Rigidbody childRigidbody = transform.parent.GetChild(i).GetComponent<Rigidbody>();
                if (childRigidbody)
                {
                    totalMass += GetTotalMassRecursive(childRigidbody, seen);
                }
            }
        }
        else
        {
            totalMass += GetTotalMassRecursive(rigidbody, seen);
        } 

        return totalMass;
    }

    // Returns aggregate mass of the given rigidbody and all rigidbodies attached to given one via joints (as long as joints are in descendents)
    private float GetTotalMassRecursive(Rigidbody rigidbody, Dictionary<Rigidbody, bool> seen)
    {
        float totalMass = 0;

        if (!seen.ContainsKey(rigidbody))
        {
            totalMass += rigidbody.mass;
            seen[rigidbody] = true;

            Joint[] joints = rigidbody.GetComponentsInChildren<Joint>();
            foreach (Joint joint in joints)
            {
                Rigidbody jointRigidbody = joint.GetComponent<Rigidbody>();
                if (jointRigidbody == rigidbody)
                {
                    totalMass += GetTotalMassRecursive(joint.connectedBody, seen);
                }
                if (joint.connectedBody == rigidbody)
                {
                    totalMass += GetTotalMassRecursive(jointRigidbody, seen);
                }
            }
        }       

        return totalMass;
    }
}