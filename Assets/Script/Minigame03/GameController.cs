using UnityEngine;
using TMPro;
using System.Collections;

public class GameController : MonoBehaviour
{
    public int totalRounds = 5; // Total rounds to be played
    public float timeLimit = 25f; // 20 seconds per round
    private float timer;
    public int currentRound = 1; // Made public to access from other scripts

    public TextMeshProUGUI timerText; // Timer UI element
    public TextMeshProUGUI roundText; // Round UI element
    public GameObject successOverlay; // Overlay for mission success
    public GameObject gameOverOverlay; // Overlay for game over

    void Start()
    {
        timer = timeLimit; // Initialize timer
        UpdateRoundUI(); // Update initial round UI
    }

    void Update()
    {
        if (currentRound <= totalRounds)
        {
            // Update timer and round display
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Max(0, Mathf.Ceil(timer)).ToString();

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
        if (currentRound < totalRounds)
        {
            currentRound++; // Move to next round
            timer = timeLimit; // Reset timer for the new round
            UpdateRoundUI(); // Update the UI for current round
        }
        else
        {
            EndGame(true); // Complete the mission if all rounds are finished
        }
    }

    // Show the game status when the game ends
    void EndGame(bool isSuccess)
    {
        timer = 0; // Stop the timer at the end of the game

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
        roundText.text = "Round: " + currentRound; // Update round number
    }

    // Optional: You can call this from other scripts if needed to reset the game
    public void ResetGame()
    {
        currentRound = 1; // Reset the round to 1
        timer = timeLimit; // Reset the timer
        UpdateRoundUI(); // Update round UI
        timerText.text = "Time: " + timeLimit.ToString(); // Reset timer UI

        // Hide overlays
        successOverlay.SetActive(false);
        gameOverOverlay.SetActive(false);
    }
}
