using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverControl : PhysicalControl
{
    [Header("Lever Properties")]
    [Tooltip("Input interactable")]
    public InputDevice input;
    [Tooltip("Transform to rotate")]
    public Transform lever;
    [Tooltip("Minimum lever angle (also the default rotation)")]
    public float minAngle = -45;
    [Tooltip("Maximum lever angle")]
    public float maxAngle = 45;
    [Tooltip("Whether the lever can rest inbetween positions")]
    public bool allowIntermediateAngle;
    [Tooltip("Whether lever resets when it triggers the OnPulled event")]
    public bool resetOnEventTrigger = true;
    [Tooltip("Whether the lever can be pulled or not")]
    public bool locked;
    [Tooltip("How quickly the lever rotates when resetting")]
    public float resetSpeed = 270;

    // Rotation offset of parent
    private float offsetAngle;
    // Current lever angle
    private float currentAngle;
    // Goal lever angle
    private float goalAngle;
    // Whether the lever is currently resetting
    private bool resetting;

    private float maxSpeed;

    public delegate void LeverEvent();
    // Invoked when the lever reaches it's maximum rotation
    public LeverEvent OnPulled;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (!lever)
            lever = transform;

        offsetAngle = Vector3.SignedAngle(Vector3.up, transform.up, transform.right);

        maxSpeed = Mathf.Max(resetSpeed, moveSpeed);

        currentAngle = minAngle;
        goalAngle = minAngle;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!locked)
        {
            if (input.interactable.isSelected)
            {
                resetting = false;

                Vector3 delta = (input.controller.transform.position - transform.position);
                Vector3 dir = Vector3.Cross(Vector3.Cross(transform.right, delta), transform.right);

                // Get new goal angle
                goalAngle = LoopAngle(Vector3.SignedAngle(Vector3.up, dir, transform.right) - offsetAngle);

                // Cap goal angle
                if (goalAngle > maxAngle)
                    goalAngle = maxAngle;

                if (goalAngle < minAngle)
                    goalAngle = minAngle;
            }
            else if (!allowIntermediateAngle && goalAngle != minAngle && (resetOnEventTrigger || goalAngle != maxAngle))
            {
                ResetLever();
            }
        }

        // Move current angle to match goal angle
        float offset = goalAngle - currentAngle;
        float speed = resetting ? resetSpeed : moveSpeed;

        if (offset > 0)
        {
            if (offset > speed * Time.deltaTime)
            {
                currentAngle += speed * Time.deltaTime;
                currentSpeed = speed / maxSpeed;
            }
            else
            {
                currentAngle = goalAngle;
                currentSpeed = (offset / Time.deltaTime) / maxSpeed;
            }

            if (currentAngle == maxAngle)
            {
                // Trigger event
                OnPulled?.Invoke();
                currentSpeed = speed / maxSpeed;
                PlayStopSound(0);
            }
        }
        else if (offset < 0)
        {
            if (offset < -speed * Time.deltaTime)
            {
                currentAngle -= speed * Time.deltaTime;
                currentSpeed = speed / maxSpeed;
            }
            else
            {
                currentAngle = goalAngle;
                currentSpeed = -(offset / Time.deltaTime) / maxSpeed;
            }
            if (currentAngle == minAngle)
            {
                resetting = false;
                currentSpeed = speed / maxSpeed;
                PlayStopSound(0);
            }
        }
        else
        {
            currentSpeed = 0;
        }

        if (currentAngle > minAngle && currentAngle < maxAngle)
        {
            AllowStopSound(0);
        }

        // Set lever rotation
        lever.localRotation = Quaternion.Euler(currentAngle, 0, 0);
    }

    // Reset lever to min position
    public void ResetLever()
    {
        resetting = true;
        goalAngle = minAngle;
    }

    private float LoopAngle(float angle)
    {
        if (angle > 180)
            return angle - 360;
        else if (angle < -180)
            return angle + 360;
        return angle;
    }
}
