using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// Inspeksi 360 derajat: 1 jari = putar, 2 jari = zoom (pinch).
/// Pasang di 3d_male (anak dari Tubuh), BUKAN di Tubuh.
/// Di Editor: tahan klik kiri + geser = putar, scroll = zoom.
public class ModelRotator : MonoBehaviour
{
    [Header("Rotasi")]
    public float rotateSpeed = 0.3f;     // derajat per piksel
    public bool invertX = false;
    public bool invertY = false;
    public bool allowVertical = true;    // false = hanya putar kiri-kanan

    [Header("Zoom")]
    public float pinchSpeed = 0.002f;
    public float scrollSpeed = 0.001f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.5f;

    Vector3 baseScale;
    Quaternion baseRotation;
    float zoom = 1f;
    Camera cam;

    void Start()
    {
        baseScale = transform.localScale;
        baseRotation = transform.localRotation;
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // --- Sentuhan (HP) ---
        var ts = Touchscreen.current;
        if (ts != null)
        {
            int count = 0;
            foreach (var t in ts.touches)
                if (t.press.isPressed) count++;

            if (count == 1)
            {
                var t = ts.primaryTouch;
                if (!t.press.wasPressedThisFrame && !IsOverUI(t.touchId.ReadValue()))
                    Rotate(t.delta.ReadValue());
            }
            else if (count >= 2)
            {
                Pinch(ts);
            }
        }

    // --- Mouse / touchpad (tes di Editor), selalu aktif ---
        var m = Mouse.current;
        if (m != null)
        {
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (m.leftButton.isPressed && !overUI)
                Rotate(m.delta.ReadValue());

            float scroll = m.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f) SetZoom(zoom + scroll * scrollSpeed);
        }
    }

    void Rotate(Vector2 d)
    {
        float x = (invertX ? d.x : -d.x) * rotateSpeed;
        float y = (invertY ? -d.y : d.y) * rotateSpeed;

        // Putar mengelilingi sumbu dunia/kamera supaya terasa natural
        transform.Rotate(Vector3.up, x, Space.World);
        if (allowVertical)
            transform.Rotate(cam.transform.right, y, Space.World);
    }

    void Pinch(Touchscreen ts)
    {
        var a = ts.touches[0];
        var b = ts.touches[1];
        Vector2 pa = a.position.ReadValue(), pb = b.position.ReadValue();
        Vector2 da = a.delta.ReadValue(), db = b.delta.ReadValue();

        float now = (pa - pb).magnitude;
        float before = ((pa - da) - (pb - db)).magnitude;
        SetZoom(zoom + (now - before) * pinchSpeed);
    }

    void SetZoom(float z)
    {
        zoom = Mathf.Clamp(z, minZoom, maxZoom);
        transform.localScale = baseScale * zoom;
    }

    bool IsOverUI(int touchId)
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject(touchId);
    }

    /// Panggil dari tombol UI "Reset Tampilan"
    public void ResetView()
    {
        zoom = 1f;
        transform.localScale = baseScale;
        transform.localRotation = baseRotation;
    }
}