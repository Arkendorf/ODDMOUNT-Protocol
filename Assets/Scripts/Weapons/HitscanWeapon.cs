using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanWeapon : Weapon
{
    [Header("Component Paramters")]
    [Tooltip("Rigidbody holding this weapon")]
    public new Rigidbody rigidbody;
    [Tooltip("Origin and direction of shots")]
    public Transform origin;
    [Header("Shot Parameters")]
    [Tooltip("Amount of damage dealt by one shot from this weapon")]
    public float damage;
    [Tooltip("Maximum range of a hitscan shot")]
    public float range;
    [Tooltip("Magnitude of force to apply to target hit by shots from this weapon")]
    public float hitForce;
    [Tooltip("Magnitude of recoil to apply to the firer of this weapon")]
    public float recoil;
    [Header("Fire Rate Parameters")]
    [Tooltip("Initial time between trigger press and first shot")]
    public float spoolTime;
    [Tooltip("Time between shots within a burst")]
    public float fireRate;
    [Tooltip("Number of shots in  burst")]
    public int burstCount;
    [Tooltip("Time between bursts")]
    public float burstDelay;
    [Header("Spray Parameters")]
    public float minSpray;
    public float maxSpray;
    [Header("Particle Properties")]
    public ParticleSystem shotSystem;

    // Current delay before the next shot
    private float delay;
    // Number of shots in the current burst
    private int burst;


    protected override void Start()
    {
        base.Start();

        if (!rigidbody)
        {
            rigidbody = GetComponentInParent<Rigidbody>();
        }
    }

    private void Update()
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
        }
    }

    public override void StartFire()
    {
        base.StartFire();

        if (shotSystem)
            shotSystem.Play();       

        delay = spoolTime;
        burst = 0;
    }

    public override void Fire()
    {
        base.StartFire();

        // Fire a hitscan if at least one ammo and delay is up
        if (ammo > 0 && delay <= 0)
        {
            FireShot();
            ammo--;
            burst++;
            if (burst >= burstCount)
            {
                burst = 0;
                delay = burstDelay;
            }
            else
            {
                delay = fireRate;
            }
        }
    }

    public override void EndFire()
    {
        base.EndFire();

        if (shotSystem)
            shotSystem.Stop();
    }

    protected virtual void FireShot()
    {
        // Create spray
        float spray = Random.Range(0, Mathf.Lerp(minSpray, maxSpray, burst / Mathf.Max(1, (float)burstCount - 1)));
        Quaternion rot = origin.rotation * Quaternion.Euler(0, 0, Random.Range(0, 360)) * Quaternion.Euler(spray, 0, 0);

        // Get forward direction for shot
        Vector3 dir = rot * Vector3.forward;

        float distance = range;
        if (Physics.Raycast(origin.position, dir, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            distance = hit.distance;

            if (hit.rigidbody)
            {
                // Add force to rigidbody
                hit.rigidbody.AddForce(dir * hitForce);

                // If we hit a mech, do damage
                if (hit.rigidbody.CompareTag("Enemy") || hit.rigidbody.CompareTag("Player"))
                {
                    MechController mechController = hit.rigidbody.GetComponentInParent<MechController>();
                    // If enemy found, add shot damage
                    if (mechController)
                    {
                        mechController.AddDamage(damage);
                    }
                }
                
            }
        }

        // Add recoil
        if (rigidbody)
            rigidbody.AddForce(-dir * recoil);

        // Shot particles
        if (shotSystem)
        {
            // Get the particle system's main module
            ParticleSystem.MainModule main = shotSystem.main;

            // Rotate particle to face target
            shotSystem.transform.rotation = rot;

            // Elongate particle to reach target
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                startSize3D = new Vector3(main.startSizeX.constant, distance, main.startSizeZ.constant)
            };

            // Play the particle
            shotSystem.Emit(emitParams, 1);
        }
    }
}
