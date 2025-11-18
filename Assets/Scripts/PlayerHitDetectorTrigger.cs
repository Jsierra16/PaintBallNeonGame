using UnityEngine;
using Fusion;

public class PlayerHitDetectorTrigger : NetworkBehaviour
{
    [Header("Player Info")]
    public string playerName;
    public int currentHits = 0;

    [Header("Optional")]
    public Player playerComponent; // networked player component

    private void Awake()
    {
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

        // Hit by another player
        if (((1 << other.gameObject.layer) & HitManager.Instance.playerLayer) != 0)
        {
            var hitPlayer = other.GetComponent<PlayerHitDetectorTrigger>();
            if (hitPlayer != null && hitPlayer != this)
            {
                // THIS player gets hit
                HitManager.Instance.RegisterHitForPlayer(this);
            }
        }

        // Hit by a PhysxBall
        var ball = other.GetComponent<PhysxBall>();
        if (ball != null)
        {
            HitManager.Instance.RegisterHitForPlayer(this);
            ball.Consume(); // remove the ball
            if (ObjectManager.Singleton != null)
                ObjectManager.Singleton.UnregisterObject(ball.gameObject);
        }
    }

    public void OnLoseAndDestroy()
    {
        // Optional: play death animation
        gameObject.SetActive(false);
    }
}
