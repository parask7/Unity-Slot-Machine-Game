using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a single slot machine reel.
/// 
/// Responsibilities:
/// - Spawns random symbols
/// - Handles reel spinning animation
/// - Stops reels sequentially
/// - Records the final middle symbol
/// - Evaluates results after all reels stop
/// </summary>
public class ReelController : MonoBehaviour
{
    [Header("Reel Symbol Prefabs")]
    [Tooltip("List of possible symbols that can appear on this reel.")]
    public GameObject[] symbolPrefabs;

    [Header("Reel Slot Parent")]
    [Tooltip("Parent transform where reel symbols are spawned.")]
    public Transform middleSlot;

    [Header("Top/Bottom Offsets")]
    [Tooltip("Local Y position for symbols spawning above the middle slot.")]
    public float topYOffset = 1f;

    [Tooltip("Local Y position for symbols below the middle slot.")]
    public float bottomYOffset = -1f;

    [Header("Spin Settings")]
    [Tooltip("Base spin duration for this reel.")]
    public float spinDuration = 2f;

    [Tooltip("Time taken by one symbol to move from top to bottom.")]
    public float symbolMoveDuration = 0.3f;

    [Tooltip("Delay between spawning moving symbols during spin.")]
    public float spawnInterval = 0.1f;

    [Tooltip("Multiplier used to speed up or slow down reel movement.")]
    public float spinSpeed = 1f;

    [Tooltip("Additional stop delay for this reel.")]
    public float stopDelay = 0f;

    [Tooltip("Delay added based on reel index to create sequential stopping.")]
    public float stopDelayStep = 0.5f;

    [Tooltip("Index of the reel. Used for sequential stop timing.")]
    public int reelIndex = 0;

    [Tooltip("Automatically starts spinning when the scene begins.")]
    public bool autoSpinOnStart = false;

    [Header("UI")]
    [Tooltip("Spin/Bet button reference. Assign this only on one reel to prevent duplicate button handling.")]
    public Button spinButton;

    // Stores all reel instances so they can be started and evaluated together.
    private static readonly List<ReelController> allReels = new List<ReelController>();

    private Coroutine spinCoroutine;
    private bool isSpinning = false;

    // Stores the final symbol name that lands in the middle visible slot.
    private string finalMiddleSymbolName;

    // Stores the actual middle symbol GameObject so win highlights can be played.
    private GameObject finalMiddleSymbolObject;

    public bool IsSpinning => isSpinning;

    // Returns true if any reel is currently spinning.
    public static bool AnyReelSpinning => allReels.Any(r => r.isSpinning);

    private void Start()
    {
        // Register this reel in the shared reel list.
        if (!allReels.Contains(this))
        {
            allReels.Add(this);
        }

        // Show initial random symbols before the first spin.
        SpawnInitialSymbols();

        if (autoSpinOnStart)
        {
            StartSpin();
        }
    }

    private void OnDestroy()
    {
        // Remove destroyed reels from the shared list to avoid null references.
        allReels.Remove(this);
    }

    /// <summary>
    /// Starts all reels together.
    /// This method is usually connected to the Spin/Bet button.
    /// </summary>
    public static void StartAllReels()
    {
        // Prevent button spam while reels are already spinning.
        if (AnyReelSpinning)
            return;

        // Deduct bet before spinning.
        if (Score.Instance != null)
        {
            Score.Instance.ApplyBet();
        }

        SetSpinButtonInteractable(false);

        // Start each reel independently.
        foreach (var reel in allReels)
        {
            if (reel != null)
            {
                reel.StartSpin();
            }
        }
    }

    /// <summary>
    /// Enables or disables the assigned spin button.
    /// Only the first reel with a valid button reference controls the button.
    /// </summary>
    private static void SetSpinButtonInteractable(bool interactable)
    {
        foreach (var reel in allReels)
        {
            if (reel != null && reel.spinButton != null)
            {
                reel.spinButton.interactable = interactable;
                break;
            }
        }
    }

