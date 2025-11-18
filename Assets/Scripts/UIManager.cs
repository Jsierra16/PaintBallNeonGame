using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton
    {
        get => _singleton;
        set
        {
            if (value == null)
                _singleton = null;
            else if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}!");
            }
        }
    }
    private static UIManager _singleton;

    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Hit UI")]
    [SerializeField] private GameObject hitsPanel; 
    [SerializeField] private TMP_Text[] playerHitTexts;

    [Header("End Game UI")]
    [SerializeField] private GameObject loserImage;
    [SerializeField] private TextMeshProUGUI loserText;

    private void Awake()
    {
        Singleton = this;

        if (loserImage != null)
            loserImage.SetActive(false);

        if (hitsPanel != null)
            hitsPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    /// <summary>
    /// Called by Player when they press Ready
    /// </summary>
    public void DidSetReady()
    {
        if (instructionText != null)
            instructionText.text = "Waiting for other player to be ready";
    }

    public void SetWaitUI(GameState newState, Player winner = null, Player loser = null)
    {
        if (newState == GameState.Waiting)
        {
            gameStateText.text = winner == null 
                ? "Kill your opponent 10 times" 
                : $"{winner.Name} won!";
            instructionText.text = "Press R when ready!";

            gameStateText.enabled = true;
            instructionText.enabled = true;
        }
        else
        {
            gameStateText.enabled = false;
            instructionText.enabled = false;
        }

        if (loser != null)
        {
            ShowLoserImage(loser.Name);
        }
    }

    public void UpdateHitsUI(int playerNumber, int hits)
    {
        if (playerNumber - 1 < 0 || playerNumber - 1 >= playerHitTexts.Length)
            return;

        playerHitTexts[playerNumber - 1].text = $"{hits}";
    }

    public void ShowLoserImage(string playerName)
    {
        if (loserImage != null)
            loserImage.SetActive(true);

        if (loserText != null)
            loserText.text = $"{playerName} lost!";

        gameStateText.enabled = false;
        instructionText.enabled = false;
    }

    public void HideLoserImage()
    {
        if (loserImage != null)
            loserImage.SetActive(false);
    }

    public void ResetUI()
    {
        if (hitsPanel != null)
        {
            foreach (var text in playerHitTexts)
                text.text = "0";
        }

        HideLoserImage();
    }
}
