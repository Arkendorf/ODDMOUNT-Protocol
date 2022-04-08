using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MechController))]
public class MechPlayerInput : MonoBehaviour
{
    [Tooltip("The action asset used for mech actions in this project")]
    public InputActionAsset actionAsset;

    //[Header("Primary Locomotion")]
    //public InputActionReference move;
    //[Header("Secondary Locomotion")]

    [Header("Locomotion")]
    public InputActionReference jump;
    public InputActionReference boost;

    // Mech controller to attach callbacks to
    private MechController mechController;

    // Action states
    private bool movePerformed;
    private bool turnPerformed;
    private bool boostPerformed;

    private void Update()
    {
        //// Update primary locomotion
        //if (movePerformed)
        //    mechController.Move(move.action.ReadValue<Vector2>());

        //// Update secondary locomotion
        //if (turnPerformed)
        //    mechController.Turn(turn.action.ReadValue<Vector2>());

        if (boostPerformed)
            mechController.Boost();
    }

    private void OnEnable()
    {
        // Find this mech's mech controller
        mechController = GetComponent<MechController>();

        //// Attach callbacks for primary locomotion
        //move.action.performed += context => { mechController.StartMove(); movePerformed = true; };
        //move.action.canceled += context => { mechController.StopMove(); movePerformed = false; };
        //// Attach callbacks for secondary locomotion
        //turn.action.performed += context => { mechController.StartTurn(); turnPerformed = true; };
        //turn.action.canceled += context => { mechController.StopTurn(); turnPerformed = false; };

        jump.action.performed += context => { mechController.Jump(); };
        boost.action.performed += context => { mechController.StartBoost(); boostPerformed = true; };
        boost.action.canceled += context => { mechController.StopBoost(); boostPerformed = false; };
    }
    private void OnDisable()
    {
        //// Detach callbacks for primary locomotion
        //move.action.performed -= context => { mechController.StartMove(); movePerformed = true; };
        //move.action.canceled -= context => { mechController.StopMove(); movePerformed = false; };
        //// Detach callbacks for secondary locomotion
        //turn.action.performed -= context => { mechController.StartTurn(); turnPerformed = true; };
        //turn.action.canceled -= context => { mechController.StartTurn(); turnPerformed = false; };

        jump.action.performed -= context => { mechController.Jump(); };
        boost.action.performed -= context => { mechController.StartBoost(); boostPerformed = true; };
        boost.action.canceled -= context => { mechController.StopBoost(); boostPerformed = false; };
    }
}
