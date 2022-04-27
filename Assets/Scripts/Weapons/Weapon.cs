using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Tooltip("The mech using this weapon")]
    public MechController mechController;
    [Tooltip("The weapon's origin. Direction of aim for the weapon / origin for projectiles instantiated by this weapon")]
    public Transform origin;
    [Tooltip("Maximum ammo in a clip for this weapon")]
    public int maxAmmo;
    [Tooltip("Maximum range of a hitscan shot")]
    public float range;

    [HideInInspector] public int ammo { get; protected set; }
    [HideInInspector] public bool firing { get; private set; }

    protected virtual void Start()
    {
        ammo = maxAmmo;
    }

    public virtual void StartFire()
    {
        firing = true;
    }

    public virtual void Fire()
    {
    }

    public virtual void StopFire()
    {
        firing = false;
    }
}
