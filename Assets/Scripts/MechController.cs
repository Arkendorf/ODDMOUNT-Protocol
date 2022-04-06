using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MechController : MonoBehaviour
{
    [Header("General Properties")]
    public Rigidbody mech;
    private RigidbodyController rigidbodyController;
    public Transform target;
    [Header("Jump Properties")]
    [Tooltip("Force to apply when the jump button is pressed")]
    public Vector3 jumpForce;
    [Header("Turn Properties")]
    [Tooltip("How fast the mech should turn, in degrees per second (which may be limited by physics)")]
    public float turnSpeed = 90;
    [Tooltip("Percentage of normal turn speed while airborne")]
    [Range(0, 1)]
    public float airborneTurnDamping = .5f;
    [Tooltip("Maximum angle difference between desired mech angle and current mech angle")]
    public float maxTurn = 90;

    // Whether the mech is airborne or not
    private bool airborne;


    // Start is called before the first frame update
    void Start()
    {
        if (!mech)
            mech = GetComponent<Rigidbody>();  
    }

    private void OnEnable()
    {
        // Get the mech's base rigidbody controller
        if (!rigidbodyController)
            rigidbodyController = GetComponentInChildren<RigidbodyController>();

        rigidbodyController.CollisionEnter += CollisionEnter;
        rigidbodyController.CollisionExit += CollisionExit;
    }

    private void OnDisable()
    {
        rigidbodyController.CollisionEnter -= CollisionEnter;
        rigidbodyController.CollisionExit -= CollisionExit;
    }

    public void Move(Vector2 input)
    {
        //Vector3 force = mech.transform.rotation * new Vector3(input.x, 0, input.y) * moveForce;
        //mech.AddForce(force);
    }

    // Update desired mech angle
    public void Turn(Vector2 input)
    {
        // Get rotation amount based on input
        float angle = input.x * turnSpeed * Time.deltaTime;
        if (airborne)
            angle *= airborneTurnDamping;

        // Rotate goal angle
        target.Rotate(Vector3.up, angle);

        // Cap difference between mech and goal angle by maxTurn
        float dif = Vector3.SignedAngle(mech.transform.forward, target.forward, Vector3.up);
        if (dif > maxTurn)
        {
            target.Rotate(Vector3.up, maxTurn - dif);
        }
        else if (dif < -maxTurn)
        {
            target.Rotate(Vector3.up, maxTurn - dif);
        }
    }

    public void Jump()
    {
        // Don't allow jumps while airborne
        if (!airborne)
            mech.AddForce(mech.transform.rotation * jumpForce);        
    }

    public void CollisionEnter()
    {
        airborne = false;
        rigidbodyController.correctGravity = true;
    }

    public void CollisionExit()
    {
        airborne = true;
        rigidbodyController.correctGravity = false;
    }
}
