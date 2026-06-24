using UnityEngine;

public class PayoutButton : MonoBehaviour
{
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