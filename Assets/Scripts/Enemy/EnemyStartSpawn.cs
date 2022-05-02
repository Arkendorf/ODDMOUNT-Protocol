using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStartSpawn : MonoBehaviour
{
    [Tooltip("Enemy to spawn on start at this GameObject's position and rotation")]
    public GameObject enemyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // Spawn enemy
        EnemyManager.Instance.AddEnemy(enemyPrefab, transform.position, transform.rotation);
    }
}
