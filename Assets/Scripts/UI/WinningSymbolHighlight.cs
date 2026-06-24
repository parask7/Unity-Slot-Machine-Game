using System.Collections;
using UnityEngine;

public class WinningSymbolHighlight : MonoBehaviour
{
    [SerializeField] private float scaleAmount = 1.2f;
    [SerializeField] private float speed = 0.15f;
    [SerializeField] private int loops = 4;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlayHighlight()
    {
        StopAllCoroutines();
        StartCoroutine(HighlightRoutine());
    }

    private IEnumerator HighlightRoutine()
    {
        for (int i = 0; i < loops; i++)
        {
            yield return ScaleTo(originalScale * scaleAmount);
            yield return ScaleTo(originalScale);
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float time = 0f;

        while (time < speed)
        {
            time += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / speed);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}