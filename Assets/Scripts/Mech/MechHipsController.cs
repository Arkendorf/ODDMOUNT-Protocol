using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHipsController : MonoBehaviour
{
    [Tooltip("The mech this waist belongs to")]
    public MechController mechController;
    [Tooltip("Velocity magnitude threshold at which velocity impacts waist rotation")]
    public float velocityThreshold = .1f;

    [HideInInspector] public Vector3 forward { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        forward = mechController.mech.transform.forward;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!mechController.dead && mechController.mech.velocity.sqrMagnitude > velocityThreshold * velocityThreshold)
        {
            forward = mechController.mech.velocity;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
