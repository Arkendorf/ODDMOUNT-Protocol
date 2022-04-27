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
    [Header("Combat")]
    public InputActionReference leftWeaponTrigger;
    public InputActionReference rightWeaponTrigger;

    // Mech controller
    private MechController mechController;

    // Action states
    private bool movePerformed;
    private bool turnPerformed;
    private bool boostPerformed;
    private bool leftWeaponPerformed;
    private bool rightWeaponPerformed;

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

        if (leftWeaponPerformed)
            mechController.weapons[0]?.Fire();

        if (rightWeaponPerformed)
            mechController.weapons[1]?.Fire();
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

        // Combat
        // Left weapon
        leftWeaponTrigger.action.performed += context => { mechController.weapons[0]?.StartFire(); leftWeaponPerformed = true; };
        leftWeaponTrigger.action.canceled += context => { mechController.weapons[0]?.StopFire(); leftWeaponPerformed = false; };
        // Right weapon
        rightWeaponTrigger.action.performed += context => { mechController.weapons[1]?.StartFire(); rightWeaponPerformed = true; };
        rightWeaponTrigger.action.canceled += context => { mechController.weapons[1]?.StopFire(); rightWeaponPerformed = false; };
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

        // Combat
        // Left weapon
        leftWeaponTrigger.action.performed -= context => { mechController.weapons[0]?.StartFire(); leftWeaponPerformed = true; };
        leftWeaponTrigger.action.canceled -= context => { mechController.weapons[0]?.StopFire(); leftWeaponPerformed = false; };
        // Right weapon
        rightWeaponTrigger.action.performed -= context => { mechController.weapons[1]?.StartFire(); rightWeaponPerformed = true; };
        rightWeaponTrigger.action.canceled -= context => { mechController.weapons[1]?.StopFire(); rightWeaponPerformed = false; };
    }
}
