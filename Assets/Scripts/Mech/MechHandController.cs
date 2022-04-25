using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHandController : MonoBehaviour
{
    public MechController mechController;
    [Tooltip("How fast the hand moves for collision effects to occur")]
    public float velocityThreshold = .5f;
    [Tooltip("Scale of hand's velocity to apply to colliding rigidbody on punch")]
    public float punchMagnitude = 1;
    [Tooltip("Scale of punch magnitude to apply to enemies as damage")]
    public float punchDamage = 10;
    [Header("Particle Properties")]
    public ParticleSystem burstSystem;
    public ParticleSystem[] dragSystems;
    private List<Collider> collisions;
    [Header("Audio Properties")]
    public new AudioSource audio;
    public AudioClip punchSound;
    public AudioClip scrapeSound;

    private new Rigidbody rigidbody;

    private Vector3 prevVelocity;
    private Vector3 prevPrevVelocity;

    // Audio effect info
    private float punchNoiseReduction = 2f;
    private float scrapeNoiseReduction = 10;
    private float scrapePitchScale = .8f;
    private float scrapeFadeSpeed = 2;
    private float goalScrapeVolume;

    // Start is called before the first frame update
    void Start()
    {
        collisions = new List<Collider>();

        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Fade out audio
        if (audio && audio.clip == scrapeSound)
        {
            audio.volume += (goalScrapeVolume - audio.pitch) * Time.deltaTime * scrapeFadeSpeed;
            goalScrapeVolume -= Time.deltaTime * scrapeFadeSpeed;
            if (goalScrapeVolume <= 0)
            {
                audio.Stop();
            }

            audio.volume = goalScrapeVolume;
            audio.pitch = 1 + audio.volume * scrapePitchScale;
        }

        // Check for colliders on objects that were deleted
        for (int i = collisions.Count - 1; i >= 0; i--)
        {
            Collider collider = collisions[i];
            if (collider == null || collider.gameObject == null)
            {
                collisions.RemoveAt(i);
                OnCollisionExit(null);
            }
        }
    }

    private void FixedUpdate()
    {
        prevPrevVelocity = prevVelocity;
        prevVelocity = rigidbody.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisions.Add(collision.collider);

        // Get contact point
        ContactPoint point = collision.GetContact(0);
        // Play drag particles
        foreach (ParticleSystem system in dragSystems)
        {
            if (!system.isPlaying)
            {
                UpdateTransform(system.transform, point);
                system.Play();
            }
        }

        if (prevPrevVelocity.sqrMagnitude > velocityThreshold * velocityThreshold)
        {
            // Get punch velocity magnitude
            float magnitude = prevPrevVelocity.magnitude;

            if (collision.rigidbody)
            {
                // Do damage if we've hit an enemy
                if (collision.rigidbody.CompareTag("Enemy"))
                {
                    MechController enemyController = collision.rigidbody.GetComponentInParent<MechController>();
                    // If enemy found, add punch damage
                    if (enemyController)
                    {
                        enemyController.AddDamage(magnitude * punchDamage);
                    }
                }

                // If we've hit another rigidbody, add velocity to it to dramatize the punch
                if (!collision.rigidbody.CompareTag("Player"))
                {
                    // Get force to apply - combination of hand velocity and direction away from player
                    Vector3 lookDir = (collision.rigidbody.position - mechController.mech.position).normalized;
                    Vector3 force = Vector3.Lerp(prevPrevVelocity / magnitude, lookDir, .5f) * magnitude * punchMagnitude;
                    // Apply velocity
                    collision.rigidbody.AddForce(force);
                }
            }      

            // Do burst
            if (burstSystem)
            {
                // Set burst position to contact point
                UpdateTransform(burstSystem.transform, point);
                // Play burst particles
                burstSystem.Play();
            }

            // Sound
            if (audio)
            {
                audio.pitch = Random.Range(0.75f, 1.2f);
                audio.volume = (magnitude - velocityThreshold) / punchNoiseReduction;
                audio.time = 0;
                audio.loop = false;
                audio.clip = punchSound;
                audio.Play();
            }
        }    
    }

    private void OnCollisionStay(Collision collision)
    {
        // Get contact point
        ContactPoint point = collision.GetContact(0);
        // Set burst position to contact point
        if (burstSystem)
            UpdateTransform(burstSystem.transform, point);
        // Update drag positions
        foreach (ParticleSystem system in dragSystems)
        {
            UpdateTransform(system.transform, point);
        }

        if (!audio.isPlaying)
        {
            audio.time = Random.Range(0, scrapeSound.length);
            audio.loop = true;
            audio.clip = scrapeSound;
            audio.Play();
        }
        if (audio.clip == scrapeSound)
        {
            goalScrapeVolume = Mathf.Max(0, prevPrevVelocity.magnitude - velocityThreshold) / scrapeNoiseReduction;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision != null && collisions.Contains(collision.collider))
            collisions.Remove(collision.collider);

        if (collisions.Count <= 0)
        {
            foreach (ParticleSystem system in dragSystems)
            {
                if (system.isPlaying)
                {
                    system.Stop();
                }
            }
        }
    }

    private void UpdateTransform(Transform transform, ContactPoint point)
    {
        transform.position = point.point;
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, point.normal), point.normal);
    }
}
