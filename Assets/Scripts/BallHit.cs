using UnityEngine;

public class BallHit : MonoBehaviour
{
    [HideInInspector] public bool consumed = false;

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Consume()
    {
        if (consumed) return;
        consumed = true;

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Destroy(gameObject, 0.05f);
    }
}
