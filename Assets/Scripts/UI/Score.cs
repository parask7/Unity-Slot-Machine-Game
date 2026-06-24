using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles player score, betting, balance updates, and symbol payout values.
/// 
/// Responsibilities:
/// - Stores the current player balance
/// - Deducts bet amount before a spin
/// - Prevents betting when balance is insufficient
/// - Adds payout rewards after wins
/// - Provides triple-symbol payout values
/// </summary>
public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Text field used to display the player's current balance.")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Score Settings")]
    [Tooltip("Starting balance given to the player at the beginning of the game.")]
    [SerializeField] private int startingScore = 1000;

    [Tooltip("Amount deducted from the balance for every spin.")]
    [SerializeField] private int betAmount = 100;

    private int currentScore;

    // Payout values for triple-symbol matches.
    private readonly Dictionary<string, int> triplePayouts = new Dictionary<string, int>
    {
        { "bar", 500 },
        { "seven", 1000 },
        { "bell", 300 },
        { "cherry", 200 }
    };

    public int CurrentScore => currentScore;
    public int BetAmount => betAmount;

    private void Awake()
    {
        // Singleton setup so other systems can access score data easily.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple Score instances found. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentScore = startingScore;
        UpdateScoreText();
    }

    /// <summary>
    /// Checks whether the player has enough balance to place the current bet.
    /// </summary>
    public bool CanBet()
    {
        return currentScore >= betAmount;
    }

    /// <summary>
    /// Deducts the bet amount from the player's balance.
    /// Returns false if the player does not have enough balance.
    /// </summary>
    public bool ApplyBet()
    {
        if (!CanBet())
            return false;

        ChangeScore(-betAmount);
        return true;
    }

    /// <summary>
    /// Adds reward amount to the player's current balance.
    /// </summary>
    public void AddScore(int amount)
    {
        ChangeScore(amount);
    }

    /// <summary>
    /// Returns the payout value for a triple match symbol.
    /// </summary>
    public int GetTriplePayout(string symbolName)
    {
        if (string.IsNullOrWhiteSpace(symbolName))
            return 0;

        symbolName = symbolName.ToLowerInvariant();

        return triplePayouts.TryGetValue(symbolName, out int payout) ? payout : 0;
    }

    /// <summary>
    /// Updates the current score and prevents it from going below zero.
    /// </summary>
    private void ChangeScore(int amount)
    {
        currentScore += amount;

        if (currentScore < 0)
            currentScore = 0;

        UpdateScoreText();
    }

    /// <summary>
    /// Refreshes the balance text shown on the UI.
    /// </summary>
    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }
}