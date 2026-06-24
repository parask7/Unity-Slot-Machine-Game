using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the main slot machine interaction flow.
///
/// Responsibilities:
/// - Handles spin button clicks
/// - Controls lever open/close visuals
/// - Starts all reels simultaneously
/// - Prevents spins while reels are already spinning
/// </summary>
public class SlotMachineManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Button used to start a new spin.")]
    public Button spinButton;

    [Header("Lever Visuals")]
    [Tooltip("Lever sprite/object shown when the lever is in the idle state.")]
    public GameObject leverOpen;

    [Tooltip("Lever sprite/object shown while the lever is pulled.")]
    public GameObject leverClosed;

    [Tooltip("Time before the lever returns to the open position.")]
    public float leverCloseDuration = 0.5f;

    // Reference to the currently running lever animation coroutine.
    private Coroutine leverCoroutine;

    private void Start()
    {
        // Register button click listener.
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinButtonClicked);
        }

        // Initialize lever in the default open state.
        SetLeverOpen();
    }

    private void OnDestroy()
    {
        // Remove listener to prevent memory leaks and duplicate subscriptions.
        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(OnSpinButtonClicked);
        }
    }

    /// <summary>
    /// Called when the player presses the spin button.
    /// Starts all reels and updates the lever visual state.
    /// </summary>
    public void OnSpinButtonClicked()
    {
        // Prevent multiple spins while reels are already active.
        if (ReelController.AnyReelSpinning)
            return;

        // Switch lever to pulled state.
        SetLeverClosed();

        // Stop any previous lever coroutine.
        if (leverCoroutine != null)
        {
            StopCoroutine(leverCoroutine);
        }

        // Start all slot machine reels.
        ReelController.StartAllReels();

        // Return lever to open state after a short delay.
        leverCoroutine = StartCoroutine(OpenLeverAfterDelay());
    }

    /// <summary>
    /// Waits for the configured delay before returning the lever
    /// to its default open position.
    /// </summary>
    private IEnumerator OpenLeverAfterDelay()
    {
        yield return new WaitForSeconds(leverCloseDuration);

        SetLeverOpen();
        leverCoroutine = null;
    }

    /// <summary>
    /// Displays the open lever visual and hides the closed one.
    /// </summary>
    private void SetLeverOpen()
    {
        if (leverOpen != null)
            leverOpen.SetActive(true);

        if (leverClosed != null)
            leverClosed.SetActive(false);
    }

    /// <summary>
    /// Displays the closed lever visual and hides the open one.
    /// </summary>
    private void SetLeverClosed()
    {
        if (leverOpen != null)
            leverOpen.SetActive(false);

        if (leverClosed != null)
            leverClosed.SetActive(true);
    }
}