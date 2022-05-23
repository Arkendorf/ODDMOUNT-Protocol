using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;

public class GenerateNavMeshEdgeMask : MonoBehaviour
{

    public Vector2 maskSize;
    public Vector2 uvSize;
    [Min(0)]
    public float maxEdgeDistance;

    // Start is called before the first frame update
    public void Generate()
    {
        // Create the texture
        Texture2D mask = new Texture2D((int)maskSize.x, (int)maskSize.y);

        // Get scale of texture relative to target object UV
        Vector2 scale = new Vector2(uvSize.x * transform.localScale.x / maskSize.x, uvSize.y * transform.localScale.y / maskSize.y);
        Vector2 offset = new Vector2(-transform.position.x, -transform.position.z);

        for (int y = 0; y < mask.height; y++)
        {
            for (int x = 0; x < mask.width; x++)
            {
                // Convert texture coordinate to world coordinate for this object
                Vector3 worldPos = transform.rotation * new Vector3(- ((x - mask.width / 2 + .5f) * scale.x + offset.x), 0, - ((y - mask.height / 2 + .5f) * scale.y + offset.y));

                float distance = 0;

                // Get distance to navmesh edge
                NavMeshHit hit;
                if (NavMesh.FindClosestEdge(worldPos, out hit, NavMesh.AllAreas))
                {
                    distance = hit.distance;
                }

                // Convert distance to color
                float greyscale = Mathf.Max(0, maxEdgeDistance - distance) / maxEdgeDistance;

                // Set texture color
                mask.SetPixel(x, y, new Color(greyscale, greyscale, greyscale));
            }
        }
        // Apply changes to texture
        mask.Apply();

        // Set texture (optional)
        GetComponent<Renderer>().sharedMaterial.SetTexture("_NavMeshEdgeMask", mask);

        // Save file
        byte[] _bytes = mask.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Textures/NavMeshEdgeMasks/" + EditorSceneManager.GetActiveScene().name + "_" + name + ".png", _bytes);
    }
}

[CustomEditor(typeof(GenerateNavMeshEdgeMask))]
[CanEditMultipleObjects]
public class GenerateNavMeshEdgeMaskEditor : Editor
{
    GenerateNavMeshEdgeMask[] scripts;

    void OnEnable()
    {
        scripts = new GenerateNavMeshEdgeMask[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            scripts[i] = (GenerateNavMeshEdgeMask)targets[i];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate And Save"))
        {
            foreach (GenerateNavMeshEdgeMask script in scripts)
                script.Generate();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

