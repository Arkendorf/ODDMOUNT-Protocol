using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LightingEffect : MonoBehaviour
{
    public Color lightColor;
    private Renderer[] renderers;
    private MaterialPropertyBlock block;
    private bool active;
    // Start is called before the first frame update
    void Start()
    {
        block = new MaterialPropertyBlock();
        block.SetColor("_EmissionColor", Color.black);
        Renderer[] mesh = GetComponentsInChildren<MeshRenderer>();
        Renderer[] skinned = GetComponentsInChildren<SkinnedMeshRenderer>();
        renderers = mesh.Concat(skinned).ToArray();
        SetLight(false);
    }

    public void ToggleLight()
    {
        SetLight(!active);
    }

    public void SetLight(bool active)
    {
        this.active = active;
        if (active)
            block.SetColor("_EmissionColor", lightColor);
        else
            block.SetColor("_EmissionColor", Color.black);
        updateRenderers();
    }

    private void updateRenderers()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.SetPropertyBlock(block);
        }
    }
}
