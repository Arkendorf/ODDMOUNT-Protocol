using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechFootPlacer : MonoBehaviour
{
    [Header("Component Properties")]
    [Tooltip("Mech controller")]
    public MechController mech;
    [Tooltip("Base of mech to base this foot's positioning on, and whose velocity to use for calculation")]
    public Transform mechBase;
    [Tooltip("The other foot (besides this one) of the mech")]
    public MechFootPlacer otherFoot;
    [Tooltip("The IK component controlling the leg attached to this foot")]
    public TriangleIK ik;
    [Header("Movement Properties")]
    [Tooltip("Default offset of this foot from the mech base")]
    public Vector3 defaultOffset;
    [Tooltip("Maximum distance (on the ground plane) between this foot and the mech base")]
    public float threshold = .5f;
    [Tooltip("Maximum distance (on the ground plane) between this foot and the mech base when sidestepping")]
    public float sidestepThreshold = .25f;
    [Tooltip("Angle at which sidesteps become valid")]
    public float sidestepAngleThreshold = 60;
    [Tooltip("How much velocity contributes to foot positioning")]
    public float velocityWeight = .5f;
    [Header("Animation Properties")]
    [Tooltip("How fast to lerp to the new position")]
    public float lerpSpeed = 8;
    [Tooltip("How high to raise the foot when moving")]
    public float liftHeight = .2f;
    [Tooltip("How fast the mech moves before sliding instead of walking")]
    public float velocityThreshold = 10;

    // Whether this foot is moving or not
    [HideInInspector] public bool moving;

    // Lerp info
    private Vector3 startPosition;
    private Vector3 goalPosition;
    private float lerpPercent;
    private float yOffset;

    private Vector3 localStartPosition;
    private Vector3 localGoalPosition;
    private bool reset;

    // Mech's rigidbody
    private new Rigidbody rigidbody;


    // Start is called before the first frame update
    void Start()
    {
        // Get the mech's rigidbody
        rigidbody = mechBase.GetComponent<Rigidbody>();

        // Set foot initial position
        transform.position = mechBase.position + mechBase.rotation * defaultOffset;
        transform.rotation = mechBase.rotation;

        // Don't lerp to start
        lerpPercent = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // Set IK axis
        ik.axis = mechBase.right;

        // If mech isn't airborne, and velocity is below threshold, move feet (walk)
        bool walking = rigidbody.velocity.sqrMagnitude < velocityThreshold * velocityThreshold && !mech.airborne;

        if (walking)
        {
            // Check if we just exited a slide
            if (reset)
            {
                // Reset positions
                startPosition = mechBase.position + localStartPosition;
                goalPosition = mechBase.position + localGoalPosition;
                // Allow future resets
                reset = false;
            }
            // Check if a foot needs to move
            else if (!moving && !otherFoot.moving)
            {
                // Find distance between average foot and body
                Vector3 avgPos = (transform.position + otherFoot.transform.position) / 2;
                Vector3 delta = (mechBase.position - avgPos);
                delta.y = 0;

                // Find foot distances from the body
                Vector3 thisDelta = (transform.position - mechBase.position);
                thisDelta.y = 0;
                Vector3 otherDelta = (otherFoot.transform.position - mechBase.position);
                otherDelta.y = 0;

                // Calculate step size. If not sidestepping, don't stunt steps
                float stepSize = threshold;

                // Get velocity
                Vector3 velocity = rigidbody.velocity;
                velocity.y = 0;

                // Get the angle between forward direction of the mech, and the mech's velocity, to see if the mech is moving more forward, or more to the side
                float angle = Vector3.Angle(mechBase.forward, velocity);

                // Check if sidesteps are valid
                if (velocity.sqrMagnitude > .001f && angle > sidestepAngleThreshold)
                {
                    // If body is outside of sidestep threshold from avg foot position
                    if (delta.sqrMagnitude > sidestepThreshold * sidestepThreshold && Vector3.Dot(thisDelta, velocity) < 0)
                    {
                        // If this is the closer foot, move it
                        if (thisDelta.sqrMagnitude <= otherDelta.sqrMagnitude)
                        {
                            StartStep(stepSize, velocity);
                        }
                    }

                    // If sidestepping stunt the normal steps
                    stepSize = sidestepThreshold;
                }

                // Normal steps are always valid
                // If body is outside of threshold from average foot position
                if (!moving && delta.sqrMagnitude > threshold * threshold && Vector3.Dot(thisDelta, velocity) < 0)
                {
                    // If this is the farthest foot, move it
                    if (thisDelta.sqrMagnitude >= otherDelta.sqrMagnitude)
                    {
                        StartStep(stepSize, velocity);
                    }
                }
            }
        }
        else if (!reset)
        {
            // If mech is not moving (ie. sliding, airborne), just use default position
            StartStep(0, Vector3.zero);
            localStartPosition = startPosition - mechBase.position;
            localGoalPosition = goalPosition - mechBase.position;
            // Don't allow more resets
            reset = true;
        }

        // Update lerp percent
        if (lerpPercent < 1)
        {
            // Increase lerp percent
            lerpPercent += lerpSpeed * Time.deltaTime;

            if (lerpPercent >= 1)
            {
                lerpPercent = 1;
                moving = false;
            }
        }      

        // Perform the lerp
        Vector3 newPosition;
        if (walking)
            newPosition = Vector3.Lerp(startPosition, goalPosition, lerpPercent);
        else
            newPosition = mechBase.position + Vector3.Lerp(localStartPosition, localGoalPosition, lerpPercent);

        // Calculate y offset (if walking)
        yOffset = walking ? Mathf.Clamp((.25f - (lerpPercent - .5f) * (lerpPercent - .5f)) * 4 * liftHeight, 0, liftHeight) : 0;
        
        // Set y
        newPosition.y = mechBase.position.y + yOffset;

        // update height and rotation
        transform.position = newPosition;
        transform.rotation = mechBase.rotation;
        
    }

    // Starts a step based on the given step size and velocity
    private void StartStep(float stepSize, Vector3 velocity)
    {
        // Calculate new start and goal positions
        Vector3 newStartPosition = transform.position;
        newStartPosition.y = 0;
        Vector3 newGoalPosition = mechBase.position + mechBase.rotation * defaultOffset + velocity.normalized * stepSize + velocity * velocityWeight;
        newGoalPosition.y = 0;

        // TODO: This IF statement is a band-aid solution to stutter steps, should be replaced
        if ((newGoalPosition - newStartPosition).sqrMagnitude > stepSize)
        {
            startPosition = newStartPosition;
            goalPosition = newGoalPosition;
            lerpPercent = 0;
            moving = true;
        }
    }
}
