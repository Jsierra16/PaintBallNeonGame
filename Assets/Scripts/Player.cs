using System.Numerics;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Player : NetworkBehaviour 
{
    [Header("Movement")]
    [SerializeField] private MeshRenderer[] modelParts;
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpulse = 10f;
    public bool IsReady;

    [Networked] public string Name { get; private set; }

    [Header("Shooting")]
    [SerializeField] private BallSpawner ballSpawner; // assign in inspector
    [SerializeField] private Transform shootPoint;    // assign in inspector

    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] private TickTimer shootDelay { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority)
        {

            foreach (MeshRenderer renderer in modelParts)
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        Runner.GetComponent<InputManager>().LocalPlayer = this;    
        Name = PlayerPrefs.GetString("Photon.Menu.Username");
        RPC_PlayerName(Name);
        CameraFollow.Singleton.SetTarget(camTarget);

        }

      
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


[Rpc(RpcSources.InputAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RPC_SetReady()
    {
        IsReady = true;
        if (HasInputAuthority)
        UIManager.Singleton.DidSetReady();  
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        kcc.SetPosition(position);
        kcc.SetLookRotation(rotation);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name)
    {
        Name = name;
    }

    private void Update()
{
    if (!HasInputAuthority) return; // Only the local player can press keys
    if (UIManager.Singleton == null) return;

    // Restart the game (try again)
    if (Input.GetKeyDown(KeyCode.T))
    {
        HitManager.Instance?.ResetAll();
        var gameLogic = FindObjectOfType<GameLogic>();
        if (gameLogic != null)
            gameLogic.ResetGame(); // Resets positions, ready state, and UI
    }

    // Go back to Main Menu
    if (Input.GetKeyDown(KeyCode.M))
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}


}
