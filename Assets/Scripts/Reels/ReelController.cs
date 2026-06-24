using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    [Header("Reel Symbol Prefabs")]
    public GameObject[] symbolPrefabs;

    [Header("Reel Slot Parent")]
    public Transform middleSlot;

    [Header("Top/Bottom Offsets")]
    public float topYOffset = 1f;
    public float bottomYOffset = -1f;

    [Header("Spin Settings")]
    public float spinDuration = 2f;
    public float symbolMoveDuration = 0.3f;
    public float spawnInterval = 0.1f;
    public float spinSpeed = 1f;
    public float stopDelay = 0f;
    public float stopDelayStep = 0.5f;
    public int reelIndex = 0;
    public bool autoSpinOnStart = false;

    [Header("UI")]
    public Button spinButton;

    private static readonly List<ReelController> allReels = new List<ReelController>();

    private Coroutine spinCoroutine;
    private bool isSpinning = false;
    private string finalMiddleSymbolName;
    private GameObject finalMiddleSymbolObject;

    public bool IsSpinning => isSpinning;
    public static bool AnyReelSpinning => allReels.Any(r => r.isSpinning);

    private void Start()
    {
        if (!allReels.Contains(this))
        {
            allReels.Add(this);
        }

        SpawnInitialSymbols();

        if (autoSpinOnStart)
        {
            StartSpin();
        }
    }

    private void OnDestroy()
    {
        allReels.Remove(this);
    }

    public static void StartAllReels()
    {
        if (AnyReelSpinning)
            return;

        if (Score.Instance != null)
        {
            Score.Instance.ApplyBet();
        }

        SetSpinButtonInteractable(false);

        foreach (var reel in allReels)
        {
            if (reel != null)
            {
                reel.StartSpin();
            }
        }
    }

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

    private void SpawnInitialSymbols()
    {
        if (middleSlot == null)
            return;

        SpawnSymbolAtPosition(Vector3.up * topYOffset);
        SpawnSymbolAtPosition(Vector3.zero, true);
        SpawnSymbolAtPosition(Vector3.up * bottomYOffset);
    }

    private void DestroyExistingSymbols()
    {
        if (middleSlot == null)
            return;

        for (int i = middleSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(middleSlot.GetChild(i).gameObject);
        }
    }

    public void StartSpin()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
        }

        isSpinning = true;

        float randomDuration = Random.Range(2f, 4f);
        float totalStopDelay = stopDelay + reelIndex * stopDelayStep;

        spinCoroutine = StartCoroutine(SpinRoutine(randomDuration + totalStopDelay));
    }

    public void StopSpin()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        isSpinning = false;
    }

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

        float adjustedMoveDuration = symbolMoveDuration / spinSpeed;
        float adjustedSpawnInterval = spawnInterval / spinSpeed;

        GameObject topSymbol = SpawnSymbolAtPosition(Vector3.up * topYOffset);
        GameObject middleSymbol = SpawnSymbolAtPosition(Vector3.zero, true);
        GameObject bottomSymbol = SpawnSymbolAtPosition(Vector3.up * bottomYOffset);

        StartCoroutine(MoveSymbolDown(topSymbol, adjustedMoveDuration));
        StartCoroutine(MoveSymbolDown(middleSymbol, adjustedMoveDuration));
        StartCoroutine(MoveSymbolDown(bottomSymbol, adjustedMoveDuration));

        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            GameObject newSymbol = SpawnSymbolAtPosition(Vector3.up * topYOffset);
            StartCoroutine(MoveSymbolDown(newSymbol, adjustedMoveDuration));

            yield return new WaitForSeconds(adjustedSpawnInterval);
        }

        DestroyExistingSymbols();

        SpawnSymbolAtPosition(Vector3.up * topYOffset);
        SpawnSymbolAtPosition(Vector3.zero, true);
        SpawnSymbolAtPosition(Vector3.up * bottomYOffset);

        isSpinning = false;
        spinCoroutine = null;

        TryEvaluateSpinResults();
    }

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

    private void TryEvaluateSpinResults()
    {
        if (allReels.Any(r => r.isSpinning))
            return;

        SetSpinButtonInteractable(true);
        EvaluateResults();
    }

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