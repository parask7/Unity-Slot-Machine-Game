using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int startingScore = 1000;
    [SerializeField] private int betAmount = 100;

    private int currentScore;
    private readonly Dictionary<string, int> triplePayouts = new Dictionary<string, int>
    {
        { "bar", 500 },
        { "seven", 1000 },
        { "bell", 300 },
        { "cherry", 200 }
    };

    private void Awake()
    {
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

    public int CurrentScore => currentScore;
    public int BetAmount => betAmount;

    public void ApplyBet()
    {
        ChangeScore(-betAmount);
    }

    public void AddScore(int amount)
    {
        ChangeScore(amount);
    }

    public int GetTriplePayout(string symbolName)
    {
        if (string.IsNullOrWhiteSpace(symbolName))
            return 0;

        symbolName = symbolName.ToLowerInvariant();
        return triplePayouts.TryGetValue(symbolName, out int payout) ? payout : 0;
    }

    private void ChangeScore(int amount)
    {
        currentScore += amount;
        if (currentScore < 0)
            currentScore = 0;

        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }
}
