using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Singleton instance
    private static MusicManager _musicManager;
    public static MusicManager Instance
    {
        get
        {
            if (!_musicManager)
            {
                _musicManager = FindObjectOfType(typeof(MusicManager)) as MusicManager;
            }
            return _musicManager;
        }
    }

    public AudioSource combatMusic;
    public float volume = 1;
    public float fadeSpeed = 4;
    private bool fading;

    // Update is called once per frame
    void Update()
    {
        bool combat = false;
        foreach (MechController enemy in EnemyManager.Instance.enemies)
        {
            MechNavMeshInput input = enemy.GetComponent<MechNavMeshInput>();
            if (input.active)
            {
                combat = true;
                break;
            }        
        }

        if (combat && !combatMusic.isPlaying)
        {
            combatMusic.Play();
            combatMusic.volume = volume;
            fading = false;
        }
        else if (!combat && combatMusic.isPlaying)
        {
            fading = true;
        }

        if (fading)
        {
            combatMusic.volume -= Time.deltaTime * (fadeSpeed * volume);
            if (combatMusic.volume <= 0)
                combatMusic.Stop();
        }
    }
}
