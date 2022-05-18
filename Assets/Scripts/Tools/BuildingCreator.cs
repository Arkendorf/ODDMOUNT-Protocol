using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class BuildingCreator : MonoBehaviour
{
    public List<GameObject> doorOptions;
    [Space()]
    public List<GameObject> windowOptions;
    public float minWindowHeight = 1;
    public float windowHeight = 1;
    public float windowWidth = 1;
    [Space()]
    public GameObject roofPrefab;
    public float roofHeight = 1;
    public float roofOverhang = 1;
    [Space()]
    public bool front;
    public bool back;
    public bool left;
    public bool right;

    private int doorStyle;
    private int windowStyle;
    private float height;
    private int storyCount;

    // Start is called before the first frame update
    public void Create()
    {
        // Remove all current children
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        // Get styles
        doorStyle = Random.Range(0, doorOptions.Count - 1);
        windowStyle = Random.Range(0, windowOptions.Count - 1);

        // Calculate building height and story count
        height = transform.position.y + transform.localScale.y / 2;        
        storyCount = Mathf.Max(0, (int)((height - minWindowHeight) / windowHeight));

        // Decorate walls
        if (front)
            DecorateWall("Front", transform.forward, transform.localScale.z / 2, transform.localScale.x);
        if (back)
            DecorateWall("Back", -transform.forward, transform.localScale.z / 2, transform.localScale.x);
        if (left)
            DecorateWall("Left", -transform.right, transform.localScale.x / 2, transform.localScale.z);
        if (right)
            DecorateWall("Right", transform.right, transform.localScale.x / 2, transform.localScale.z);

        // Create roof
        GameObject roof = Instantiate(roofPrefab);
        roof.name = "Roof";
        roof.transform.parent = transform;
        roof.transform.rotation = transform.rotation;
        roof.transform.position = new Vector3(transform.position.x, height + roofHeight / 2, transform.position.z);
        roof.transform.localScale = new Vector3(1 + roofOverhang * 2 / transform.localScale.x, roofHeight / transform.localScale.y, 1 + roofOverhang * 2 / transform.localScale.z);
        roof.isStatic = true;
    }

    private void DecorateWall(string name, Vector3 forward, float offset, float width)
    {
        Vector3 scale = new Vector3(1 / width, 1 / transform.localScale.y, 1 / (offset * 2));
        Vector3 basePosition = new Vector3(transform.position.x, 0, transform.position.z);

        if (height >= minWindowHeight)
        {
            GameObject door = Instantiate(doorOptions[doorStyle]);
            door.name = name + " Door";
            door.transform.parent = transform;
            door.transform.rotation = Quaternion.LookRotation(forward);
            door.transform.position = basePosition + forward * offset;
            door.transform.localScale = scale;
            door.isStatic = true;
        }

        int windowCount = (int)(width / windowWidth);
        float windowOffset = - (windowCount * windowWidth) / 2;
        for (int y = 0; y < storyCount; y++)
        {
            for (int x = 0; x < windowCount; x++)
            {
                GameObject window = Instantiate(windowOptions[windowStyle]);
                window.name = name + " Window (" + x + ", " + y + ")"; 
                window.transform.parent = transform;
                window.transform.rotation = Quaternion.LookRotation(forward);
                window.transform.position = basePosition + window.transform.rotation * new Vector3(windowOffset + (x + .5f) * windowWidth, minWindowHeight + (y + .5f) * windowHeight, offset);
                window.transform.localScale = scale;
                window.isStatic = true;
            }
        }
    }
}

[CustomEditor(typeof(BuildingCreator))]
[CanEditMultipleObjects]
public class BuildingCreatorEditor : Editor
{
    BuildingCreator script;

    void OnEnable()
    {
        script = (BuildingCreator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("Create"))
        {
            script.Create();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif