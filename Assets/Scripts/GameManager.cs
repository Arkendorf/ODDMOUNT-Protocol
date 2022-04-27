using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public MechController playerMech;

    private EnemyManager enemyManager;

    private void OnEnable()
    {
        if (playerMech)
            playerMech.OnDeath += Lose;

        enemyManager = EnemyManager.Instance;
        enemyManager.EnemyRemoved += CheckWin;
    }

    private void OnDisable()
    {
        if (playerMech)
            playerMech.OnDeath -= Lose;

        enemyManager.EnemyRemoved -= CheckWin;
    }


    public void Lose()
    {
        StartCoroutine(ResetScene());
    }

    private IEnumerator ResetScene()
    {
        // Delay reset
        yield return new WaitForSeconds(5);
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void CheckWin(MechController enemy)
    {
        if (enemyManager.enemies.Count <= 0)
        {
            Win();
        }
    }

    public void Win()
    {
        Debug.Log("You won the level!");
    }
}
