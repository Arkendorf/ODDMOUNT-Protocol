using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarController : MonoBehaviour
{
    [Tooltip("Mech to center the radar on")]
    public MechController center;
    [Tooltip("Transform of the grid")]
    public SpriteRenderer grid;
    [Tooltip("Sprite to use to represent enemies")]
    public Sprite enemySprite;
    [Tooltip("Color to use for enemy sprites")]
    public Color enemyColor;
    [Tooltip("Range, in distance units, this radar covers")]
    public float range = 10;

    // EnemyManager instance in the scene
    private EnemyManager enemyManager;

    // List of sprite renderers rendering enemies on the radar
    private List<SpriteRenderer> enemyRenderers;

    private void OnEnable()
    {
        // Create list for enemy renderers
        enemyRenderers = new List<SpriteRenderer>();

        // Find enemy manager
        enemyManager = EnemyManager.Instance;

        // Subscribe to manager events
        enemyManager.EnemyAdded += OnEnemyAdded;
        enemyManager.EnemyRemoved += OnEnemyRemoved;
    }

    private void OnDisable()
    {
        // Unsubscribe from manager events
        enemyManager.EnemyAdded -= OnEnemyAdded;
        enemyManager.EnemyRemoved -= OnEnemyRemoved;
    }

    // Update is called once per frame
    void Update()
    {
        // Update grid position and rotation
        Vector3 position = - new Vector3((center.mech.position.x / range) % (1.33f), (center.mech.position.z / range) % (1.33f), 0);
        grid.transform.localPosition = position;
        Quaternion rotation = Quaternion.Euler(0, 0, center.mech.transform.eulerAngles.y);
        grid.transform.parent.localRotation = rotation;

        for (int i = 0; i < enemyManager.enemies.Count; i++)
        {
            MechController enemy = enemyManager.enemies[i];
            SpriteRenderer renderer = enemyRenderers[i];
            // Get position difference
            Vector3 delta = enemy.mech.transform.position - center.mech.transform.position;
            delta.y = 0;
            delta /= range;
            // Set marker position
            renderer.transform.localPosition = rotation * new Vector3(delta.x, delta.z, 0);
        }
    }

    private void OnEnemyAdded(MechController enemy)
    {
        // Create marker for the enemy
        GameObject enemyMarker = new GameObject("Enemy");
        enemyMarker.transform.parent = transform;
        enemyMarker.transform.localScale = Vector3.one * .5f;
        enemyMarker.transform.localRotation = Quaternion.identity;
        // Create renderer for the enemy
        SpriteRenderer renderer = enemyMarker.AddComponent<SpriteRenderer>();
        renderer.sprite = enemySprite;
        renderer.color = enemyColor;
        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        renderer.sortingOrder = 1;
        // Add it to the list
        enemyRenderers.Add(renderer);
    }

    private void OnEnemyRemoved(MechController enemy)
    {
        // Get renderer index
        int i = enemyManager.enemies.IndexOf(enemy);
        // Get the renderer, and remove it from the list
        SpriteRenderer renderer = enemyRenderers[i];
        enemyRenderers.RemoveAt(i);
        // Destroy the marker
        Destroy(renderer.gameObject);
    }
}
