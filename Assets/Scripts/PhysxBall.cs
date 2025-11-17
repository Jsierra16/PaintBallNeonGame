using Fusion;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Init(Vector3 forward)
    {
        life = TickTimer.CreateFromSeconds(Runner, 5f);

        // Only the host moves the ball with physics
        if (HasStateAuthority)
        {
            rb.velocity = forward;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
    }

    public void Consume()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        col.enabled = false;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Runner.Despawn(Object);
    }
}
