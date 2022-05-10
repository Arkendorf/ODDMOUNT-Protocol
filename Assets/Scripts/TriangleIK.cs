using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleIK : MonoBehaviour
{
    [Tooltip("The transform this IK arm is reaching towards")]
    public Transform target;
    [Tooltip("Optional. If specified, the IK arm will try to center this transform on the target transform")]
    public Transform attachTransform;
    [Tooltip("Whether or not to lock the wrist")]
    public bool lockWrist;
    [Tooltip("The axis to bend the arm around")]
    public Vector3 axis = Vector3.up;
    [Tooltip("Angle to rotate around the forward axis of the arm")]
    public float angle = 0;

    private Transform elbow;
    private Transform root;

    private float lowerLength;
    private float upperLength;

    private Quaternion rootRotation;

    private Vector3 offset;
    private float offsetLength;
    private float offsetHeight;

    // Start is called before the first frame update
    void Start()
    {
        elbow = transform.parent;
        root = elbow.parent;
        lowerLength = (transform.position - elbow.position).magnitude;
        upperLength = (elbow.position - root.position).magnitude;

        rootRotation = Quaternion.Inverse(Quaternion.LookRotation(elbow.position - root.position, axis)) * Quaternion.Euler(0, angle, 0);

        SetAttachTransform(attachTransform);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = (target.position - root.position);
        if (lockWrist)
            delta -= transform.rotation * Vector3.up * offsetHeight;
        else
            delta -= target.rotation * offset;

        Quaternion pivot = Quaternion.LookRotation(delta, axis);

        float length = delta.magnitude;

        var a = upperLength;
        var b = lowerLength;
        if (lockWrist)
            b += offsetLength;
        var c = length;

        float B = Mathf.Acos((c * c + a * a - b * b) / (2 * c * a)) * Mathf.Rad2Deg;
        float C = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) * Mathf.Rad2Deg;

        if (!float.IsNaN(C))
        {
            root.rotation = pivot * Quaternion.Euler(0, -B, 0) * rootRotation;
            elbow.rotation = pivot * Quaternion.Euler(0, (180 - C - B), 0) * rootRotation;
        }
        else
        {
            root.rotation = pivot * rootRotation;
            elbow.rotation = pivot * rootRotation;
        }

        if (lockWrist)
            transform.rotation = Quaternion.LookRotation(elbow.forward, target.up);
        else
            transform.rotation = target.rotation;
    }

    // Sets the attach transform for this IK arm
    public void SetAttachTransform(Transform newAttach)
    {
        attachTransform = newAttach;

        // Calculate offset
        if (attachTransform)
            offset = Quaternion.Inverse(transform.rotation) * (attachTransform.position - transform.position);
        else
            offset = Vector3.zero;

        offsetLength = new Vector3(offset.x, 0, offset.z).magnitude;
        offsetHeight = offset.y;
    }
}
