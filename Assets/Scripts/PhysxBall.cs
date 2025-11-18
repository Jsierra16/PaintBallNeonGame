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

    private void OnEnable()
    {
        // Register with ObjectManager so all clients know about it
        if (ObjectManager.Singleton != null)
            ObjectManager.Singleton.RegisterObject(gameObject);
        
        col.enabled = true;
        rb.isKinematic = false;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
    }

    private void OnDisable()
    {
        if (ObjectManager.Singleton != null)
            ObjectManager.Singleton.UnregisterObject(gameObject);
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
            Consume();
    }

    public void Consume()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        col.enabled = false;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Remove from ObjectManager
        if (ObjectManager.Singleton != null)
            ObjectManager.Singleton.UnregisterObject(gameObject);

        // Despawn network object
        if (Runner != null && Object != null)
            Runner.Despawn(Object);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return; // Only server counts hits

        var hitPlayer = other.GetComponent<PlayerHitDetectorTrigger>();
        if (hitPlayer != null)
        {
            HitManager.Instance?.RegisterHitForPlayer(hitPlayer);
            Consume();
        }
    }
}
