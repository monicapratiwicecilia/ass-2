using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimaticBackground : MonoBehaviour
{
    [Header("Background Elements")]
    public Image backgroundImage;
    public RectTransform scanLine;
    public TextMeshProUGUI telemetryStatusText;

    [Header("Scan Line Settings")]
    public float scanSpeed = 180f;
    public float scanMinY = -500f;
    public float scanMaxY = 500f;

    [Header("Pulse Settings")]
    public Color darkColor = new Color(0.043f, 0.059f, 0.102f, 1f); // #0B0F1A
    public Color pulseColor = new Color(0.078f, 0.133f, 0.278f, 1f); // #142247 Deep Electric
    public float pulseSpeed = 1.2f;

    private float scanDirection = 1f;
    private float currentScanY = 0f;

    private readonly string[] statusCodes = new string[]
    {
        "SYS STATUS: OPERATIONAL // PMI ENGINE READY",
        "PROTOCOL: HENSSGE NOMOGRAM COMPUTATION V2.4",
        "AUTOPSY PROTOCOL: LIVOR & ALGOR ACTIVE",
        "BIOMETRICS: 3D ANATOMICAL RECONSTRUCTION",
        "SERVER: FK TELKOM UNIVERSITY FORENSIC LINK OK"
    };

    private float telemetryTimer = 0f;
    private int telemetryIndex = 0;

    private void Start()
    {
        if (scanLine != null)
        {
            currentScanY = scanLine.anchoredPosition.y;
        }
    }

    private void Update()
    {
        // 1. Scan Line Animation
        if (scanLine != null)
        {
            currentScanY += scanDirection * scanSpeed * Time.deltaTime;
            if (currentScanY >= scanMaxY)
            {
                currentScanY = scanMaxY;
                scanDirection = -1f;
            }
            else if (currentScanY <= scanMinY)
            {
                currentScanY = scanMinY;
                scanDirection = 1f;
            }
            scanLine.anchoredPosition = new Vector2(scanLine.anchoredPosition.x, currentScanY);
        }

        // 2. Background Pulse Animation
        if (backgroundImage != null)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            backgroundImage.color = Color.Lerp(darkColor, pulseColor, t);
        }

        // 3. Telemetry status ticker
        if (telemetryStatusText != null)
        {
            telemetryTimer += Time.deltaTime;
            if (telemetryTimer >= 3.0f)
            {
                telemetryTimer = 0f;
                telemetryIndex = (telemetryIndex + 1) % statusCodes.Length;
                telemetryStatusText.text = statusCodes[telemetryIndex];
            }
        }
    }
}
