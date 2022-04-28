using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Tooltip("The mech using this weapon")]
    public MechController mechController;
    [Tooltip("The weapon's audio manager")]
    public AudioManager audioManager;
    public AudioClip reloadSound;
    [Tooltip("The weapon's origin. Direction of aim for the weapon / origin for projectiles instantiated by this weapon")]
    public Transform origin;
    [Tooltip("The lever to reload this weapon")]
    public LeverControl lever;
    [Tooltip("Maximum ammo in a clip for this weapon")]
    public int maxAmmo;
    [Tooltip("How fast to reload, in seconds")]
    public float reloadTime = 2;
    [Tooltip("Maximum range of a hitscan shot")]
    public float range;


    [HideInInspector] public int ammo { get; protected set; }
    [HideInInspector] public bool firing { get; private set; }
    [HideInInspector] public bool reloading { get; private set; }

    protected virtual void Start()
    {
        ReloadImmediate();

        // Set lever pull speed so that it will be fully pulled in the amount of time it takes to reload
        if (lever)
            lever.moveSpeed = (lever.maxAngle - lever.minAngle) / reloadTime;
    }

    private void OnEnable()
    {
        if (lever)
            lever.OnPulled += ReloadImmediate;
    }

    private void OnDisable()
    {
        if (lever)
            lever.OnPulled -= ReloadImmediate;
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

    public virtual void ReloadImmediate()
    {
        ammo = maxAmmo;
        audioManager.Play(reloadSound, false, .25f, Random.Range(0.75f, 1.2f));
    }

    public void Reload()
    {
        if (!reloading)
        {
            reloading = true;
            StartCoroutine(ReloadHelper());
        }
    }

    private IEnumerator ReloadHelper()
    {
        yield return new WaitForSeconds(reloadTime);
        ReloadImmediate();
        reloading = false;
    }
}
