using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechFootPlacer : MonoBehaviour
{
    public Transform mechBase;
    public Vector3 offset;
    public TriangleIK ik;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mechBase.position + mechBase.rotation * offset;
        transform.rotation = mechBase.rotation;
        ik.axis = mechBase.right;
    }
}
