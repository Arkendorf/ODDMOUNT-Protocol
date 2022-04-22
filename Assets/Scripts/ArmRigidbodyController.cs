using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmRigidbodyController : RigidbodyController
{
    [Space]
    public Transform anchorParent;
    public Vector3 anchorVector;
    public float anchorRange;

    private float delta = .1f;

    protected override Quaternion GetTargetRotation()
    {
        Quaternion targetRotation = target.rotation;
        Vector3 anchorVector = this.anchorVector;
        // Rotate anchor vector by parent if it exists
        if (anchorParent)
        {
            anchorVector = anchorParent.rotation * anchorVector;
        }
        // Cap target rotation to outside the anchor vector forbidden zone
        if (Vector3.Angle(targetRotation * Vector3.forward, anchorVector) <= anchorRange)
        {
            targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(anchorRange, Vector3.Cross(anchorVector, targetRotation * Vector3.forward)) * anchorVector);
        }
        return targetRotation;
    }

    // Update is called once per frame
    protected override Vector3 GetTorque(Quaternion targetRotation)
    {
        Vector3 torque = Vector3.zero;

        if (target)
        {
            // Get current rotation, and goal rotation
            Quaternion currentRotation = transform.rotation;

            Vector3 anchorVector = this.anchorVector;

            // Rotate anchor vector by parent if it exists
            if (anchorParent)
            {
                anchorVector = anchorParent.rotation * anchorVector;
            }

            // Cap target rotation to outside the anchor vector forbidden zone
            if (Vector3.Angle(targetRotation * Vector3.forward, anchorVector) <= anchorRange)
            {
                targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(anchorRange, Vector3.Cross(anchorVector, targetRotation * Vector3.forward)) * anchorVector);
            }

#if UNITY_EDITOR
            // Debug stuff
            Debug.DrawLine(transform.position, transform.position + currentRotation * Vector3.forward, Color.blue);
            Debug.DrawLine(transform.position, transform.position + targetRotation * Vector3.forward, Color.red);
            Debug.DrawLine(transform.position, transform.position + anchorVector, Color.black);
#endif

            // Get angle between current and target
            float deltaAngle = Vector3.Angle(currentRotation * Vector3.forward, targetRotation * Vector3.forward);

            // Get offset
            Vector3 axis = Vector3.Cross(currentRotation * Vector3.forward, targetRotation * Vector3.forward);
            Vector3 right = Vector3.Cross(anchorVector, axis);
            Vector3 unclampedAnchorVector = Vector3.Cross(axis, Vector3.Cross(anchorVector, axis));

            // Clamp offset to range
            float offsetAngle = Vector3.SignedAngle(anchorVector, unclampedAnchorVector, right);
            offsetAngle = Mathf.Clamp(offsetAngle, -anchorRange, anchorRange);

            // Adjust anchor vector by offset
            Vector3 offsetAnchorVector = Quaternion.AngleAxis(offsetAngle, right) * anchorVector;

            // Angle between current and target, crossing through clamped anchor vector
            float crossAngle = Vector3.Angle(currentRotation * Vector3.forward, offsetAnchorVector) + Vector3.Angle(offsetAnchorVector, targetRotation * Vector3.forward);

            // Check if angle is crossing through forbidden anchor vector zone
            bool reverse = crossAngle - delta <= deltaAngle;

            // Calculate torque (only one alignment should reverse at a time)
            if (alignForward)
                torque += AlignVectors(currentRotation * Vector3.forward, targetRotation * Vector3.forward, reverse);

            if (alignUp && !alignForward && !alignRight)
                torque += AlignVectors(currentRotation * Vector3.up, targetRotation * Vector3.up, reverse);
            else if (alignUp)
                torque += AlignVectors(currentRotation * Vector3.up, targetRotation * Vector3.up);

            if (alignRight && !alignForward)
                torque += AlignVectors(currentRotation * Vector3.right, targetRotation * Vector3.right, reverse);
            else if (alignRight)
                torque += AlignVectors(currentRotation * Vector3.right, targetRotation * Vector3.right);
        }

        // Return the calculated torque
        return torque;
    }
}
