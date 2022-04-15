using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleIK : MonoBehaviour
{
    public Transform target;
    public Vector3 axis = Vector3.up;
    public float angle = 0;

    private Transform elbow;
    private Transform root;

    private float lowerLength;
    private float upperLength;

    private Quaternion rootRotation;



    // Start is called before the first frame update
    void Start()
    {
        elbow = transform.parent;
        root = elbow.parent;
        lowerLength = (transform.position - elbow.position).magnitude;
        upperLength = (elbow.position - root.position).magnitude;

        rootRotation = Quaternion.Inverse(Quaternion.LookRotation(elbow.position - root.position, axis)) * Quaternion.Euler(0, angle, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = (target.position - root.position);
        Quaternion pivot = Quaternion.LookRotation(delta, axis);

        float length = (target.position - root.position).magnitude;

        var a = upperLength;
        var b = lowerLength;
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

        transform.rotation = target.rotation;
    }
}
