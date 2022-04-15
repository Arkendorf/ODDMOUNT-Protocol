using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleIK : MonoBehaviour
{
    public Transform target;
    public Vector3 normal = Vector3.up;

    private Transform elbow;
    private Transform root;

    private float lowerLength;
    private float upperLength;

    private Vector3 rootLookVector;
    private Quaternion rootRotation;


    // Start is called before the first frame update
    void Start()
    {
        elbow = transform.parent;
        root = elbow.parent;
        lowerLength = (transform.position - elbow.position).magnitude;
        upperLength = (elbow.position - root.position).magnitude;

        rootLookVector = elbow.position - root.position;
    }

    // Update is called once per frame
    void Update()
    {
        rootRotation = Quaternion.Inverse(Quaternion.LookRotation(rootLookVector, normal));


        Vector3 delta = (target.position - root.position);
        Quaternion pivot = Quaternion.LookRotation(delta, normal);

        float length = (target.position - root.position).magnitude;

        var a = upperLength;
        var b = lowerLength;
        var c = length;

        float B = Mathf.Acos((c * c + a * a - b * b) / (2 * c * a)) * Mathf.Rad2Deg;
        float C = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) * Mathf.Rad2Deg;

        if (!float.IsNaN(C))
        {
            root.rotation = pivot * rootRotation * Quaternion.AngleAxis((-B), normal);
            elbow.localRotation = Quaternion.AngleAxis(180 - C, normal);
        }
        else
        {
            root.rotation = pivot * rootRotation;
            elbow.localRotation = Quaternion.identity;
        }
  
        transform.rotation = target.rotation;
    }
}
