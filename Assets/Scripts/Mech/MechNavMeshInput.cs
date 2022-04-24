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
    [Tooltip("How far target can move before a repath")]
    public float targetMoveThreshold = .3f;
    [Tooltip("Desired velocity magnitude must exceed this number before the mech starts moving")]
    public float moveThreshold = .1f;
    [Tooltip("Angle difference must exceed this number before the mech starts moving, in degrees")]
    public float angleThreshold = 1;
    [Tooltip("Distance between target and current transform when slowing should start")]
    public float stopDistance = .5f;

    // Mech controller
    private MechController mechController;
    // Nav mesh agent
    private NavMeshAgent agent;

    // Previous position of the target
    private Vector3 oldPosition;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            // Check if mech has reached destination
            if ((target.position - mechController.mech.transform.position).sqrMagnitude > stopDistance * stopDistance)
            {
                // Set the destination to the target if the target has moved
                if ((target.position - oldPosition).sqrMagnitude > targetMoveThreshold * targetMoveThreshold)
                {
                    agent.SetDestination(target.position);
                    oldPosition = target.position;
                }

                // Update position
                UpdatePosition();

                // Update rotation
                UpdateRotation(agent.desiredVelocity);
            }
            else // If mech is close enough, face the target
            {
                UpdateRotation(target.position - mechController.mech.transform.position);

                // Don't move if in the right spot
                if (mechController.moving)
                    mechController.StopMove();
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

    private void UpdateRotation(Vector3 lookVector)
    {
        // Goal rotation (in direction of velocity if moving, or towards target)
        Quaternion goalRotation;
        if (lookVector.sqrMagnitude > 0)
            goalRotation = Quaternion.LookRotation(lookVector);
        else
            goalRotation = Quaternion.LookRotation(target.position - mechController.mech.transform.position);
        // Get current rotation
        Quaternion currentRotation = mechController.mech.transform.rotation;
        // Get rotation options
        float rightAngle = LoopAngle(goalRotation.eulerAngles.y - currentRotation.eulerAngles.y);
        float leftAngle = LoopAngle(goalRotation.eulerAngles.y - currentRotation.eulerAngles.y - 360);
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
