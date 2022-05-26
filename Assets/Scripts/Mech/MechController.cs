using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MechController : MonoBehaviour
{
    [Header("Component Properties")]
    public Rigidbody mech;
    public RigidbodyController rigidbodyController;
    public Transform rotationTarget;
    [Header("Combat Properties")]
    [Tooltip("This mech's max health")]
    public float maxHealth = 100;
    [Tooltip("This mech's weapons (two max for player mechs)")]
    public List<Weapon> weapons;
    [Header("Move Properties")]
    [Tooltip("Force to apply to move the mech")]
    public float moveForce = 4000;
    [Tooltip("If velocity magnitude is over this number, walking won't increase speed")]
    public float maxMove = 3;
    [Tooltip("Amount of force to apply when not or airborne to reduce mech's velocity back to zero")]
    public float moveDamping = 1000;
    [Tooltip("Percentage of normal turn speed while airborne")]
    [Range(0, 1)]
    public float airborneMoveDamping = .5f;
    [Header("Turn Properties")]
    [Tooltip("How fast the mech should turn, in degrees per second (which may be limited by physics)")]
    public float turnSpeed = 90;
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
    [Tooltip("Mech's maximum fuel")]
    public float maxFuel = 100;
    [Tooltip("Fuel cost per application of boost force")]
    public float boostCost = .1f;
    [Header("Other Properties")]
    [Tooltip("Percent of gravity to resist")]
    [Range(0, 1)]
    public float gravityResistance = .8f;

    // Whether the mech is receiving the move input or not
    [HideInInspector] public bool moving { get; private set; }
    private Vector2 moveInput;
    // Whether the mech is turning
    [HideInInspector] public bool turning { get; private set; }
    // Whether the mech is boosting
    [HideInInspector] public bool boosting { get; private set; }
    private Vector3 directionalBoostForce;
    // Whether the mech is airborne or not
    [HideInInspector] public bool airborne { get; private set; }

    // Mech's current amount of fuel
    [HideInInspector] public float fuel { get; private set; }

    // Mech's current health
    [HideInInspector] public float health { get; private set; }

    [HideInInspector] public bool dead { get; private set; }

    public enum MechDamageType {Collision, Projectile}

    public delegate void MechEvent();
    public MechEvent OnDeath;
    public delegate void MechDamageEvent(MechDamageType damageType);
    public MechDamageEvent OnTakeDamage;
    public MechDamageEvent OnDealDamage;

    // Start is called before the first frame update
    void Start()
    {
        if (!mech)
            mech = GetComponent<Rigidbody>();

        fuel = maxFuel;
        health = maxHealth;
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
        Vector3 moveDir = mech.transform.rotation * new Vector3(moveInput.x, 0, moveInput.y);
        if (moving)
        {
            Vector3 force = moveDir * moveForce;
            if (airborne)
                force *= airborneMoveDamping;

            // Cap force if it would make velocity exceed maximum
            Vector3 newVelocity = mech.velocity + force * Time.fixedDeltaTime / mech.mass;
            if (newVelocity.sqrMagnitude > maxMove * maxMove)
                force = force.normalized * Mathf.Max(0, maxMove - mech.velocity.magnitude) * mech.mass / Time.fixedDeltaTime;

            mech.AddForce(force);
        }
        // Damp movement
        if (!airborne)
        {
            // Get velocity to damp
            Vector3 velocity;
            if (moving)
                velocity = Vector3.Project(mech.velocity, Vector3.Cross(moveDir, Vector3.up));
            else
                velocity = mech.velocity;

            // Get damping force
            Vector3 force = -velocity.normalized * moveDamping;

            // If damping force exceeds remaining velocity, make it perfectly nullify remaining velocity
            Vector3 delta = force * Time.fixedDeltaTime / mech.mass;
            if (velocity.sqrMagnitude < delta.sqrMagnitude)
                force = -velocity * mech.mass / Time.fixedDeltaTime;

            mech.AddForce(force);
        }

        // Boost the mech
        if (boosting && fuel > 0)
        {
            mech.AddForce(directionalBoostForce);
            // Consume fuel
            fuel = Mathf.Max(0, fuel - boostCost);
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
        turning = true;
    }

    // Update desired mech angle
    public void Turn(Vector2 input)
    {
        // Get rotation amount based on input
        float angle = input.x * turnSpeed * Time.deltaTime;
        if (airborne)
            angle *= airborneTurnDamping;

        // Rotate goal angle
        rotationTarget.Rotate(Vector3.up, angle);

        // Cap difference between mech and goal angle by maxTurn
        float delta = Vector3.SignedAngle(mech.transform.forward, rotationTarget.forward, Vector3.up);
        if (delta > maxTurn)
        {
            rotationTarget.Rotate(Vector3.up, maxTurn - delta);
        }
        else if (delta < -maxTurn)
        {
            rotationTarget.Rotate(Vector3.up, maxTurn - delta);
        }
    }

    public void StopTurn()
    {
        // Reset goal rotation when turn ends
        rotationTarget.rotation = mech.transform.rotation;

        turning = false;
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
        if (!boosting)
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

    private void CollisionEnter(Collision collision)
    {
        // If colliding with the ground, no longer airborne
        if (collision.gameObject.layer == 6)
        {
            airborne = false;
            rigidbodyController.gravityResistance = gravityResistance;
        }    
    }

    private void CollisionExit(Collision collision)
    {
        // If colliding with the ground, no longer airborne
        if (collision.gameObject.layer == 6)
        {
            airborne = true;
            rigidbodyController.gravityResistance = 0;
        }
    }

    // Add to mech's current fuel levels
    public void AddFuel(float amount)
    {
        fuel = Mathf.Min(maxFuel, fuel + amount);
    }

    public void TakeDamage(float damage, MechDamageType damageType = MechDamageType.Collision)
    {
        // Do damage to mech
        health -= damage;

        OnTakeDamage?.Invoke(damageType);

        // Invoke death event if damage killed mech
        if (!dead && health <= 0)
        {
            dead = true;
            OnDeath?.Invoke();
        }
    }
    
    public void DealDamage(MechController enemy, float damage, MechDamageType damageType = MechDamageType.Collision)
    {
        enemy.TakeDamage(damage, damageType);

        OnDealDamage?.Invoke(damageType);
    }
}
