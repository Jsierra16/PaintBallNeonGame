using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

public class Player : NetworkBehaviour 
{
    [Header("Movement")]
    [SerializeField] private MeshRenderer[] modelParts;
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpulse = 10f;

    [Header("Shooting")]
    [SerializeField] private BallSpawner ballSpawner; // assign in inspector
    [SerializeField] private Transform shootPoint;    // assign in inspector

    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] private TickTimer shootDelay { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority)
            CameraFollow.Singleton.SetTarget(camTarget);

        foreach (MeshRenderer renderer in modelParts)
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetInput input)) return;

        // --- Movement & Look ---
        kcc.AddLookRotation(input.LookDelta * lookSensitivity);
        UpdateCamTarget();

        Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
        float jump = 0f;
        if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump) && kcc.IsGrounded)
            jump = jumpImpulse;

        kcc.Move(worldDirection.normalized * speed, jump);
        PreviousButtons = input.Buttons;

        // --- Shooting ---
        if (HasStateAuthority && shootDelay.ExpiredOrNotRunning(Runner))
        {
            // Use bit 0 for left click
            bool shoot = input.Buttons.IsSet((InputButton)0);
            if (shoot && ballSpawner != null && shootPoint != null)
            {
                shootDelay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                // Spawn the ball forward from ShootPoint
                Vector3 direction = shootPoint.forward;
                ballSpawner.LaunchBall(direction);
            }
        }
    }

    public override void Render()
    {
        UpdateCamTarget();
    }

    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }
}
