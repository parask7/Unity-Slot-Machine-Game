using UnityEngine;

/// <summary>
/// Displays the slot machine payout table using the popup system.
/// 
/// This script is typically attached to a UI button that allows
/// players to view symbol rewards and payout information.
/// </summary>
public class PayoutButton : MonoBehaviour
{
    /// <summary>
    /// Shows the current payout table in the popup window.
    /// </summary>
    public void ShowPayouts()
    {
        PopUp.Instance?.ShowMessage(
            "PAYOUTS\n\n" +
            "Cherry   : 200\n" +
            "Bell     : 300\n" +
            "BAR      : 500\n" +
            "Seven    : 1000\n\n" +
            "Any Pair : 100"
        );
    }
}