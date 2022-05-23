using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class TextureAtlasItem
{
    public Texture2D texture;
    public MeshFilter meshFilter;
}

public class TextureAtlasCreator : MonoBehaviour
{
    public Texture2D atlas;
    public List<TextureAtlasItem> items;
    public int padding;

#if UNITY_EDITOR
    public void Create()
    {
        Texture2D[] textures = new Texture2D[items.Count];
        MeshFilter[] meshFilters = new MeshFilter[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            textures[i]  = items[i].texture;
            meshFilters[i] = items[i].meshFilter;
        }

        var uvRectChangesFromTextureAtlas = atlas.PackTextures(textures, padding, 8192);

        for (int i = 0; i < uvRectChangesFromTextureAtlas.Length; i++)
        {
            var rect = uvRectChangesFromTextureAtlas[i];

            var meshFilter = meshFilters[i];

            if (meshFilter)
            {
                var atlasedMesh = CopyMesh(meshFilter.sharedMesh);
                var remappedUVs = meshFilter.sharedMesh.uv;

                for (var j = 0; j < remappedUVs.Length; j++)
                {
                    var uv = remappedUVs[j];

                    uv.x = rect.x + (uv.x * rect.width);
                    uv.y = rect.y + (uv.y * rect.height);

                    remappedUVs[j] = uv;
                }

                atlasedMesh.uv = remappedUVs;

                // Save the mesh to files
                AssetDatabase.CreateAsset(atlasedMesh, AssetDatabase.GetAssetPath(meshFilter.sharedMesh) + " Atlased.asset");

                meshFilter.sharedMesh = atlasedMesh;
            }
        }
    }

    private static Mesh CopyMesh(Mesh mesh)
    {
        // Duplicate the mesh
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.uv = mesh.uv;
        newMesh.normals = mesh.normals;
        newMesh.colors = mesh.colors;
        newMesh.tangents = mesh.tangents;
        // Return the copy
        return newMesh;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextureAtlasCreator))]
[CanEditMultipleObjects]
public class TextureAtlasCreatorEditor : Editor
{
    TextureAtlasCreator[] scripts;

    void OnEnable()
    {
        scripts = new TextureAtlasCreator[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            scripts[i] = (TextureAtlasCreator)targets[i];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("Create Atlas"))
        {
            foreach(TextureAtlasCreator script in scripts)
                script.Create();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif