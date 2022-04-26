using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int maxAmmo;
    protected int ammo;

    protected virtual void Start()
    {
        ammo = maxAmmo;
    }

    public virtual void StartFire()
    {
    }

    public virtual void Fire()
    {
    }

    public virtual void EndFire()
    {
    }
}
