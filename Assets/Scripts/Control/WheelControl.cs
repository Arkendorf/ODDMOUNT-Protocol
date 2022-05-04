using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : PhysicalControl
{
    [Space]
    [Tooltip("Audio source for the wheel")]
    public AudioManager wheelAudioManager;

    [Header("Component Properties")]
    [Tooltip("Nonessential transform for visuals")]
    public Transform axis;
    [Tooltip("The shaft for this wheel control")]
    public Transform shaft;
    [Tooltip("The wheel object for this wheel control")]
    public Transform wheel;
    [Tooltip("The left input device on the wheel")]
    public InputDevice left;
    [Tooltip("The right input device on the wheel")]
    public InputDevice right;

    [Header("Move Properties")]
    [Tooltip("Maximum angle wheel can be pushed forward")]
    public float maxXAngle = 30;
    [Tooltip("Maximum angle wheel can be pulled backward")]
    public float minXAngle = -30;
    [Tooltip("Maximum angle wheel can be pushed to the right")]
    public float maxZAngle = 30;
    [Tooltip("Maximum angle wheel can be pushed to the left")]
    public float minZAngle = -30;
    [Tooltip("Deadzone (in degrees) that must be moved for the shaft to move")]
    public float shaftDeadzone = 5;

    [Header("Turn Properties")]
    public float maxWheelAngle = 30;
    public float minWheelAngle = -30;
    [Tooltip("Deadzone (in degrees) that must be moved for the wheel to move")]
    public float wheelDeadzone = 5;

    // Default shaft angle
    private float defaultXAngle;
    private float defaultYAngle;
    private float defaultZAngle;

    private bool reset;
    private Quaternion goalShaftRotation;
    private Quaternion goalWheelRotation;

    private Quaternion prevShaftRotation;
    private Quaternion prevWheelRotation;

    private AudioSource wheelMoveSource;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        defaultXAngle = shaft.eulerAngles.x;
        if (defaultXAngle < 0)
            defaultXAngle = 0;
        defaultYAngle = shaft.eulerAngles.y;
        defaultZAngle = 0;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

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
        else
        {
            // Keep resetting wheel rotation to neutral
            goalWheelRotation = Quaternion.LookRotation(shaft.forward, shaft.up);
        }


        // Lerp to goal rotation
        Quaternion currentShaftRotation = Quaternion.Inverse(transform.rotation) * shaft.rotation;
        shaft.rotation = transform.rotation * Quaternion.Lerp(currentShaftRotation, goalShaftRotation, moveSpeed * Time.deltaTime);

        Quaternion currentWheelRotation = wheel.rotation;
        wheel.rotation = Quaternion.Lerp(currentWheelRotation, goalWheelRotation, moveSpeed * Time.deltaTime);

        // Just for visuals
        if (axis)
        {
            Vector3 forward = Vector3.Cross(Vector3.up, shaft.right);
            axis.rotation = Quaternion.LookRotation(forward, Vector3.Cross(shaft.right, forward));
        }


        // Update audio
        if (prevShaftRotation != currentShaftRotation)
            currentSpeed = Quaternion.Angle(prevShaftRotation, currentShaftRotation);
        else
            currentSpeed = 0;

        if (prevWheelRotation != currentWheelRotation)
            wheelMoveSource = UpdateMoveAudio(wheelAudioManager, Quaternion.Angle(prevWheelRotation, currentWheelRotation), ref wheelMoveSource);
        else
            wheelMoveSource = UpdateMoveAudio(wheelAudioManager, 0, ref wheelMoveSource);

        // Save previous rotations
        prevShaftRotation = currentShaftRotation;
        prevWheelRotation = currentWheelRotation;
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
        {
            angles.x = maxXAngle;
            PlayStopSound(0);
        }        
        else if (angles.x < minXAngle)
        {
            angles.x = minXAngle;
            PlayStopSound(0);
        }
        else
        {
            AllowStopSound(0);
        }

        if (angles.z > maxZAngle)
        {
            angles.z = maxZAngle;
            PlayStopSound(1);
        }
        else if (angles.z < minZAngle)
        {
            angles.z = minZAngle;
            PlayStopSound(1);
        }
        else
        {
            AllowStopSound(1);
        }
        

        // Apply deadzones
        if ((angles.x > defaultXAngle && angles.x <= defaultXAngle + shaftDeadzone) || (angles.x < defaultXAngle && angles.x >= defaultXAngle - shaftDeadzone))
            angles.x = defaultXAngle;
        if ((angles.z > defaultZAngle && angles.z <= defaultZAngle + shaftDeadzone) || (angles.z < defaultZAngle && angles.z >= defaultZAngle - shaftDeadzone))
            angles.z = defaultZAngle;

        // Rotate shaft
        goalShaftRotation = Quaternion.Euler(angles);

        // Move mech
        if (mechController)
        {
            // Create input
            Vector2 input = new Vector2();
            if (angles.z > defaultZAngle)
                input.x = -(angles.z - defaultZAngle - shaftDeadzone) / (maxZAngle - defaultZAngle - shaftDeadzone);
            else if (angles.z < defaultZAngle)
                input.x = (angles.z - defaultZAngle + shaftDeadzone) / (minZAngle - defaultZAngle + shaftDeadzone);

            if (angles.x > defaultXAngle)
                input.y = (angles.x - defaultXAngle - shaftDeadzone) / (maxXAngle - defaultXAngle - shaftDeadzone);
            else if(angles.x < defaultXAngle)
                input.y = -(angles.x - defaultXAngle + shaftDeadzone) / (minXAngle - defaultXAngle + shaftDeadzone);

            // Move the mech
            if (input.x != 0 || input.y != 0) {
                if (!mechController.moving)
                    mechController.StartMove();
                mechController.Move(input);
            }
            else if (mechController.moving)
            {
                mechController.StopMove();
            }         
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
        Vector3 delta = Vector3.ProjectOnPlane(rightPos, wheel.forward) - Vector3.ProjectOnPlane(leftPos, wheel.forward);
        float angle = Vector3.SignedAngle(transform.right, delta, wheel.forward);

        // Cap angle
        if (angle > maxWheelAngle)
        {
            angle = maxWheelAngle;
            PlayStopSound(2);
        }
        else if (angle < minWheelAngle)
        {
            angle = minWheelAngle;
            PlayStopSound(2);
        }
        else
        {
            AllowStopSound(2);
        }

        // Apply deadzone
        if ((angle > 0 && angle <= wheelDeadzone) || (angle < 0 && angle >= -wheelDeadzone))
            angle = 0;

        // Rotate wheel
        goalWheelRotation = Quaternion.LookRotation(shaft.forward, shaft.up) * Quaternion.Euler(0, 0, angle);

        // Move mech
        if (mechController)
        {
            // Create input
            Vector2 input = new Vector2();
            if (angle > 0)
                input.x = -(angle - wheelDeadzone) / (maxWheelAngle - wheelDeadzone);
            else if (angle < 0)
                input.x = (angle + wheelDeadzone) / (minWheelAngle + wheelDeadzone);

            // Turn the mech         
            if (input.x != 0 || input.y != 0)
            {
                if (!mechController.turning)
                    mechController.StartTurn();
                mechController.Turn(input);
            }
            else if (mechController.turning)
            {
                mechController.StopTurn();
            }
        }
    }
}
