using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechHandController : MonoBehaviour
{
    [Tooltip("How fast the hand moves for collision effects to occur")]
    public float velocityThreshold = 1;
    [Header("Particle Properties")]
    public ParticleSystem burstSystem;
    public ParticleSystem[] dragSystems;
    private int collisions;
    [Header("Audio Properties")]
    public new AudioSource audio;
    public AudioClip punchSound;
    public AudioClip scrapeSound;

    private new Rigidbody rigidbody;

    // Audio effect info
    private float punchNoiseReduction = 2f;
    private float scrapeNoiseReduction = 10;
    private float scrapePitchScale = .8f;
    private float scrapeFadeSpeed = 2;
    private float goalScrapeVolume;

    // Start is called before the first frame update
    void Start()
    {
        collisions = 0;

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
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisions++;

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

        if (rigidbody.velocity.sqrMagnitude > velocityThreshold * velocityThreshold)
        {
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
                audio.volume = (rigidbody.velocity.magnitude - velocityThreshold) / punchNoiseReduction;
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
            goalScrapeVolume = Mathf.Max(0, rigidbody.velocity.magnitude - velocityThreshold) / scrapeNoiseReduction;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collisions--;
        if (collisions <= 0)
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
