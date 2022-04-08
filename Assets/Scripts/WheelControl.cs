using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : MonoBehaviour
{
    [Header("Component Properties")]
    [Tooltip("Mech to control with this wheel")]
    public MechController mechController;
    [Tooltip("The shaft for this wheel control")]
    public Transform shaft;
    [Tooltip("The wheel object for this wheel control")]
    public Transform wheel;
    [Tooltip("The left input device on the wheel")]
    public InputDevice left;
    [Tooltip("The right input device on the wheel")]
    public InputDevice right;

    [Header("Move Properties")]
    [Header("Maximum angle wheel can be pushed forward")]
    public float maxXAngle = 30;
    [Header("Maximum angle wheel can be pulled backward")]
    public float minXAngle = -30;
    [Header("Maximum angle wheel can be pushed to the right")]
    public float maxZAngle = 30;
    [Header("Maximum angle wheel can be pushed to the left")]
    public float minZAngle = -30;

    [Header("Turn Properties")]
    public float maxWheelAngle = 30;
    public float minWheelAngle = -30;

    [Header("Wheel Properties")]
    public float moveSpeed = 8;

    // Default shaft angle
    private float defaultXAngle;
    private float defaultYAngle;
    private float defaultZAngle;

    private bool reset;
    private Quaternion goalShaftRotation;
    private Quaternion goalWheelRotation;

    //public Transform leftTran;
    //public Transform rightTran;

    // Start is called before the first frame update
    void Start()
    {
        defaultXAngle = shaft.eulerAngles.x;
        defaultYAngle = shaft.eulerAngles.y;
        defaultZAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // If wheel is grabbed
        if (left.interactable.isSelected || right.interactable.isSelected)
        {
            // Get position of left controller
            Vector3 leftPos = wheel.position;
            if (left.interactable.isSelected)
                leftPos = left.controller.transform.position;

            // Get position of right controller
            Vector3 rightPos = wheel.position;
            if (right.interactable.isSelected)
                rightPos = right.controller.transform.position;

            RotateShaft(leftPos, rightPos);
            RotateWheel(leftPos, rightPos);

            reset = false;
        }
        else if (!reset)
        {
            // Reset control
            // Reset shaft to neutral
            goalShaftRotation = Quaternion.Euler(defaultXAngle, defaultYAngle, defaultZAngle);
            // Reset wheel to neutral
            goalWheelRotation = Quaternion.LookRotation(Vector3.Cross(transform.right, -shaft.forward), -shaft.forward);

            // Reset mech
            if (mechController)
            {
                if (mechController.moving)
                    mechController.StopMove();
                if (mechController.turning)
                    mechController.StopTurn();
            }          

            reset = true;
        }

        // Lerp to goal rotation
        shaft.rotation = Quaternion.Lerp(shaft.rotation, goalShaftRotation, moveSpeed * Time.deltaTime);
        wheel.rotation = Quaternion.Lerp(wheel.rotation, goalWheelRotation, moveSpeed * Time.deltaTime);
    }

    private void RotateShaft(Vector3 leftPos, Vector3 rightPos)
    {
        // Get Euler angles
        Vector3 angles;
        if (left.interactable.isSelected && right.interactable.isSelected)
        {
            // Position average position of hands on the center of the wheel
            angles = GetGoalAngles((leftPos + rightPos) / 2, wheel.transform.position);
        }
        else if (right.interactable.isSelected)
        {
            // Position right hand on right control
            angles = GetGoalAngles(rightPos, right.transform.position);
        }
        else
        {
            // Position left hand on left control
            angles = GetGoalAngles(leftPos, left.transform.position);
        }

        // Cap angles
        if (angles.x > maxXAngle)
            angles.x = maxXAngle;
        else if (angles.x < minXAngle)
            angles.x = minXAngle;

        if (angles.z > maxZAngle)
            angles.z = maxZAngle;
        else if (angles.z < minZAngle)
            angles.z = minZAngle;

        // Rotate shaft
        goalShaftRotation = Quaternion.Euler(angles);

        // Move mech
        if (mechController)
        {
            // Create input
            Vector2 input = new Vector2();
            if (angles.z > defaultZAngle)
                input.x = -(angles.z - defaultZAngle) / (maxZAngle - defaultZAngle);
            else
                input.x = (angles.z - defaultZAngle) / (minZAngle - defaultZAngle);

            if (angles.x > defaultXAngle)
                input.y = (angles.x - defaultXAngle) / (maxXAngle - defaultXAngle);
            else
                input.y = -(angles.x - defaultXAngle) / (minXAngle - defaultXAngle);

            // Move the mech
            if (!mechController.moving)
                mechController.StartMove();
            mechController.Move(input);
        }
    }

    private Vector3 GetGoalAngles(Vector3 hand, Vector3 control)
    {
        Vector3 wheelOffset = control - shaft.position;
        Vector3 handOffset = hand - shaft.position;
        // Hand offset + wheel vertical offset + wheel horizontal offset;
        Vector3 target = handOffset - Vector3.Project(wheelOffset, shaft.forward) - Vector3.Project(wheelOffset, shaft.right);

        // Get new forward direction based on target
        Vector3 forward = Vector3.Cross(transform.right, target);
        // Get angles
        float xAngle = Vector3.SignedAngle(transform.forward, forward, transform.right);
        float zAngle = -Vector3.SignedAngle(Vector3.Cross(target, forward), transform.right, forward);

        return new Vector3(xAngle, defaultYAngle, zAngle);
    }

    private void RotateWheel(Vector3 leftPos, Vector3 rightPos)
    {
        // Control wheel rotation
        // Project controller positions to plane of wheel, and get the difference between the resulting positions 
        Vector3 delta = Vector3.ProjectOnPlane(rightPos, wheel.up) - Vector3.ProjectOnPlane(leftPos, wheel.up);
        float angle = Vector3.SignedAngle(transform.right, delta, wheel.up);

        if (angle > maxWheelAngle)
            angle = maxWheelAngle;
        else if (angle < minWheelAngle)
            angle = minWheelAngle;

        // Rotate wheel
        goalWheelRotation = Quaternion.LookRotation(Vector3.Cross(shaft.forward, transform.right), -shaft.forward) * Quaternion.Euler(0, angle, 0);

        // Unrestrained wheel rotation
        //goalWheelRotation = Quaternion.LookRotation(Vector3.Cross(delta, wheel.up), wheel.up);

        // Move mech
        if (mechController)
        {
            // Create input
            Vector2 input = new Vector2();
            if (angle > 0)
                input.x = angle / maxWheelAngle;
            else
                input.x = -angle / minWheelAngle;

            // Move the mech
            if (!mechController.turning)
                mechController.StartTurn();
            mechController.Turn(input);
        }
    }
}
