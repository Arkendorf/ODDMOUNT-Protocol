using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Range(0, 256)]
    public int priority = 128;
    [Range(-1, 1)]
    public float stereoPan;
    [Range(0, 1)]
    public float spatialBlend;
    private List<AudioSource> audios;

    // Start is called before the first frame update
    void Start()
    {
        audios = new List<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = audios.Count - 1; i >= 0; i--)
        {
            if (!audios[i].isPlaying)
            {
                Stop(audios[i]);
            }
        }
    }

    public AudioSource Play(AudioClip clip, bool loop, float volume = 1, float pitch = 1)
    {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.volume = volume;
        audio.pitch = pitch;
        audio.priority = priority;
        audio.panStereo = stereoPan;
        audio.spatialBlend = 0;
        audio.loop = loop;

        audio.clip = clip;
        audio.Play();

        audios.Add(audio);
        return audio;
    }

    public void Stop(AudioSource audio)
    {
        audio.Stop();
        if (audios.Contains(audio))
        {
            audios.Remove(audio);
        }
        Destroy(audio);
    }
}
