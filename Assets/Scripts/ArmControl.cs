using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArmControl : MonoBehaviour
{
    [Tooltip("Input interactable")]
    public InputDevice input;
    [Tooltip("IK Controller")]
    public DitzelGames.FastIK.FastIKFabric ik;

    [Tooltip("Arm Properties")]
    public float moveSpeed = 8;

    // Target for the IK
    private GameObject target;
    // Transform of the controller
    private Transform hand;
    // Offset between controller
    private Vector3 offset;

    // Goal position and rotation for IK
    private Vector3 goalPosition;
    private Quaternion goalRotation;

    // Start is called before the first frame update
    void Start()
    {
        // Create the target
        target = new GameObject("Target");
        target.transform.parent = transform;

        // Set initial transform
        target.transform.position = ik.transform.position;
        target.transform.rotation = ik.transform.rotation;
        goalPosition = ik.transform.position;
        goalRotation = ik.transform.rotation;

        // Set initial IK state
        ik.Target = target.transform;

        // Attach callbacks to events
        input.Selected += Selected;
        input.Deselected += Deselected;
    }

    private void OnDisable()
    {
        // Remove callbacks
        input.Selected -= Selected;
        input.Deselected -= Deselected;
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
            goalPosition = hand.position - hand.rotation * offset;
            goalRotation = hand.rotation;
        }

        // Lerp position
        target.transform.position = Vector3.Lerp(target.transform.position, goalPosition, moveSpeed * Time.deltaTime);
        target.transform.rotation = Quaternion.Lerp(target.transform.rotation, goalRotation, moveSpeed * Time.deltaTime);
    }
}
