using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPressReact : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    public float pressScale = 0.9f; // Scale facto/r for the "pressed" state (shrink to 90% of original size)
    public float animationSpeed = 10f;

    private Vector3 originalScale;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateButtonPress(pressScale)); // Shrink the button
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateButtonPress(1f)); // Restore original size
    }

    private System.Collections.IEnumerator AnimateButtonPress(float targetScale)
    {
        Vector3 target = originalScale * targetScale;
        while (Vector3.Distance(rectTransform.localScale, target) > 0.01f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, target, Time.deltaTime * animationSpeed);
            yield return null;
        }
    }
}
