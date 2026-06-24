using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineManager : MonoBehaviour
{
    [Header("UI")]
    public Button spinButton;

    [Header("Lever Visuals")]
    public GameObject leverOpen;
    public GameObject leverClosed;
    public float leverCloseDuration = 0.5f;

    private Coroutine leverCoroutine;

    private void Start()
    {
        if (spinButton != null)
        {
            spinButton.onClick.AddListener(OnSpinButtonClicked);
        }

        SetLeverOpen();
    }

    private void OnDestroy()
    {
        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(OnSpinButtonClicked);
        }
    }

    public void OnSpinButtonClicked()
    {
        if (ReelController.AnyReelSpinning)
            return;

        SetLeverClosed();

        if (leverCoroutine != null)
        {
            StopCoroutine(leverCoroutine);
        }

        ReelController.StartAllReels();
        leverCoroutine = StartCoroutine(OpenLeverAfterDelay());
    }

    private IEnumerator OpenLeverAfterDelay()
    {
        yield return new WaitForSeconds(leverCloseDuration);
        SetLeverOpen();
        leverCoroutine = null;
    }

    private void SetLeverOpen()
    {
        if (leverOpen != null)
            leverOpen.SetActive(true);

        if (leverClosed != null)
            leverClosed.SetActive(false);
    }

    private void SetLeverClosed()
    {
        if (leverOpen != null)
            leverOpen.SetActive(false);

        if (leverClosed != null)
            leverClosed.SetActive(true);
    }
}

