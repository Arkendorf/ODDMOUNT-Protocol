using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Control Properties")]
    [Tooltip("The mech using this weapon")]
    public MechController mechController;
    [Tooltip("The lever to reload this weapon")]
    public LeverControl lever;
    [Tooltip("The counter tracking this weapon's ammo")]
    public CounterController counter;
    [Tooltip("Whether to represent ammo as a percent")]
    public bool percent;
    [Tooltip("The weapon's audio manager")]
    public AudioManager audioManager;
    [Tooltip("Sound to play when weapon is reloaded")]
    public AudioClip reloadSound;
    [Tooltip("Sound to play when the weapon tries to fire but is out of ammo")]
    public AudioClip emptySound;
    [Header("Weapon Properties")]
    [Tooltip("The weapon's origin. Direction of aim for the weapon / origin for projectiles instantiated by this weapon")]
    public Transform origin;
    [Tooltip("Maximum ammo in a clip for this weapon")]
    public int maxAmmo;
    [Tooltip("How fast to reload, in seconds")]
    public float reloadTime = 2;
    [Tooltip("Maximum range of a hitscan shot")]
    public float range;

    public delegate void WeaponEvent();
    public WeaponEvent OnFireShot;


    [HideInInspector] public int ammo { get; protected set; }
    [HideInInspector] public bool firing { get; private set; }
    [HideInInspector] public bool reloading { get; private set; }

    protected virtual void Start()
    {
        // Set lever pull speed so that it will be fully pulled in the amount of time it takes to reload
        if (lever)
            lever.moveSpeed = (lever.maxAngle - lever.minAngle) / reloadTime;

        if (counter)
            counter.percent = percent;

        ReloadImmediate();
    }

    protected virtual void Update()
    {
        if (firing)
        {
            SetCounter();
        }
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
        if (ammo <= 0 && audioManager && emptySound)
            audioManager.Play(emptySound, false, .3f, Random.Range(0.75f, 1.2f));
    }

    public virtual bool Fire()
    {
        return ammo > 0;
    }

    public virtual void StopFire()
    {
        firing = false;
        SetCounter();
    }

    public virtual void ReloadImmediate()
    {
        ammo = maxAmmo;

        if (audioManager && reloadSound)
            audioManager.Play(reloadSound, false, .25f, Random.Range(0.75f, 1.2f));

        SetCounter();
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

    private void SetCounter()
    {
        if (counter)
        {
            if (percent)
                counter.SetValue((ammo * 100) / maxAmmo);
            else
                counter.SetValue(ammo);
        }
    }
}
