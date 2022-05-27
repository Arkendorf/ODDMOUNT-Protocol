using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanWeapon : Weapon
{
    [Space()]
    [Tooltip("Rigidbody holding this weapon")]
    public new Rigidbody rigidbody;
    [Header("Shot Parameters")]
    [Tooltip("Amount of damage dealt by one shot from this weapon")]
    public float damage;
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
    [Header("Effect Properties")]
    public ParticleSystem tracer;
    public ParticleSystem muzzleFlash;
    public AudioClip shotSound;
    public LightingEffect lightFlash;

    // Current delay before the next shot
    private float delay;
    private float maxDelay;
    // Number of shots in the current burst
    private int burst;

    private float flashDelay;


    protected override void Start()
    {
        base.Start();

        if (!rigidbody)
        {
            rigidbody = GetComponentInParent<Rigidbody>();
        }

        // How long muzzle flash lasts
        flashDelay = muzzleFlash ? muzzleFlash.main.startLifetime.constant : .02f;

        if (tracer)
        {
            ParticleSystemRenderer renderer = tracer.GetComponent<ParticleSystemRenderer>();
            renderer.maxParticleSize = range;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (delay > 0)
        {
            delay -= Time.deltaTime;
        }

        if (lightFlash && delay <= Mathf.Max(0, maxDelay - flashDelay))
        {
            lightFlash.SetLight(false);
        }
    }

    public override void StartFire()
    {
        base.StartFire();

        delay = spoolTime;
        burst = 0;

        // Play particles
        if (tracer)
            tracer.Play();
        if (muzzleFlash)
            muzzleFlash.Play();
    }

    public override bool Fire()
    {
        if (base.Fire())
        {
            // Fire a hitscan and delay is up
            if (delay <= 0)
            {
                FireShot();
                ammo--;
                burst++;
                if (burst >= burstCount)
                {
                    burst = 0;
                    maxDelay = burstDelay;
                }
                else
                {
                    maxDelay = fireRate;
                }
                delay = maxDelay;

            }
            return true;
        }
        return false;
    }

    public override void StopFire()
    {
        base.StopFire();

        // Stop particles
        if (tracer)
            tracer.Stop();
        if (muzzleFlash)
            muzzleFlash.Stop();
    }

    protected virtual void FireShot()
    {
        OnFireShot?.Invoke();

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
                    MechController hostileMech = hit.rigidbody.GetComponentInParent<MechController>();
                    // If enemy found, add shot damage
                    if (hostileMech)
                    {
                        mechController.DealDamage(hostileMech, damage, MechController.MechDamageType.Projectile);
                    }
                }
                
            }
        }

        // Add recoil
        if (rigidbody)
            rigidbody.AddForce(-dir * recoil);

        // Shot particles
        if (tracer)
        {
            // Get the particle system's main module
            ParticleSystem.MainModule main = tracer.main;

            // Rotate particle to face target
            tracer.transform.rotation = rot;

            // Elongate particle to reach target
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                startSize3D = new Vector3(main.startSizeX.constant, distance, main.startSizeZ.constant)
            };

            // Play the particle
            tracer.Emit(emitParams, 1);
        }

        if (muzzleFlash)
        {
            // Play the particle
            muzzleFlash.Emit(1);
        }

        // Play audio
        if (audioManager)
        {
            audioManager.Play(shotSound, false, .3f, Random.Range(0.75f, 1.2f));
        }

        // Play lighting Effect
        if (lightFlash)
        {
            lightFlash.SetLight(true);
        }
    }
}
