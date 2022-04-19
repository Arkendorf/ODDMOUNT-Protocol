using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyTriggerSpawn : MonoBehaviour
{
    [Tooltip("Enemy to spawn")]
    public GameObject enemyPrefab;
    [Tooltip("Where to spawn the enemy")]
    public Transform spawnpoint;

    private new Collider collider;
    private bool spawned;

    // Start is called before the first frame update
    void Start()
    {
        // Get the collider
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If collider has been entered by player and enemy not spawned, spawn it
        if (!spawned && other.CompareTag("Player"))
        {
            EnemyManager.Instance.AddEnemy(enemyPrefab, spawnpoint.position, spawnpoint.rotation);
            spawned = true;
        }
    }
}