    /// <summary>
    /// Spawns the initial static top, middle, and bottom symbols.
    /// </summary>
    private void SpawnInitialSymbols()
    {
        if (middleSlot == null)
            return;

        SpawnSymbolAtPosition(Vector3.up * topYOffset);
        SpawnSymbolAtPosition(Vector3.zero, true);
        SpawnSymbolAtPosition(Vector3.up * bottomYOffset);
    }

    /// <summary>
    /// Clears all current symbols from this reel.
    /// </summary>
    private void DestroyExistingSymbols()
    {
        if (middleSlot == null)
            return;

        for (int i = middleSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(middleSlot.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Starts the spin coroutine for this reel.
    /// </summary>
    public void StartSpin()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
        }

        isSpinning = true;

        // Random duration makes reel stopping feel less predictable.
        float randomDuration = Random.Range(2f, 4f);

        // Reel index creates sequential stopping between reels.
        float totalStopDelay = stopDelay + reelIndex * stopDelayStep;

        spinCoroutine = StartCoroutine(SpinRoutine(randomDuration + totalStopDelay));
    }

    /// <summary>
    /// Stops this reel immediately.
    /// </summary>
    public void StopSpin()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        isSpinning = false;
    }

    /// <summary>
    /// Handles the reel animation loop until the spin duration is complete.
    /// </summary>
    private IEnumerator SpinRoutine(float duration)
    {
        if (symbolPrefabs == null || symbolPrefabs.Length == 0)
        {
            Debug.LogWarning("ReelController: No symbol prefabs assigned.");
            isSpinning = false;
            SetSpinButtonInteractable(true);
            yield break;
        }

        if (middleSlot == null)
        {
            Debug.LogWarning("ReelController: Middle slot transform is not assigned.");
            isSpinning = false;
            SetSpinButtonInteractable(true);
            yield break;
        }

        DestroyExistingSymbols();

        // Apply spin speed to movement and spawn timing.
        float adjustedMoveDuration = symbolMoveDuration / spinSpeed;
        float adjustedSpawnInterval = spawnInterval / spinSpeed;

        // Spawn initial moving symbols so the reel immediately appears active.
        GameObject topSymbol = SpawnSymbolAtPosition(Vector3.up * topYOffset);
        GameObject middleSymbol = SpawnSymbolAtPosition(Vector3.zero, true);
        GameObject bottomSymbol = SpawnSymbolAtPosition(Vector3.up * bottomYOffset);

        StartCoroutine(MoveSymbolDown(topSymbol, adjustedMoveDuration));
        StartCoroutine(MoveSymbolDown(middleSymbol, adjustedMoveDuration));
        StartCoroutine(MoveSymbolDown(bottomSymbol, adjustedMoveDuration));

        float endTime = Time.time + duration;

        // Continuously spawn symbols while the reel is spinning.
        while (Time.time < endTime)
        {
            GameObject newSymbol = SpawnSymbolAtPosition(Vector3.up * topYOffset);
            StartCoroutine(MoveSymbolDown(newSymbol, adjustedMoveDuration));

            yield return new WaitForSeconds(adjustedSpawnInterval);
        }

        // Final reveal: clear moving symbols and place final static symbols.
        DestroyExistingSymbols();

        SpawnSymbolAtPosition(Vector3.up * topYOffset);
        SpawnSymbolAtPosition(Vector3.zero, true);
        SpawnSymbolAtPosition(Vector3.up * bottomYOffset);

        isSpinning = false;
        spinCoroutine = null;

        TryEvaluateSpinResults();
    }

    /// <summary>
    /// Spawns a random symbol prefab at the given local position.
    /// If recordMiddle is true, this symbol is stored as the final result for this reel.
    /// </summary>
    private GameObject SpawnSymbolAtPosition(Vector3 localPosition, bool recordMiddle = false)
    {
        if (symbolPrefabs == null || symbolPrefabs.Length == 0)
            return null;

        int randomIndex = Random.Range(0, symbolPrefabs.Length);
        GameObject prefab = symbolPrefabs[randomIndex];

        if (prefab == null)
            return null;

        GameObject symbolInstance = Instantiate(prefab, middleSlot);
        symbolInstance.transform.localPosition = localPosition;
        symbolInstance.transform.localRotation = Quaternion.identity;
        symbolInstance.transform.localScale = Vector3.one;

        if (recordMiddle)
        {
            finalMiddleSymbolName = GetSymbolName(prefab);
            finalMiddleSymbolObject = symbolInstance;
        }

        return symbolInstance;
    }

