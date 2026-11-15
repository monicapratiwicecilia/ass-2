using UnityEngine;

public class AnimatedBackground : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 0.05f;
    public float panAmount = 25f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.08f;
    public float minScale = 1.0f;
    public float maxScale = 1.08f;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            initialPosition = rectTransform.anchoredPosition;
        }
    }

    private void Update()
    {
        if (rectTransform == null) return;

        // Gerakan geser (pan) halus
        float offsetX = Mathf.Sin(Time.time * panSpeed) * panAmount;
        float offsetY = Mathf.Cos(Time.time * panSpeed * 0.8f) * (panAmount * 0.6f);
        rectTransform.anchoredPosition = initialPosition + new Vector2(offsetX, offsetY);

        // Gerakan perbesaran (zoom) halus
        float scaleT = (Mathf.Sin(Time.time * zoomSpeed) + 1f) * 0.5f;
        float currentScale = Mathf.Lerp(minScale, maxScale, scaleT);
        rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
    }
}
