using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    public static PopUp Instance { get; private set; }

    [Header("Popup References")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField] private float showDelay = 0.5f;
    [SerializeField] private float showDuration = 1f;


    private Coroutine popupCoroutine;

    [SerializeField] private GameObject spinButton;

    private void Awake()
    {
        Instance = this;

        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(ShowPopupRoutine(message));
    }

    private IEnumerator ShowPopupRoutine(string message)
    {
        popupRoot.SetActive(false);

        yield return new WaitForSeconds(showDelay);

        messageText.text = message;
        popupRoot.SetActive(true);

        if (spinButton != null)
            spinButton.SetActive(false);

        yield return new WaitForSeconds(showDuration);

        popupRoot.SetActive(false);

        if (spinButton != null)
            spinButton.SetActive(true);

        popupCoroutine = null;
    }
}