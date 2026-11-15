using UnityEngine;
using UnityEngine.UI;

public class ARScanReticle : MonoBehaviour
{
    [Header("Komponen Scan")]
    public RectTransform scanLine;
    public float scanSpeed = 160f;
    public float scanHeight = 440f;

    [Header("Efek Pulse")]
    public CanvasGroup promptGroup;
    public float pulseSpeed = 2f;

    private float timer = 0f;
    private bool movingDown = true;

    private void Update()
    {
        // Gerakan garis scan naik-turun
        if (scanLine != null)
        {
            float halfH = scanHeight * 0.5f;
            Vector2 pos = scanLine.anchoredPosition;

            if (movingDown)
            {
                pos.y -= scanSpeed * Time.deltaTime;
                if (pos.y <= -halfH)
                {
                    pos.y = -halfH;
                    movingDown = false;
                }
            }
            else
            {
                pos.y += scanSpeed * Time.deltaTime;
                if (pos.y >= halfH)
                {
                    pos.y = halfH;
                    movingDown = true;
                }
            }

            scanLine.anchoredPosition = pos;
        }

        // Pulse pada teks instruksi
        if (promptGroup != null)
        {
            timer += Time.deltaTime * pulseSpeed;
            promptGroup.alpha = 0.65f + 0.35f * Mathf.PingPong(timer, 1f);
        }
    }
}
