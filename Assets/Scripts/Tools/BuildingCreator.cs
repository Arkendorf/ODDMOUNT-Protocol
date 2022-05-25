using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingCreator : MonoBehaviour
{
    public Material baseBuildingMaterial;
    public List<Material> materialOptions;
    public Material baseTrimMaterial;
    public List<Material> trimOptions;
    public Material placeholderTrimMaterial;
    [Space()]
    public List<GameObject> doorOptions;
    public float doorHeight = 1;
    [Space()]
    public List<GameObject> windowOptions;
    public float minWindowHeight = 1;
    public float maxWindowHeight = 1;
    public float minWindowWidth = 1;
    public float maxWindowWidth = 1;
    [Space()]
    public GameObject roofPrefab;
    public float roofHeight = 1;
    public float roofOverhang = 1;
    [Space()]
    public bool front;
    public bool back;
    public bool left;
    public bool right;

#if UNITY_EDITOR
    private int doorStyle;
    private int windowStyle;
    private float height;
    private int storyCount;
    private float windowWidth;
    private float windowHeight;
    private int trimStyle;
#endif

    private void OnEnable()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.SetPropertyBlock(CreatePropertyBlock(renderer.sharedMaterial));
        renderer.sharedMaterial = baseBuildingMaterial;

        Material trimMat = transform.Find("Roof").GetComponent<Renderer>().sharedMaterial;
        MaterialPropertyBlock trimBlock = CreatePropertyBlock(trimMat);
        SetTrimProperties(trimMat, trimBlock);

        this.enabled = false;
    }

    private MaterialPropertyBlock CreatePropertyBlock(Material baseMat)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture("_MainTex", baseMat.GetTexture("_MainTex"));
        block.SetTexture("_Metallic", baseMat.GetTexture("_Metallic"));
        block.SetTexture("_Normal", baseMat.GetTexture("_Normal"));
        block.SetFloat("_Scale", baseMat.GetFloat("_Scale"));
        return block;
    }

    private void SetTrimProperties(Material mat, MaterialPropertyBlock block)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] newMaterials = renderer.sharedMaterials;
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == mat)
                {
                    newMaterials[i] = baseTrimMaterial;
                    renderer.SetPropertyBlock(block, i);
                }
            }
            renderer.sharedMaterials = newMaterials;
        }
    }

#if UNITY_EDITOR

    // Start is called before the first frame update
    public void Create()
    {
        // Remove all current children
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);

        // Get styles
        doorStyle = Random.Range(0, doorOptions.Count);
        windowStyle = Random.Range(0, windowOptions.Count);
        trimStyle = Random.Range(0, trimOptions.Count);

        // Calculate other parameters
        windowWidth = Random.Range(minWindowWidth, maxWindowWidth);
        windowHeight = Random.Range(minWindowHeight, maxWindowHeight);

        // Calculate building height and story count
        height = transform.position.y + transform.localScale.y / 2;        
        storyCount = Mathf.Max(0, (int)((height - doorHeight) / windowHeight));

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
        roof.GetComponent<Renderer>().sharedMaterial = trimOptions[trimStyle];

        // Set material
        GetComponent<Renderer>().sharedMaterial = materialOptions[Random.Range(0, materialOptions.Count)];
    }

    private void DecorateWall(string name, Vector3 forward, float offset, float width)
    {
        Vector3 scale = new Vector3(1 / width, 1 / transform.localScale.y, 1 / (offset * 2));
        Vector3 basePosition = new Vector3(transform.position.x, 0, transform.position.z);

        if (height >= doorHeight)
        {
            GameObject door = Instantiate(doorOptions[doorStyle]);
            door.name = name + " Door";
            door.transform.parent = transform;
            door.transform.rotation = Quaternion.LookRotation(forward);
            door.transform.position = basePosition + door.transform.rotation * new Vector3(Random.Range(-width / 3, width / 3), 0, offset);
            door.transform.localScale = scale;
            door.isStatic = true;
            SetTrim(door.transform, trimOptions[trimStyle]);
            door.GetComponent<MeshRenderer>().receiveGI = ReceiveGI.LightProbes;
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
                window.transform.position = basePosition + window.transform.rotation * new Vector3(windowOffset + (x + .5f) * windowWidth, doorHeight + (y + .5f) * windowHeight, offset);
                window.transform.localScale = scale;
                window.isStatic = true;
                SetTrim(window.transform, trimOptions[trimStyle]);
                window.GetComponent<MeshRenderer>().receiveGI = ReceiveGI.LightProbes;
            }
        }
    }

    private void SetTrim(Transform feature, Material mat)
    {
        Renderer[] renderers = feature.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] newMaterials = renderer.sharedMaterials;
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == placeholderTrimMaterial)
                {
                    newMaterials[i] = mat;
                }
            }
            renderer.sharedMaterials = newMaterials;
        }
    }
}

[CustomEditor(typeof(BuildingCreator))]
[CanEditMultipleObjects]
public class BuildingCreatorEditor : Editor
{
    BuildingCreator[] scripts;

    void OnEnable()
    {
        scripts = new BuildingCreator[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            scripts[i] = (BuildingCreator)targets[i];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("Create"))
        {
            foreach (BuildingCreator script in scripts)
                script.Create();
        }

        serializedObject.ApplyModifiedProperties();
    }
#endif
}
