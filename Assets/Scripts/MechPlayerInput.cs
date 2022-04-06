using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MechController))]
public class MechPlayerInput : MonoBehaviour
{
    [Tooltip("The action asset used for mech actions in this project")]
    public InputActionAsset actionAsset;
    [Header("Primary Locomotion")]
    public InputActionReference move;
    [Header("Secondary Locomotion")]
    public InputActionReference turn;
    public InputActionReference jump;
    public InputActionReference boost;

    // Mech controller to attach callbacks to
    private MechController mechController;

    // Action states
    private bool movePerformed;
    private bool turnPerformed;

    private void Update()
    {
        // Update primary locomotion
        if (movePerformed)
            mechController.Move(move.action.ReadValue<Vector2>());

        // Update secondary locomotion
        if (turnPerformed)
            mechController.Turn(turn.action.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        // Find this mech's mech controller
        mechController = GetComponent<MechController>();

        // Attach callbacks for primary locomotion
        move.action.performed += context => { movePerformed = true; };
        move.action.canceled += context => { movePerformed = false; };
        // Attach callbacks for secondary locomotion
        turn.action.performed += context => { turnPerformed = true; };
        turn.action.canceled += context => { turnPerformed = false; };
        jump.action.performed += context => { mechController.Jump(); };
    }
    private void OnDisable()
    {
        // Detach callbacks for primary locomotion
        move.action.performed -= context => { movePerformed = true; };
        move.action.canceled -= context => { movePerformed = false; };
        // Detach callbacks for secondary locomotion
        turn.action.performed -= context => { turnPerformed = true; };
        turn.action.canceled -= context => { turnPerformed = false; };
        jump.action.performed -= context => { mechController.Jump(); };
    }
}
