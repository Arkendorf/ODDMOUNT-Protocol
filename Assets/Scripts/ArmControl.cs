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

    // Start is called before the first frame update
    void Start()
    {
        // Create the target
        target = new GameObject("IKTarget");
        target.transform.parent = transform;

        // Set initial transform
        target.transform.position = ik.transform.position;
        target.transform.rotation = ik.transform.rotation;
        goalPosition = ik.transform.position;
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
    void Update()
    {
        // Update goal position and rotation
        if (input.interactable.isSelected)
        {
            goalPosition = hand.position - goalRotation * offset;
            up = hand.up;
        }

        // Lerp position
        target.transform.position = Vector3.Lerp(target.transform.position, goalPosition, moveSpeed * Time.deltaTime);
        // Get goal rotation
        goalRotation = Quaternion.LookRotation(ik.transform.parent.forward, up);
        Quaternion currentRotation = Quaternion.LookRotation(ik.transform.parent.forward, target.transform.up);
        // Lerp rotation
        target.transform.rotation = Quaternion.Lerp(currentRotation, goalRotation, moveSpeed * Time.deltaTime);

        // Update RigidbodyController targets
        Transform controlSegment = ik.transform;
        for (int i = 0; i < rigidbodyTargets.Length; i++)
        {
            rigidbodyTargets[i].transform.rotation = Quaternion.Inverse(transform.rotation) * mechController.mech.transform.rotation * controlSegment.rotation;
            controlSegment = controlSegment.parent;
        }
    }
}
