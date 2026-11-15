using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Kontrol rotasi 360 derajat (1 jari / klik kiri) dan zoom pinch (2 jari / mouse scroll).
/// Digunakan pada 3d_male (tubuh) serta objek 3D kartu (Stetoskop, Termometer, Kaca Pembesar).
/// Memutar dan memperbesar/memperkecil mengelilingi pusat geometri mesh (pivot),
/// sehingga objek (khususnya 3d_male yang pivot aslinya jauh) tidak bergeser atau terbang keluar dari papan.
/// </summary>
public class ModelRotator : MonoBehaviour
{
    public enum AxisMode
    {
        ParentUp,
        ParentForward,
        ParentRight,
        WorldUp,
        ModelSpine
    }

    [Header("Rotasi")]
    public AxisMode axisMode = AxisMode.ParentUp;
    public float rotateSpeed = 0.35f;               // derajat per piksel
    public bool invertX = false;
    public bool invertY = false;
    public bool allowVertical = true;               // aktifkan rotasi vertikal agar bisa melihat bagian belakang

    [Header("Zoom")]
    public float pinchSpeed = 0.0035f;
    public float scrollSpeed = 0.001f;
    public float minZoom = 0.4f;
    public float maxZoom = 3.5f;

    Renderer bodyRenderer;
    Vector3 baseScale;
    Quaternion baseRotation;
    Vector3 basePosition;
    float zoom = 1f;

    float lastPinchDist;
    bool isPinching;
    Camera cam;

    readonly List<UnityEngine.InputSystem.Controls.TouchControl> activeTouches =
        new List<UnityEngine.InputSystem.Controls.TouchControl>();

    void Awake()
    {
        baseScale = transform.localScale;
        baseRotation = transform.localRotation;
        basePosition = transform.localPosition;
        bodyRenderer = GetComponentInChildren<Renderer>(true);
        cam = Camera.main;
    }

    void OnEnable()
    {
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        // --- Sentuhan (Mobile / HP) ---
        var ts = Touchscreen.current;
        int count = 0;
        if (ts != null)
        {
            activeTouches.Clear();
            var touches = ts.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                if (t.press.isPressed)
                    activeTouches.Add(t);
            }
            count = activeTouches.Count;

            if (count == 1)
            {
                isPinching = false;
                var t = activeTouches[0];
                if (!t.press.wasPressedThisFrame)
                    Rotate(t.delta.ReadValue());
            }
            else if (count >= 2)
            {
                var t0 = activeTouches[0];
                var t1 = activeTouches[1];
                Vector2 p0 = t0.position.ReadValue();
                Vector2 p1 = t1.position.ReadValue();
                float dist = Vector2.Distance(p0, p1);

                if (isPinching)
                {
                    if (lastPinchDist > 5f && dist > 5f)
                    {
                        float delta = dist - lastPinchDist;
                        SetZoom(zoom + delta * pinchSpeed);
                    }
                }
                lastPinchDist = dist;
                isPinching = true;
            }
            else
            {
                isPinching = false;
            }
        }

        // --- Mouse / Touchpad (Laptop / Unity Editor) ---
        if (count == 0)
        {
            var m = Mouse.current;
            if (m != null)
            {
                if (m.leftButton.isPressed)
                    Rotate(m.delta.ReadValue());

                float scroll = m.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    SetZoom(zoom + scroll * scrollSpeed);
            }
        }
    }

    public Vector3 GetPivotWorld()
    {
        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>(true);

        if (bodyRenderer != null)
        {
            // Transform center mesh lokal ke world coordinate
            return bodyRenderer.transform.TransformPoint(bodyRenderer.localBounds.center);
        }
        return transform.position;
    }

    void Rotate(Vector2 d)
    {
        if (cam == null) cam = Camera.main;
        Vector3 pivot = GetPivotWorld();

        float x = (invertX ? d.x : -d.x) * rotateSpeed;
        float y = (invertY ? -d.y : d.y) * rotateSpeed;

        // 1. Putar horizontal mengelilingi sumbu vertikal dunia/kamera (ke kiri/kanan)
        Vector3 upAxis = cam != null ? cam.transform.up : Vector3.up;
        transform.RotateAround(pivot, upAxis, x);

        // 2. Putar vertikal mengelilingi sumbu horizontal kamera (membalik ke belakang untuk melihat punggung/belakang)
        if (allowVertical && cam != null)
        {
            transform.RotateAround(pivot, cam.transform.right, y);
        }
    }

    void SetZoom(float targetZoom)
    {
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        if (Mathf.Approximately(targetZoom, zoom)) return;

        Vector3 pivot = GetPivotWorld();
        float factor = targetZoom / zoom;
        zoom = targetZoom;

        transform.localScale = baseScale * zoom;
        // Jaga agar titik tengah geometri mesh (pivot) tetap di tempatnya (tidak terbang/bergeser saat membesar/mengecil)
        transform.position = pivot + factor * (transform.position - pivot);
    }

    /// <summary>
    /// Mengembalikan tampilan ke rotasi, posisi, dan zoom awal.
    /// </summary>
    public void ResetView()
    {
        zoom = 1f;
        transform.localScale = baseScale;
        transform.localRotation = baseRotation;
        transform.localPosition = basePosition;
    }
}
