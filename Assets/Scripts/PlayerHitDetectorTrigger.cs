using UnityEngine;
using Fusion;

public class PlayerHitDetectorTrigger : NetworkBehaviour
{
    [Header("Player Info")]
    public string playerName;
    public int currentHits = 0;

    [Header("Optional")]
    public Player playerComponent; // assign Player network component on prefab

    private void Awake()
    {
        // Register with HitManager
        if (HitManager.Instance != null)
            HitManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (HitManager.Instance != null)
            HitManager.Instance.UnregisterPlayer(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return; // only server counts hits

        // Detect collision with other players
        if (((1 << other.gameObject.layer) & HitManager.Instance.playerLayer) != 0)
        {
            var hitPlayer = other.GetComponent<PlayerHitDetectorTrigger>();
            if (hitPlayer != null && hitPlayer != this)
                HitManager.Instance.RegisterHitForPlayer(hitPlayer);
        }
    }

    // Called by HitManager when player reaches hitsToLose
    public void OnLoseAndDestroy()
    {
        // Optional: play death animation, disable player
        gameObject.SetActive(false);
    }
}
