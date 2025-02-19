using UnityEngine;
using TMPro;
using System.Collections;

public class GameController : MonoBehaviour
{
    public int totalRounds = 5; // Total rounds to be played
    public float timeLimit = 25f; // Time limit per round
    private float timer;
    public int currentRound = 1; // Current round (accessible from other scripts)

    public TextMeshProUGUI timerText; // Timer UI element
    public TextMeshProUGUI roundText; // Round UI element
    public GameObject successOverlay; // Overlay for mission success
    public GameObject gameOverOverlay; // Overlay for game over

    private bool isGameActive = true; // Flag to check if the game is active
    private int lastUpdatedRound = 0; // To prevent unnecessary round UI updates

    void Start()
    {
        // Check if UI elements and overlays are assigned
        if (timerText == null || roundText == null || successOverlay == null || gameOverOverlay == null)
        {
            Debug.LogError("UI elements or overlays are not assigned in the inspector!");
            isGameActive = false;
            return;
        }

        timer = timeLimit; // Initialize timer
        UpdateRoundUI(); // Update initial round UI
        successOverlay.SetActive(false); // Ensure success overlay is hidden
        gameOverOverlay.SetActive(false); // Ensure game over overlay is hidden
    }

    void Update()
    {
        if (isGameActive && currentRound <= totalRounds)
        {
            // Update timer
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Max(0, Mathf.Ceil(timer)).ToString();

            // Update round UI only if the round has changed
            if (lastUpdatedRound != currentRound)
            {
                UpdateRoundUI();
                lastUpdatedRound = currentRound; // Store the last updated round
            }

            // Check if time runs out
            if (timer <= 0)
            {
                EndGame(false); // Game over when time runs out
            }
        }
    }

    // Call this function to move to the next round
    public void NextRound()
    {
        if (!isGameActive) return;

        if (currentRound < totalRounds)
        {
            currentRound++; // Move to next round
            timer = timeLimit; // Reset timer for the new round
        }
        else
        {
            EndGame(true); // Complete the mission if all rounds are finished
        }
    }

    // Show the game status when the game ends
    void EndGame(bool isSuccess)
    {
        if (!isGameActive) return;

        isGameActive = false; // Stop the game
        timer = 0; // Stop the timer

        if (isSuccess)
        {
            // Show success overlay
            successOverlay.SetActive(true);
        }
        else
        {
            // Show game over overlay
            gameOverOverlay.SetActive(true);
        }
    }

    // Update round UI display
    void UpdateRoundUI()
    {
        if (roundText != null)
        {
            roundText.text = "Round: " + currentRound; // Update round number
        }
    }

    // Optional: You can call this from other scripts if needed to reset the game
    public void ResetGame()
    {
        if (!isGameActive) return;

        currentRound = 1; // Reset the round to 1
        timer = timeLimit; // Reset the timer
        UpdateRoundUI(); // Update round UI
        if (timerText != null)
        {
            timerText.text = "Time: " + timeLimit.ToString(); // Reset timer UI
        }

        // Hide overlays
        if (successOverlay != null) successOverlay.SetActive(false);
        if (gameOverOverlay != null) gameOverOverlay.SetActive(false);

        isGameActive = true; // Reactivate the game
    }
}
