using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class PlaceOnEdge : MonoBehaviour
{

#if UNITY_EDITOR
    public float edgeOffset = .5f;
    public float minAngleOffset = -30;
    public float maxAngleOffset = 30;
    [Min(0)]
    public float minScale = 1;
    [Min(0)]
    public float maxScale = 1;

    private float angleOffset;
    private float scale;


    private float maxLifetime = 5;
    private float spawnTime;

    private void Start()
    {
        angleOffset = Random.Range(minAngleOffset, maxAngleOffset);
        scale = Random.Range(minScale, maxScale);
        spawnTime = Time.time;

        transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        NavMeshHit hit;
        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            transform.rotation = Quaternion.LookRotation(hit.normal);
            transform.position = hit.position - transform.rotation * Vector3.forward * edgeOffset;
            transform.rotation *= Quaternion.Euler(0, angleOffset, 0);
        }

        if (Time.time - spawnTime > maxLifetime)
        {
            DestroyImmediate(this);
        }
    }
#endif
}
