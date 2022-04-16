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

    // Whether this foot is moving or not
    [HideInInspector] public bool moving;

    // Lerp info
    private Vector3 startPosition;
    private Vector3 goalPosition;
    private float lerpPercent;
    private float yOffset;

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

        if (!mech.boosting && !mech.airborne && (mech.moving || mech.turning))
        {
            // Check if a foot needs to move
            if (!moving && !otherFoot.moving)
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
        else
        {
            // If mech is not moving (ie. sliding, airborne), just use default position
            StartStep(0, Vector3.zero);
        }

        // Calculate y offset
        yOffset = Mathf.Clamp((.25f - (lerpPercent - .5f) * (lerpPercent - .5f)) * 4 * liftHeight, 0, liftHeight);
        // Lerp
        if (lerpPercent < 1)
        {
            // Increase lerp percent
            lerpPercent += lerpSpeed * Time.deltaTime;
            // Perform the lerp
            transform.position = Vector3.Lerp(startPosition, goalPosition, lerpPercent);

            if (lerpPercent >= 1)
            {
                lerpPercent = 1;
                moving = false;
            }
        }

        // update height and rotation
        transform.position = new Vector3(transform.position.x, mechBase.position.y + yOffset, transform.position.z);
        transform.rotation = mechBase.rotation;
    }

    // Starts a step based on the given step size and velocity
    private void StartStep(float stepSize, Vector3 velocity)
    {
        startPosition = transform.position;
        startPosition.y = 0;
        goalPosition = mechBase.position + mechBase.rotation * defaultOffset + velocity.normalized * stepSize + velocity * velocityWeight;
        goalPosition.y = 0;

        // TODO: This IF statement is a band-aid solution to stutter steps, should be replaced
        if ((goalPosition - startPosition).sqrMagnitude > stepSize)
        {
            lerpPercent = 0;
            moving = true;
        }
    }
}
