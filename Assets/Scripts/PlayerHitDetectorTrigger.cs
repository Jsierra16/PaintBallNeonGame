using Fusion;
using UnityEngine;

public class PlayerHitDetectorTrigger : NetworkBehaviour
{
    [HideInInspector] public int currentHits = 0;
    public int maxHits = 3;
    public string playerName = "Player";

    private void OnTriggerEnter(Collider other)
    {
        BallHit ball = other.GetComponent<BallHit>();
        if (ball != null && !ball.consumed)
        {
            ball.Consume();
            
            // Register hit via HitManager
            HitManager.Instance.RegisterHitForPlayer(this);
        }
    }

    // Called by HitManager when player loses
    public void OnLoseAndDestroy()
    {
        // Optional: play death animation, disable player input
        gameObject.SetActive(false);
    }

    public void UpdateHitsUI()
    {
        // Optional: update player UI element
        // Example: health bar, text, etc.
    }
}