    /// <summary>
    /// Moves a symbol downward to simulate reel scrolling.
    /// The symbol is destroyed after it exits the visible reel area.
    /// </summary>
    private IEnumerator MoveSymbolDown(GameObject symbol, float moveDuration)
    {
        if (symbol == null)
            yield break;

        Vector3 startPosition = symbol.transform.localPosition;
        Vector3 endPosition = symbol.transform.localPosition + Vector3.down * (topYOffset + Mathf.Abs(bottomYOffset));
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration && symbol != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;

            symbol.transform.localPosition = Vector3.Lerp(startPosition, endPosition, progress);

            yield return null;
        }

        if (symbol != null)
        {
            Destroy(symbol);
        }
    }

    /// <summary>
    /// Evaluates spin results only after every reel has stopped.
    /// </summary>
    private void TryEvaluateSpinResults()
    {
        if (allReels.Any(r => r.isSpinning))
            return;

        SetSpinButtonInteractable(true);
        EvaluateResults();
    }

    /// <summary>
    /// Checks final middle symbols and applies rewards based on match type.
    /// Triple match uses the payout table from Score.
    /// Pair match gives a fixed reward.
    /// </summary>
    private void EvaluateResults()
    {
        if (Score.Instance == null)
            return;

        string[] middleSymbols = allReels
            .Select(r => r.finalMiddleSymbolName)
            .ToArray();

        if (middleSymbols.Length == 0 || middleSymbols.Any(string.IsNullOrEmpty))
            return;

        bool allSame = middleSymbols.All(s => s == middleSymbols[0]);

        // Triple match condition.
        if (allSame)
        {
            int payout = Score.Instance.GetTriplePayout(middleSymbols[0]);

            if (payout > 0)
            {
                Score.Instance.AddScore(payout);
                HighlightWinningSymbols(middleSymbols[0]);

                string message = $"Triple {middleSymbols[0]}! Payout: {payout}";
                Debug.Log(message);
                PopUp.Instance?.ShowMessage(message);
            }
            else
            {
                string warning = $"Triple {middleSymbols[0]} found, but no payout defined.";
                Debug.LogWarning(warning);
                PopUp.Instance?.ShowMessage(warning);
            }

            return;
        }

        // Pair match condition.
        var pairGroup = middleSymbols
            .GroupBy(s => s)
            .FirstOrDefault(g => g.Count() >= 2);

        if (pairGroup != null)
        {
            Score.Instance.AddScore(100);
            HighlightWinningSymbols(pairGroup.Key);

            string message = $"Pair of {pairGroup.Key}! Payout: 100";
            Debug.Log(message);
            PopUp.Instance?.ShowMessage(message);

            return;
        }

        PopUp.Instance?.ShowMessage("No win. Try again.");
    }

    /// <summary>
    /// Plays highlight animation on all symbols involved in a winning result.
    /// </summary>
    private void HighlightWinningSymbols(string winningSymbol)
    {
        foreach (var reel in allReels)
        {
            if (reel.finalMiddleSymbolName == winningSymbol && reel.finalMiddleSymbolObject != null)
            {
                WinningSymbolHighlight highlight =
                    reel.finalMiddleSymbolObject.GetComponent<WinningSymbolHighlight>();

                if (highlight != null)
                {
                    highlight.PlayHighlight();
                }
            }
        }
    }

    /// <summary>
    /// Converts prefab names into clean symbol IDs used by the payout system.
    /// Example: SlotSymbol_Cherry becomes cherry.
    /// </summary>
    private string GetSymbolName(GameObject prefab)
    {
        if (prefab == null)
            return string.Empty;

        string name = prefab.name;
        name = name.Replace("(Clone)", "").Trim();

        if (name.StartsWith("SlotSymbol_"))
            name = name.Substring("SlotSymbol_".Length);

        return name.ToLowerInvariant();
    }
}