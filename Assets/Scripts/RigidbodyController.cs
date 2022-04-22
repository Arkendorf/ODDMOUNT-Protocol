using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyController : MonoBehaviour
{
    [Tooltip("Transform to attempt to match")]
    public Transform target;
    public bool resetParent = false;
    public bool ignoreCollision = true;
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

    protected new Rigidbody rigidbody;

    public delegate void CollisionEvents(Collision collision);
    public event CollisionEvents CollisionEnter;
    public event CollisionEvents CollisionExit;

    private Vector3 prevParentPosition;
    private Quaternion prevParentRotation;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = maxAngularVelocity;

        if (ignoreCollision)
        {
            Collider collider = GetComponent<Collider>();
            // Prevent collision with parent
            if (transform.parent)
            {
                Collider parent = transform.parent.GetComponent<Collider>();
                if (parent)
                    Physics.IgnoreCollision(collider, parent);
            }   
            // Prevent collision with children
            foreach(Collider child in GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, child);
            }
        }
        if (resetParent && transform.parent)
        {
            transform.parent = null;
        }
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
        Vector3 targetPosition = GetTargetPosition();

        Vector3 force = GetForce(targetPosition);

        // Add damping
        force += -moveDamping * rigidbody.velocity;
        // Don't damp unaligned axes
        if (!alignX)
            force.x = 0;
        if (!alignY)
            force.y = 0;
        if (!alignZ)
            force.z = 0;

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

    protected virtual Vector3 GetTargetPosition()
    {
        return target.position;
    }

    protected virtual Vector3 GetForce(Vector3 targetPosition)
    {
        Vector3 force = Vector3.zero;

        Vector3 delta = Vector3.zero;
        if (target)
        {
            delta = targetPosition - transform.position;
            // Don't add force for unaligned axes
            if (!alignX)
                delta.x = 0;
            if (!alignY)
                delta.y = 0;
            if (!alignZ)
                delta.z = 0;
        }

        force += delta * moveSpeed;

        return force;
    }

    public void CorrectRotation()
    {
        Quaternion targetRotation = GetTargetRotation();

        Vector3 torque = GetTorque(targetRotation);

        // Add damping if at least one axis is aligned
        if (alignForward || alignUp || alignRight)
            torque += -rotateDamping * rigidbody.angularVelocity;

        // Cap torque
        if (torque.sqrMagnitude > maxTorque * maxTorque)
        {
            torque = torque.normalized * maxTorque;
        }

        // Apply torque
        rigidbody.AddTorque(torque);
    }

    protected virtual Quaternion GetTargetRotation()
    {
        return target.rotation;
    }

    protected virtual Vector3 GetTorque(Quaternion targetRotation)
    {
        Vector3 torque = Vector3.zero;

#if UNITY_EDITOR
        // Debug stuff
        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.blue);
        Debug.DrawLine(transform.position, transform.position + target.forward, Color.red);
#endif

        // Align directions
        if (target)
        {
            if (alignForward)
                torque += AlignVectors(transform.forward, targetRotation * Vector3.forward);
            if (alignUp)
                torque += AlignVectors(transform.up, targetRotation * Vector3.up);
            if (alignRight)
                torque += AlignVectors(transform.right, targetRotation * Vector3.right);
        }

        return torque;
    }

    //protected Vector3 AlignVectors(Vector3 current, Vector3 goal, bool reverse = false)
    //{
    //    Quaternion delta = Quaternion.FromToRotation(current, goal);
    //    delta.ToAngleAxis(out float angle, out Vector3 axis);
    //    return axis.normalized * angle * rotateSpeed;
    //}

    protected Vector3 AlignVectors(Vector3 current, Vector3 goal, bool reverse = false)
    {
        Vector3 axis = Vector3.Cross(current, goal);
        float angle = Vector3.Angle(current, goal);
        if (reverse)
        {
            angle = (360 - angle) % 360;
            axis *= -1;
        }
        return axis.normalized * angle * rotateSpeed;
    }
}