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

    // Target for the IK
    private GameObject target;
    // Transform of the controller
    private Transform hand;
    // Offset between controller
    private Vector3 offset;

    // Up vector
    private Vector3 up;

    // Goal position and rotation for IK
    private Vector3 goalPosition;
    private Quaternion goalRotation;

    private GameObject[] rigidbodyTargets;

    // Audio variables
    private float audioThreshold = .001f;

    // Start is called before the first frame update
    void Start()
    {
        // Create the target
        target = new GameObject("IKTarget");
        target.transform.parent = transform;

        // Set initial transform
        target.transform.position = ik.transform.position;
        target.transform.rotation = ik.transform.rotation;
        goalPosition = ik.transform.position - transform.position;
        goalRotation = ik.transform.rotation;

        // Set initial IK state
        ik.target = target.transform;

        // Attach callbacks to events
        input.Selected += Selected;
        input.Deselected += Deselected;

        // Attach controllers
        AssignControl(3);
    }

    private void OnDisable()
    {
        // Remove callbacks
        input.Selected -= Selected;
        input.Deselected -= Deselected;
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
        Transform model = input.controller.model;
        offset = Quaternion.Inverse(model.rotation) * (model.position - ik.transform.position);
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
            goalPosition = Quaternion.Inverse(transform.rotation) * (hand.position - goalRotation * offset - transform.position);
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
}
