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
    public string LeftSecondaryLocomotionMapName;
    public string RightSecondaryLocomotionMapName;
    [Space()]
    public string turnActionName;
    public string jumpActionName;
    public string boostActionName;

    // Mech controller to attach callbacks to
    private MechController mechController;
    

    private void OnEnable()
    {
        // Find this mech's mech controller
        mechController = GetComponent<MechController>();

        // Attach callbacks for secondary locomotion
        AttachSecondaryLocomotion(actionAsset.FindActionMap(LeftSecondaryLocomotionMapName));
        AttachSecondaryLocomotion(actionAsset.FindActionMap(RightSecondaryLocomotionMapName));
    }
    private void OnDisable()
    {
        // Detach callbacks for secondary locomotion
        DetachSecondaryLocomotion(actionAsset.FindActionMap(LeftSecondaryLocomotionMapName));
        DetachSecondaryLocomotion(actionAsset.FindActionMap(RightSecondaryLocomotionMapName));
    }

    private void AttachSecondaryLocomotion(InputActionMap map)
    {
        map.FindAction(jumpActionName).performed += mechController.Jump;
    }

    private void DetachSecondaryLocomotion(InputActionMap map)
    {
        map.FindAction(jumpActionName).performed -= mechController.Jump;
    }
}
