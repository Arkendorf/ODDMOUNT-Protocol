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
    [Tooltip("Sound to play")]
    public new AudioSource audio;

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

    [Header("Wheel Properties")]
    public float moveSpeed = 8;

    // Default shaft angle
    private float defaultXAngle;
    private float defaultYAngle;
    private float defaultZAngle;

    private bool reset;
    private Quaternion goalShaftRotation;
    private Quaternion goalWheelRotation;

    private Dictionary<int,bool> soundPlayed;

    // Start is called before the first frame update
    void Start()
    {
        defaultXAngle = shaft.eulerAngles.x;
        if (defaultXAngle < 0)
            defaultXAngle = 0;
        defaultYAngle = shaft.eulerAngles.y;
        defaultZAngle = 0;

        soundPlayed = new Dictionary<int, bool>();
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
            goalWheelRotation = Quaternion.LookRotation(Vector3.Cross(transform.right, goalShaftRotation * Vector3.back), goalShaftRotation * Vector3.back);

            // Reset mech
            if (mechController)
            {
                if (mechController.moving)
                    mechController.StopMove();
                if (mechController.turning)
                    mechController.StopTurn();
            }

            PlaySound(0);
            AllowSound(0);

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
        {
            angles.x = maxXAngle;
            PlaySound(1);
        }
        else
            AllowSound(1);
            
        if (angles.x < minXAngle)
        {
            angles.x = minXAngle;
            PlaySound(2);
        }
        else
            AllowSound(2);

        if (angles.z > maxZAngle)
        {
            angles.z = maxZAngle;
            PlaySound(3);
        }
        else
            AllowSound(3);

        if (angles.z < minZAngle)
        {
            angles.z = minZAngle;
            PlaySound(4);
        }
        else
            AllowSound(4);

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
            PlaySound(5);
        }
        else
            AllowSound(5);

        if (angle < minWheelAngle)
        {
            angle = minWheelAngle;
            PlaySound(6);
        }
        else
            AllowSound(6);

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

    private void PlaySound(int id)
    {
        if (!audio.isPlaying && soundPlayed[id] == false)
        {
            audio.pitch = Random.Range(0.75f, 1.2f);
            audio.Play();
            soundPlayed[id] = true;
        }
    }

    private void AllowSound(int id)
    {
        soundPlayed[id] = false;
    }
}
