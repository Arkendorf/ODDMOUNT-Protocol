using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MechController : MonoBehaviour
{
    [Header("Component Properties")]
    public Rigidbody mech;
    private RigidbodyController rigidbodyController;
    public Transform target;
    [Header("Move")]
    [Tooltip("Force to apply to move the mech")]
    public float moveForce = 6000;
    [Tooltip("Percentage of normal turn speed while airborne")]
    [Range(0, 1)]
    public float airborneMoveDamping = .5f;
    [Header("Turn Properties")]
    [Tooltip("How fast the mech should turn, in degrees per second (which may be limited by physics)")]
    public float turnSpeed = 90;
    [Tooltip("How quickly to reset rotation")]
    public float turnReset = 60;
    [Tooltip("Maximum angle difference between desired mech angle and current mech angle")]
    public float maxTurn = 90;
    [Tooltip("Percentage of normal turn speed while airborne")]
    [Range(0, 1)]
    public float airborneTurnDamping = .5f;
    [Header("Jump Properties")]
    [Tooltip("Force to apply when the jump button is pressed")]
    public Vector3 jumpForce;
    [Header("Boost Properties")]
    [Tooltip("Force to apply per-frame while boosting (in the boost direction)")]
    public Vector3 boostForce;
    [Header("Other Properties")]
    [Tooltip("Percent of gravity to resist")]
    [Range(0, 1)]
    public float gravityResistance = .8f;

    // Whether the mech is receiving the move input or not
    private bool moving;
    private Vector2 moveInput;
    // Whether the mech is boosting
    private bool boosting;
    private Vector3 directionalBoostForce;
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
        airborne = true;

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

    private void FixedUpdate()
    {
        // Move the mech
        if (moving)
        {
            Vector3 force = mech.transform.rotation * new Vector3(moveInput.x, 0, moveInput.y) * moveForce;
            if (airborne)
                force *= airborneMoveDamping;

            mech.AddForce(force);
        }

        // Boost the mech
        if (boosting)
        {
            mech.AddForce(directionalBoostForce);
        }
    }

    public void StartMove()
    {
        moving = true;
    }

    public void Move(Vector2 input)
    {
        moveInput = input;
    }

    public void StopMove()
    {
        moving = false;
    }

    public void StartTurn()
    {
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
        float delta = Vector3.SignedAngle(mech.transform.forward, target.forward, Vector3.up);
        if (delta > maxTurn)
        {
            target.Rotate(Vector3.up, maxTurn - delta);
        }
        else if (delta < -maxTurn)
        {
            target.Rotate(Vector3.up, maxTurn - delta);
        }
    }

    public void StopTurn()
    {
        // Reset goal rotation when turn ends
        target.rotation = mech.transform.rotation;
    }

    public void Jump()
    {
        // Don't allow jumps while airborne
        if (!airborne)
        {
            mech.AddForce(mech.transform.rotation * jumpForce);
            // Mark as airborne
            airborne = true;
        }           
    }

    public void StartBoost()
    {
        // Mark that boosting has started
        boosting = true;

        // Calculate boost force
        directionalBoostForce = mech.transform.rotation * boostForce;
        if (moving)
        {
            directionalBoostForce = Quaternion.LookRotation(mech.transform.rotation * new Vector3(moveInput.x, 0, moveInput.y).normalized) * boostForce;
        }
    }

    public void Boost()
    {
        // Counteract gravity while boosting
        rigidbodyController.gravityResistance = 1;
    }

    public void StopBoost()
    {
        boosting = false;

        if (!airborne)
        {
            rigidbodyController.gravityResistance = gravityResistance;
        }
    }

    public void CollisionEnter(Collision collision)
    {
        // If colliding with the ground, no longer airborne
        if (collision.gameObject.layer == 6)
        {
            airborne = false;
            rigidbodyController.gravityResistance = gravityResistance;
        }    
    }

    public void CollisionExit(Collision collision)
    {
        // If colliding with the ground, no longer airborne
        if (collision.gameObject.layer == 6)
        {
            airborne = true;
            rigidbodyController.gravityResistance = 0;
        }
    }
}
