using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Singleton instance
    private static EnemyManager _enemyManager;
    public static EnemyManager Instance
    {
        get
        {
            if (!_enemyManager)
            {
                _enemyManager = FindObjectOfType(typeof(EnemyManager)) as EnemyManager;
            }
            return _enemyManager;
        }
    }
    [Tooltip("Target for enemies")]
    public Transform enemyTarget;
    [HideInInspector] public List<MechController> enemies { get; private set; }
    [Tooltip("Default enemy prefab")]
    public GameObject defaultPrefab;

    public delegate void EnemyEvent(MechController enemy);
    public EnemyEvent EnemyAdded;
    public EnemyEvent EnemyRemoved;

    void Awake()
    {
        enemies = new List<MechController>();
    }

    public MechController AddEnemy(GameObject prefab) { return AddEnemy(prefab, Vector3.zero, Quaternion.identity); }
    public MechController AddEnemy(GameObject prefab, Vector3 position) { return AddEnemy(prefab, position, Quaternion.identity); }
    public MechController AddEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Create the enemy
        GameObject mech = Instantiate(prefab ?? defaultPrefab, position, rotation);
        // Get it's mech controller
        MechController mechController = mech.GetComponent<MechController>();
        // If mech controller exists, add it to enemies and return it
        if (mechController)
        {
            // Set enemy target
            MechNavMeshInput mechInput = mech.GetComponent<MechNavMeshInput>();
            if (mechInput)
                mechInput.target = enemyTarget;
            // Add enemy to the list
            enemies.Add(mechController);
            // invoke event
            EnemyAdded?.Invoke(mechController);
            // Return the enemy
            return mechController;
        }
        else
        {
            Destroy(mech);
            return null;
        }
    }

    public void RemoveEnemy(GameObject mech, bool destroy = true)
    {
        // Get it's mech controller
        MechController mechController = mech.GetComponent<MechController>();
        RemoveEnemy(mechController, destroy);
    }

    public void RemoveEnemy(MechController mechController, bool destroy = true)
    {
        // Remove it from the list
        if (enemies.Contains(mechController))
        {
            // invoke event
            EnemyRemoved?.Invoke(mechController);
            // Remove from the list
            enemies.Remove(mechController);        
        }
        // Delete the mech if requested
        if (destroy)
        {
            Destroy(mechController.gameObject);
        }
    }
}
