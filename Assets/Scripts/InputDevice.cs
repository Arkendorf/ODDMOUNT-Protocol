using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class InputDevice : MonoBehaviour
{
    [Tooltip("The action asset used for mech actions in this project")]
    public InputActionAsset actionAsset;
    [Tooltip("The action map within the above action asset to activate if this control device is grabbed with the left controller")]
    public string leftActionMap;
    [Tooltip("The action map within the above action asset to activate if this control device is grabbed with the right controller")]
    public string rightActionMap;

    public List<AudioSource> buttonAudio;

    public AudioClip pressClip;
    public AudioClip releaseClip;


    // XR interactable component on this gameobject
    [HideInInspector] public XRGrabInteractable interactable { get; private set; }
    [HideInInspector] public XRBaseController controller { get; private set; }
    private XRDirectInteractor interactor;

    // Transform data;
    private Transform parent;
    private Quaternion localRotation;
    private Vector3 localPosition;

    // Model data to store while device is interacted with
    private Transform modelParent;
    private Vector3 modelLocalPosition;
    private Quaternion modelLocalRotation;
    private Vector3 modelLocalScale;

    public delegate void InputEvent();
    public InputEvent Selected;
    public InputEvent Deselected;

    private void Start()
    {
        // Start actions as disabled
        actionAsset.FindActionMap(leftActionMap).Disable();
        actionAsset.FindActionMap(rightActionMap).Disable();

        parent = transform.parent;
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        // Get the XR interactable component
        interactable = GetComponent<XRGrabInteractable>();

        // Add callbacks for selection
        interactable.selectEntered.AddListener(SelectEntered);
        interactable.selectExited.AddListener(SelectExited);
    }

    private void OnDisable()
    {
        SelectExited();

        // Remove callbacks for selected
        interactable.selectEntered.RemoveListener(SelectEntered);
        interactable.selectExited.AddListener(SelectExited);

        // If this control device is disabled while the interactable is still active, artifically end selection
        if (interactable.isSelected)
        {
            interactor.EndManualInteraction();
        }
    }

    private void Update()
    {
        if (interactable.isSelected)
        {
            SelectedUpdate();
        }
    }

    // Called every frame while this control device is selected, should be overridden
    protected void SelectedUpdate()
    {

    }

    private void SelectEntered(SelectEnterEventArgs arg)
    {
        // Get the controller and the interactor interacting with this control device
        interactor = arg.interactorObject.transform.GetComponent<XRDirectInteractor>();
        controller = interactor.xrController;

        transform.parent = parent;
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;

        if (controller.model)
        {
            // Save parent, position, and rotation before overriding them
            modelParent = controller.model.parent;
            modelLocalPosition = controller.model.localPosition;
            modelLocalRotation = controller.model.localRotation;
            modelLocalScale = controller.model.localScale;

            // If the interactable has an attach transform, get the relevant data
            Vector3 attachPosition = interactable.transform.position;
            Quaternion attachRotation = interactable.transform.rotation;
            if (interactable.attachTransform)
            {
                attachPosition = interactable.attachTransform.position;
                attachRotation = interactable.attachTransform.rotation;
            }

            // Set the parent, position, and rotation
            controller.model.SetParent(transform);
            controller.model.rotation = attachRotation * Quaternion.Inverse(interactor.attachTransform.localRotation);
            controller.model.position = attachPosition - controller.model.rotation * interactor.attachTransform.localPosition;
        }


        // Activate proper action map depending on which hand is grabbing
        InputActionMap map;
        if (interactor.CompareTag("Left Hand"))
        {
            map = actionAsset.FindActionMap(leftActionMap);    
        }
        else
        {
            map = actionAsset.FindActionMap(rightActionMap);
        }
        if (map != null) { map.Enable(); }
        AttachAudio(map);

        // Once setup is done, call event
        Selected?.Invoke();
    }

    private void SelectExited(SelectExitEventArgs arg)
    {
        SelectExited();
    }
    private void SelectExited()
    {
        // Deactivate proper action map depending on which hand is grabbing
        InputActionMap map;
        if (interactor.CompareTag("Left Hand"))
        {
            map = actionAsset.FindActionMap(leftActionMap);
        }
        else
        {
            map = actionAsset.FindActionMap(rightActionMap);
        }
        DetachAudio(map);
        map.Disable();

        if (controller.model)
        {
            // Reset the parent, position, and rotation
            controller.model.SetParent(modelParent);
            controller.model.localPosition = modelLocalPosition;
            controller.model.localRotation = modelLocalRotation;
            controller.model.localScale = modelLocalScale;
        }

        // Clear values
        controller = null;
        interactor = null;

        // Once setup is done, call event
        Deselected?.Invoke();
    }

    // Attach audio to buttons
    private void AttachAudio(InputActionMap map)
    {
        int i = 0;
        foreach (InputAction action in map.actions)
        {
            if (action.type == InputActionType.Button)
            {
                int button = i;
                action.performed += context => PlayAudio(button, pressClip);
                action.canceled += context => PlayAudio(button, releaseClip);
                i++;
            }          
        }
    }

    private void DetachAudio(InputActionMap map)
    {
        int i = 0;
        foreach (InputAction action in map.actions)
        {
            if (action.type == InputActionType.Button)
            {
                int button = i;
                action.performed -= context => PlayAudio(button, pressClip);
                action.canceled -= context => PlayAudio(button, releaseClip);
                i++;
            }
        }
    }

    private void PlayAudio(int button, AudioClip clip)
    {
        buttonAudio[button].clip = clip;
        buttonAudio[button].pitch = UnityEngine.Random.Range(0.75f, 1.2f);
        buttonAudio[button].Play();
    }
}
