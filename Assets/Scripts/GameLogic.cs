using UnityEngine;
using Fusion;
using System.Collections.Generic;

public enum GameState { Waiting, Playing }

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private Transform spawnpoint;
    [SerializeField] private Transform spawnpointPivot;

    [Networked] private Player Loser { get; set; }
    [Networked, OnChangedRender(nameof(GameStateChanged))] private GameState State { get; set; }
    [Networked, Capacity(4)] private NetworkDictionary<PlayerRef, Player> Players => default;

    public override void Spawned()
    {
        Loser = null;
        State = GameState.Waiting;
        UIManager.Singleton.SetWaitUI(State, null, Loser);
    }

    private void GameStateChanged()
    {
        if (State == GameState.Playing)
            UIManager.Singleton.HideLoserImage();

        UIManager.Singleton.SetWaitUI(State, null, Loser);
    }

    public void PlayerLost(Player lostPlayer)
    {
        if (!Runner.IsServer) return; // Only host decides

        Loser = lostPlayer;

        // Update UI to show loser immediately
        UIManager.Singleton.ShowLoserImage(Loser.Name);

        // Switch state to waiting so the round resets
        State = GameState.Waiting;
    }

    private void PreparePlayers()
    {
        float spacingAngle = 360f / Players.Count;
        spawnpointPivot.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        foreach (var kvp in Players)
        {
            GetNextSpawnpoint(spacingAngle, out Vector3 pos, out Quaternion rot);
            kvp.Value.Teleport(pos, rot);
        }
    }

    private void UnreadyAll()
    {
        foreach (var kvp in Players)
            kvp.Value.IsReady = false;
    }

    private void GetNextSpawnpoint(float spacingAngle, out Vector3 position, out Quaternion rotation)
    {
        position = spawnpoint.position;
        rotation = spawnpoint.rotation;
        spawnpointPivot.Rotate(0f, spacingAngle, 0f);
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority) return;

        GetNextSpawnpoint(90f, out Vector3 pos, out Quaternion rot);
        NetworkObject obj = Runner.Spawn(playerPrefab, pos, rot, player);
        Players.Add(player, obj.GetComponent<Player>());
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority) return;

        if (Players.TryGet(player, out Player p))
        {
            Players.Remove(player);
            Runner.Despawn(p.Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Runner.IsServer && other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out Player player))
        {
            UnreadyAll();
            State = GameState.Waiting;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Players.Count < 1) return;

        if (Runner.IsServer && State == GameState.Waiting)
        {
            bool areAllReady = true;
            foreach (var kvp in Players)
            {
                if (!kvp.Value.IsReady)
                {
                    areAllReady = false;
                    break;
                }
            }

            if (areAllReady)
            {
                State = GameState.Playing;
                PreparePlayers();
            }
        }
    }
}
