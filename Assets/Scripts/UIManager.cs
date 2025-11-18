using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (value == null) _singleton = null;
            else if (_singleton == null) _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}!");
            }
        }
    }
    private static UIManager _singleton;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Hit & Winner UI")]
    [SerializeField] private GameObject winnerImage; 
    [SerializeField] private GameObject hitsPanel; 
    [SerializeField] private TMP_Text[] playerHitTexts;  // one slot per player

    private void Awake()
    {
        Singleton = this;

        if (winnerImage != null) winnerImage.SetActive(false);
        if (hitsPanel != null) hitsPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Singleton == this) Singleton = null;
    }

    public void DidSetReady()
    {
        instructionText.text = "Waiting for other player to be ready";
    }

    public void SetWaitUI(GameState newState, Player winner, Player loser = null)
    {
        if (newState == GameState.Waiting)
        {
            if (loser != null)
            {
                ShowWinnerImage(loser.Name);
                return;
            }
            else if (winner != null)
            {
                ShowWinnerImage(winner.Name);
                return;
            }
            else
            {
                gameStateText.text = "Kill your opponent 10 times";
                instructionText.text = "Press R when ready";
            }
        }

        bool active = newState == GameState.Waiting;
        gameStateText.enabled = active;
        instructionText.enabled = active;
        if (hitsPanel != null) hitsPanel.SetActive(active);
    }

    public void UpdateHitsUI(int playerNumber, int hits)
    {
        if (playerNumber - 1 < 0 || playerNumber - 1 >= playerHitTexts.Length) return;
        playerHitTexts[playerNumber - 1].text = hits.ToString();
    }

    public void ShowWinnerImage(string name)
    {
        if (winnerImage != null) winnerImage.SetActive(true);

        gameStateText.enabled = false;
        instructionText.enabled = false;
        if (hitsPanel != null) hitsPanel.SetActive(false);
    }

    public void HideWinnerImage()
    {
        if (winnerImage != null) winnerImage.SetActive(false);
        if (hitsPanel != null) hitsPanel.SetActive(true);
    }

    public void ResetUI()
    {
        HideWinnerImage();
        foreach (var text in playerHitTexts)
            text.text = "0";
    }
}
