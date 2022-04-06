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
    [Tooltip("How fast the mech should move in meters per second (which may be limited by physics)")]
    public float moveSpeed = 15;
    [Tooltip("How quickly to reset movement")]
    public float moveReset = 10;
    [Tooltip("Maximum distance difference between desired mech position and current mech position")]
    public float maxMove = .5f;
    [Tooltip("Percentage of normal turn speed while airborne")]
    [Range(0, 1)]
    public float airborneMoveDamping = .5f;
    [Header("Jump Properties")]
    [Tooltip("Force to apply when the jump button is pressed")]
    public Vector3 jumpForce;
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
    [Header("Other Properties")]
    [Tooltip("Percent of gravity to resist")]
    [Range(0, 1)]
    public float gravityResistance = .8f;

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

    private void Update()
    {
        // Return target rotation to mech rotation
        //float deltaRotation = Vector3.SignedAngle(mech.transform.forward, target.forward, Vector3.up);
        //if (deltaRotation > 0)
        //    target.Rotate(Vector3.up, Mathf.Min(deltaRotation, turnReset * Time.deltaTime));
        //else
        //    target.Rotate(Vector3.up, Mathf.Max(deltaRotation, -turnReset * Time.deltaTime));

        // Return target position to mech position
        Vector3 deltaPosition = mech.transform.position - target.position;
        target.position += deltaPosition.normalized * Mathf.Min(deltaPosition.magnitude, moveReset * Time.deltaTime);
        // Always set target y to match mech
        target.position = new Vector3(target.position.x, mech.transform.position.y, target.position.z);
    }

    public void Move(Vector2 input)
    {
        // Get move amount based on input
        Vector3 offset = mech.transform.rotation * new Vector3(input.x, 0, input.y) * moveSpeed * Time.deltaTime;
        if (airborne)
            offset *= airborneMoveDamping;

        // Set target position
        target.position += offset;

        // Cap difference between mech and goal position by maxMove
        Vector3 delta = target.position - mech.transform.position;
        if (delta.sqrMagnitude > maxMove * maxMove)
        {
            target.position = mech.transform.position + delta.normalized * maxMove;
        }
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

    public void Jump()
    {
        // Don't allow jumps while airborne
        if (!airborne)
        {
            mech.AddForce(mech.transform.rotation * jumpForce);
            // Mark as airborne
            CollisionExit();
        }
                   
    }

    public void CollisionEnter()
    {
        airborne = false;
        rigidbodyController.gravityResistance = gravityResistance;
    }

    public void CollisionExit()
    {
        airborne = true;
        rigidbodyController.gravityResistance = 0;
    }
}
