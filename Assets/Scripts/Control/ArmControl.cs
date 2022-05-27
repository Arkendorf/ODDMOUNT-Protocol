using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArmControl : PhysicalControl
{
    [Header("Arm Properties")]
    [Tooltip("Mech wrist transform this control should manipulate")]
    public Transform mechWrist;
    [Tooltip("Input interactable")]
    public InputDevice input;
    [Tooltip("IK Controller")]
    public TriangleIK ik;
    [Tooltip("Minimum distance between shoulder and target")]
    public float minDistance = .2f;

    // Target for the IK
    private GameObject target;
    // Transform of the controller
    private Transform hand;

    // Up vector
    private Vector3 up;

    // Goal position and rotation for IK
    private Vector3 goalPosition;
    private Quaternion goalRotation;

    private GameObject[] rigidbodyTargets;

    private MechHandController handController;
    private Weapon weapon;

    // Audio variables
    private float audioThreshold = .001f;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Create the target
        target = new GameObject("IKTarget");
        target.transform.parent = transform;

        // Set initial transform
        if (ik.attachTransform)
            target.transform.position = ik.attachTransform.position;
        else
            target.transform.position = ik.transform.position;
        target.transform.rotation = ik.transform.rotation;
        goalPosition = Quaternion.Inverse(transform.rotation) * (target.transform.position - transform.position);
        goalRotation = target.transform.rotation;

        // Set initial IK state
        ik.target = target.transform;

        // Attach callbacks to events
        input.Selected += Selected;
        input.Deselected += Deselected;

        // Attach haptic callbacks
        handController = mechWrist.GetComponent<MechHandController>();
        if (handController)
            handController.OnCollision += OnHandCollision;
        weapon = mechWrist.GetComponentInChildren<Weapon>();
        if (weapon)
            weapon.OnFireShot += OnWeaponFire;


        // Attach controllers
        AssignControl(3);
    }

    private void OnDisable()
    {
        // Remove callbacks
        input.Selected -= Selected;
        input.Deselected -= Deselected;

        // Remove haptic callbacks
        if (handController)
            handController.OnCollision -= OnHandCollision;
        if (weapon)
            weapon.OnFireShot += OnWeaponFire;
    }

    // Create and assign RigidbodyController targets
    private void AssignControl(int segments)
    {
        rigidbodyTargets = new GameObject[segments];

        Transform segment = mechWrist;
        for (int i = 0; i < segments; i++)
        {
            rigidbodyTargets[i] = new GameObject("RigidbodyTarget" + i);
            rigidbodyTargets[i].transform.parent = transform;

            RigidbodyController controller = segment.GetComponent<RigidbodyController>();
            if (controller)
                controller.target = rigidbodyTargets[i].transform;

            segment = segment.parent;
        }
    }

    private void Selected()
    {
        // Get offset between controller and end of arm
        hand = input.controller.transform;
    }

    private void Deselected()
    {
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // Update goal position and rotation
        if (input.interactable.isSelected)
        {
            goalPosition = Quaternion.Inverse(transform.rotation) * (hand.position - transform.position);

            // Don't allow goal to be too close to the IK root, or weird stuff will happen
            if (goalPosition.sqrMagnitude < minDistance * minDistance)
                goalPosition = goalPosition.normalized * minDistance;

            up = Quaternion.Inverse(transform.rotation) * hand.up;
        }

        // Lerp position
        Vector3 currentPosition = Quaternion.Inverse(transform.rotation) * (target.transform.position - transform.position);
        target.transform.position = transform.position + transform.rotation * Vector3.Lerp(currentPosition, goalPosition, moveSpeed * Time.deltaTime);
        // Get goal rotation
        goalRotation = Quaternion.LookRotation(ik.transform.parent.forward, transform.rotation * up);
        Quaternion currentRotation = Quaternion.LookRotation(ik.transform.parent.forward, target.transform.up);
        // Lerp rotation
        target.transform.rotation = Quaternion.Lerp(currentRotation, goalRotation, moveSpeed * Time.deltaTime);

        // Update RigidbodyController targets
        Transform controlSegment = ik.transform;
        for (int i = 0; i < rigidbodyTargets.Length; i++)
        {
            rigidbodyTargets[i].transform.rotation = Quaternion.Inverse(transform.rotation) * mechController.mech.rotation * controlSegment.rotation;
            controlSegment = controlSegment.parent;
        }

        // Audio
        Vector3 delta = (goalPosition - currentPosition);
        if (delta.sqrMagnitude >= audioThreshold * audioThreshold)
        {
            currentSpeed = (delta.magnitude - audioThreshold);
        }
        else
        {
            currentSpeed = 0;
        }
    }    

    private void OnHandCollision()
    {
        if (input.controller)
            HapticsManager.Instance.SendHapticImpulse(.4f, .1f, input.controller);
    }

    private void OnWeaponFire()
    {
        if (input.controller)
            HapticsManager.Instance.SendHapticImpulse(.4f, .1f, input.controller);
    }
}
