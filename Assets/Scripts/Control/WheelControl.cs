using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : PhysicalControl
{
    [Tooltip("Audio source for the wheel")]
    public AudioSource wheelAudio;

    [Header("Component Properties")]
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

    // Audio
    private bool xLimit;
    private bool xLimitPlayed;
    private bool zLimit;
    private bool zLimitPlayed;
    private bool wheelLimit;
    private bool wheelLimitPlayed;
    private float wheelAudioThreshold = .001f;
    private float wheelMoveReduction = 8;
    private float wheelStopReduction = 128;
    private float shaftAudioThreshold = .001f;
    private float shaftMoveReduction = 4;
    private float shaftStopReduction = 64;

    // Start is called before the first frame update
    void Start()
    {
        defaultXAngle = shaft.eulerAngles.x;
        if (defaultXAngle < 0)
            defaultXAngle = 0;
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
            goalWheelRotation = Quaternion.LookRotation(Vector3.Cross(shaft.forward, transform.right), -shaft.forward);
        }

        // Lerp to goal rotation
        Quaternion currentShaftRotation = Quaternion.Inverse(transform.rotation) * shaft.rotation;
        shaft.rotation = transform.rotation * Quaternion.Lerp(currentShaftRotation, goalShaftRotation, moveSpeed * Time.deltaTime);

        Quaternion currentWheelRotation = wheel.rotation;
        wheel.rotation = Quaternion.Lerp(currentWheelRotation, goalWheelRotation, moveSpeed * Time.deltaTime);

        // Wheel audio
        float wheelAngle = Quaternion.Angle(currentWheelRotation, goalWheelRotation);
        if (wheelLimit)
        {
            if (!wheelLimitPlayed)
            {
                PlayStopSound(wheelAngle / wheelStopReduction, wheelAudio);
                wheelLimitPlayed = true;
            }
            wheelLimit = false;
        }
        else if (wheelAngle > wheelAudioThreshold)
        {
            PlayMoveSound(wheelAudio);
            UpdateMoveSound(wheelAngle / wheelMoveReduction, wheelAudio);
        }
        else if (wheelAudio.clip == moveSound && wheelAudio.isPlaying)
        {
            StopMoveSound(wheelAudio);
        }

        // Shaft audio
        float shaftAngle = Quaternion.Angle(currentShaftRotation, goalShaftRotation);
        if (xLimit || zLimit)
        {
            if (xLimit && !xLimitPlayed)
            {
                PlayStopSound(shaftAngle / shaftStopReduction);
                xLimitPlayed = true;
            }
            else if (zLimit && !zLimitPlayed)
            {
                PlayStopSound(shaftAngle / shaftStopReduction);
                zLimitPlayed = true;
            }
            xLimit = false;
            zLimit = false;
        }
        else if (shaftAngle > shaftAudioThreshold)
        {
            PlayMoveSound();
            UpdateMoveSound(shaftAngle / shaftMoveReduction);
        }
        else if (audio.clip == moveSound && audio.isPlaying)
        {
            StopMoveSound();
        }
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
            xLimit = true;
        }        
        else if (angles.x < minXAngle)
        {
            angles.x = minXAngle;
            xLimit = true;
        }
        else
        {
            xLimitPlayed = false;
        }

        if (angles.z > maxZAngle)
        {
            angles.z = maxZAngle;
            zLimit = true;
        }
        else if (angles.z < minZAngle)
        {
            angles.z = minZAngle;
            zLimit = true;
        }
        else
        {
            zLimitPlayed = false;
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
        Vector3 delta = Vector3.ProjectOnPlane(rightPos, wheel.up) - Vector3.ProjectOnPlane(leftPos, wheel.up);
        float angle = Vector3.SignedAngle(transform.right, delta, wheel.up);

        // Cap angle
        if (angle > maxWheelAngle)
        {
            angle = maxWheelAngle;
            wheelLimit = true;
        }
        else if (angle < minWheelAngle)
        {
            angle = minWheelAngle;
            wheelLimit = true;
        }
        else
        {
            wheelLimitPlayed = false;
        }         

        // Apply deadzone
        if ((angle > 0 && angle <= wheelDeadzone) || (angle < 0 && angle >= -wheelDeadzone))
            angle = 0;

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
                input.x = (angle - wheelDeadzone) / (maxWheelAngle - wheelDeadzone);
            else if (angle < 0)
                input.x = -(angle + wheelDeadzone) / (minWheelAngle + wheelDeadzone);

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
