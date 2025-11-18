using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;


public class HitManager : NetworkBehaviour
{
    public static HitManager Instance;

    [Header("Hit Settings")]
    public int hitsToLose = 10;

    [Header("UI")]
    public UIManager uiManager;
    public LayerMask playerLayer;

    private Dictionary<int, PlayerHitDetectorTrigger> players = new Dictionary<int, PlayerHitDetectorTrigger>();
    private Dictionary<int, int> playerHits = new Dictionary<int, int>();
    private Dictionary<int, int> playerNumberById = new Dictionary<int, int>();

    private Queue<int> availableNumbers = new Queue<int>();
    private int nextNumber = 1;

    private bool gameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int RegisterPlayer(PlayerHitDetectorTrigger player)
    {
        if (player == null) return -1;

        int id = player.GetInstanceID();
        if (!players.ContainsKey(id))
        {
            players.Add(id, player);
            playerHits[id] = player.currentHits;

            int assigned = availableNumbers.Count > 0 ? availableNumbers.Dequeue() : nextNumber++;
            playerNumberById[id] = assigned;

            player.playerName = $"Player {assigned}";
        }
        else
        {
            players[id] = player;
            if (!playerNumberById.ContainsKey(id))
            {
                int assigned = availableNumbers.Count > 0 ? availableNumbers.Dequeue() : nextNumber++;
                playerNumberById[id] = assigned;
                player.playerName = $"Player {assigned}";
            }
        }

        uiManager.UpdateHitsUI(playerNumberById[id], playerHits[id]);
        return playerNumberById[id];
    }

    public void UnregisterPlayer(PlayerHitDetectorTrigger player)
    {
        if (player == null) return;
        int id = player.GetInstanceID();
        if (playerNumberById.TryGetValue(id, out int number))
            availableNumbers.Enqueue(number);

        players.Remove(id);
        playerHits.Remove(id);
        playerNumberById.Remove(id);
    }

    public void RegisterHitForPlayer(PlayerHitDetectorTrigger player)
{
    if (gameOver || player == null) return;

    int id = player.GetInstanceID();
    if (!playerHits.ContainsKey(id))
        playerHits[id] = player.currentHits;

    playerHits[id]++;
    player.currentHits = playerHits[id];

    int number = playerNumberById[id];
    uiManager.UpdateHitsUI(number, playerHits[id]);

    Debug.Log($"{player.playerName} got hit! Total hits: {playerHits[id]}");

    if (playerHits[id] >= hitsToLose)
    {
        gameOver = true;

        // Get the actual Player object
        Player playerObj = player.playerComponent;
        if (playerObj != null)
        {
            // Freeze the player by disabling the KCC
            var kcc = playerObj.GetComponent<SimpleKCC>();
            if (kcc != null)
                kcc.enabled = false;

            // Disable shooting
            if (playerObj.GetComponent<BallSpawner>() != null)
                playerObj.GetComponent<BallSpawner>().enabled = false;

            // Optional: mark as not ready
            playerObj.IsReady = false;
        }

        // Notify GameLogic
        var logic = FindObjectOfType<GameLogic>();
        if (logic != null)
            logic.PlayerLost(playerObj);

        // Show end game UI
        uiManager.ShowEndGameOptions(player.playerName);
    }
}

    public void ResetAll()
    {
        gameOver = false;
        playerHits.Clear();
        playerNumberById.Clear();
        players.Clear();
        availableNumbers.Clear();
        nextNumber = 1;

        uiManager.ResetUI();
    }
}
