using Fusion;
using UnityEngine;
using TMPro;

public class PlayerCollision : NetworkBehaviour
{
    [Header("Player Settings")]
    public int maxHits = 3;

    [Networked] public int currentHits { get; private set; }

    [Header("UI")]
    public TMP_Text gameOverText;

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority)
            return; // Only the host handles hit logic

        var ball = collision.gameObject.GetComponent<PhysxBall>();
        if (ball != null)
        {
            currentHits++;
            ball.Consume();

            Debug.Log($"{name} hit! Total hits: {currentHits}");

            if (currentHits >= maxHits)
                GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log($"{name} reached max hits! Game Over!");

        if(gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"{name} reached {maxHits} hits!\nGame Over!";
        }

        // Optional: disable player
    }
}
