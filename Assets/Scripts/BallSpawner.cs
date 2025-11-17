using Fusion;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float launchForce = 10f;

    public void LaunchBall(Vector3 direction)
    {
        if (Runner == null) return;

        // Spawn the networked ball
        NetworkObject ball = Runner.Spawn(ballPrefab, spawnPoint.position, Quaternion.identity);
        var physBall = ball.GetComponent<PhysxBall>();
        if (physBall != null)
        {
            physBall.Init(direction.normalized * launchForce);
        }
    }
}
