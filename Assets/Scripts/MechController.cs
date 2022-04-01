using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MechController : MonoBehaviour
{
    public Rigidbody mech;
    public Vector3 jumpForce;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!mech)
            mech = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // Attach callbacks to input actions

    }

    private void OnDisable()
    {
        // Detach callbacks to input actions
    }

    public void Jump(InputAction.CallbackContext context) { Jump(); }
    public void Jump()
    {
        mech.AddForce(mech.transform.rotation * jumpForce);
    }
}
