using UnityEngine;

/// Membuat objek (panel teks) selalu menghadap kamera.
/// Pasang di panel teks popup (Quad latar + teks TextMeshPro 3D sebagai anaknya).
public class Billboard : MonoBehaviour
{
    public bool lockUpright = true;   // true = hanya berputar horizontal, tidak miring

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = transform.position - cam.transform.position;
        if (lockUpright) dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(dir);
    }
}