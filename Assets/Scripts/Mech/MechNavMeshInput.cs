using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MechController))]
[RequireComponent(typeof(NavMeshAgent))]
public class MechNavMeshInput : MonoBehaviour
{
    [Tooltip("Target transform")]
    public Transform target;
    [Tooltip("Tag on rigidbodies this mech should shoot")]
    public string hostileTag = "Player";
    [Tooltip("How far target can move before a repath")]
    public float targetMoveThreshold = .3f;
    [Tooltip("Desired velocity magnitude must exceed this number before the mech starts moving")]
    public float moveThreshold = .1f;
    [Tooltip("Angle difference must exceed this number before the mech starts moving, in degrees")]
    public float angleThreshold = 1;
    [Tooltip("Distance between target and current transform when slowing should start")]
    public float stopDistance = .5f;

    // Distance from target on xz plane
    public float distance { get; private set; }

    // Mech controller
    private MechController mechController;
    // Nav mesh agent
    private NavMeshAgent agent;

    // Previous position of the target
    private Vector3 oldPosition;

    // Weapon info
    private float weaponAngle;
    private float weaponDistance;
    private float weaponDir;

    // Start is called before the first frame update
    void Start()
    {
        // Get agent component on this gameobject
        mechController = GetComponent<MechController>();
        // Get agent component on this gameobject
        agent = GetComponent<NavMeshAgent>();
        // Make agent parameters match mech controller
        agent.speed = mechController.maxMove;
        agent.angularSpeed = mechController.turnSpeed;
        agent.acceleration = mechController.moveForce / mechController.mech.mass;
        agent.stoppingDistance = stopDistance;
        // Prevent agent from updating position, only use it to generate a path
        agent.updatePosition = false;
        agent.updateRotation = false;

        // Set path if a target is given
        if (target)
        {
            agent.SetDestination(target.position);
            oldPosition = target.position;
        }

        if (mechController.weapons[0])
        {
            // Get offset between weapon and mech, ignoring y
            Vector3 weaponOffset = mechController.weapons[0].origin.position - mechController.mech.position;
            weaponOffset.y = 0;
            // Save magnitude
            weaponDistance = weaponOffset.magnitude;
            // Get forward vector of weapon, ignoring y
            Vector3 weaponForward = mechController.weapons[0].origin.forward;
            weaponForward.y = 0;
            // Get angle between weapon offset and it's forward vector
            weaponAngle = 180 - Mathf.Abs(Vector3.Angle(weaponForward, weaponOffset));
            // Save which side weapon is on
            weaponDir = (Quaternion.Inverse(mechController.mech.rotation) * (mechController.weapons[0].origin.position - mechController.mech.position)).x > 0 ? 1 : -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            // Get mech's offset from target
            Vector3 delta = (target.position - mechController.mech.position);
            delta.y = 0;

            distance = delta.magnitude;

            // Set the destination to the target if the target has moved
            if ((target.position - oldPosition).sqrMagnitude > targetMoveThreshold * targetMoveThreshold)
            {
                agent.SetDestination(target.position);
                oldPosition = target.position;
            }

            // Update position
            UpdatePosition();

            // Update rotation
            UpdateRotation();

            // Iterate through weapons. If they are facing a player, fire
            foreach(Weapon weapon in mechController.weapons)
            {
                RaycastHit hit;
                bool raycast = Physics.Raycast(weapon.origin.position, weapon.origin.forward, out hit, weapon.range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                if (raycast && hit.rigidbody && hit.rigidbody.CompareTag(hostileTag ))
                {
                    if (!weapon.firing)
                        weapon.StartFire();
                    else
                        weapon.Fire();
                }
                else if (weapon.firing)
                {
                    weapon.StopFire();
                }
            }
        }
     
        // Update agent's conception of where it is in space
        agent.nextPosition = mechController.mech.transform.position;
    }

    private void UpdatePosition()
    {
        // Check if mech needs to move
        if (agent.desiredVelocity.sqrMagnitude > moveThreshold * moveThreshold)
        {
            // If not yet moving, start moving
            if (!mechController.moving)
                mechController.StartMove();

            // Get movement direction
            Vector3 dir = Quaternion.Inverse(mechController.mech.transform.rotation) * agent.desiredVelocity;

            // Create input
            Vector2 input = new Vector2(dir.x, dir.z);
            input = input.normalized * (input.magnitude / agent.speed);

            // Update move
            mechController.Move(input);
        }
        else if (mechController.moving) // Stop move if necessary
        {
            mechController.StopMove();
        }
    }

    private void UpdateRotation()
    {
        // Get mech's offset from target
        Vector3 delta = (target.position - mechController.mech.position);
        delta.y = 0;

        // Goal rotation
        float goalAngle = Quaternion.LookRotation(delta).eulerAngles.y;
        // If mech has a primary weapon, aim it at the target
        if (mechController.weapons[0])
        {
            float l = weaponDistance;
            float M = weaponAngle;

            float m = distance;

            float offsetAngle = -Mathf.Asin(l * Mathf.Sin(M * Mathf.Deg2Rad) / m) * Mathf.Rad2Deg;

            goalAngle += weaponDir * offsetAngle;
        }                 
        
        // Get current rotation
        float currentAngle = mechController.mech.transform.eulerAngles.y;
        // Get rotation options
        float rightAngle = LoopAngle(goalAngle - currentAngle);
        float leftAngle = LoopAngle(goalAngle - currentAngle - 360);
        // Get best rotation
        float angle = Mathf.Abs(leftAngle) < Mathf.Abs(rightAngle) ? leftAngle : rightAngle;

        // If angle difference is great enough, rotate the mech
        if (Mathf.Abs(angle) > angleThreshold)
        {
            // If not yet turning, start moving
            if (!mechController.turning)
                mechController.StartTurn();

            // Create input
            float inputX = Mathf.Clamp(angle / (mechController.turnSpeed * Time.deltaTime), -1, 1);
            Vector2 input = new Vector2(inputX, 0);

            // Update turn
            mechController.Turn(input);
        }
        else if (mechController.turning) // Stop turn if necessary
        {
            mechController.StopTurn();
        }
    }

    public void Stop()
    {
        if (mechController.turning)
            mechController.StopTurn();

        if (mechController.moving)
            mechController.StopMove();

        foreach (Weapon weapon in mechController.weapons)
        {
            if (weapon.firing)
                weapon.StopFire();
        }
    }

    // Loop an angle
    private float LoopAngle(float angle)
    {
        if (angle > 180)
            return angle - 360;
        else if (angle < -180)
            return angle + 360;
        else
            return angle;
    }
}
