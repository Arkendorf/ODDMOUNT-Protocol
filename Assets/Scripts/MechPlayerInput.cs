using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MechController))]
public class MechPlayerInput : MonoBehaviour
{
    [Tooltip("The action asset used for mech actions in this project")]
    public InputActionAsset actionAsset;
    [Header("Secondary Locomotion")]
    public InputActionReference turn;
    public InputActionReference jump;
    public InputActionReference boost;

    // Mech controller to attach callbacks to
    private MechController mechController;

    private void Update()
    {
        // Update secondary locomotion
        if (turn.action.enabled)
            mechController.Turn(turn.action.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        // Find this mech's mech controller
        mechController = GetComponent<MechController>();

        // Attach callbacks for secondary locomotion
        jump.action.performed += mechController.Jump;
    }
    private void OnDisable()
    {
        // Detach callbacks for secondary locomotion
        jump.action.performed -= mechController.Jump;
    }
}
