using UnityEngine;
using Vuforia;

public class ModelLocker : MonoBehaviour
{
    public Transform model;              // seret 3d_male ke sini
    public bool lockOnFirstDetect = false; // Matikan default agar model 3D tetap menempel di target dan tidak miring di udara
    public float lockDelay = 0.5f;       // jeda supaya tracking stabil dulu

    ObserverBehaviour obs;
    Vector3 savedLocalPos;
    Quaternion savedLocalRot;
    Vector3 savedLocalScale;
    bool locked;

    void Awake()
    {
        obs = GetComponent<ObserverBehaviour>();
        if (obs != null) obs.OnTargetStatusChanged += OnStatus;
        if (model != null)
        {
            savedLocalPos = model.localPosition;
            savedLocalRot = model.localRotation;
            savedLocalScale = model.localScale;
        }
    }

    void OnDestroy()
    {
        if (obs) obs.OnTargetStatusChanged -= OnStatus;
    }

    void OnStatus(ObserverBehaviour b, TargetStatus s)
    {
        if (!lockOnFirstDetect || locked) return;
        if (s.Status == Status.TRACKED)
            Invoke(nameof(Lock), lockDelay);
    }

    public void Lock()
    {
        if (locked || model == null) return;
        model.SetParent(null, true);     // true = posisi dunia dipertahankan
        locked = true;
    }

    // Panggil dari tombol UI "Reset" untuk memasang model ke papan lagi
    public void Unlock()
    {
        CancelInvoke(nameof(Lock));
        if (model != null)
        {
            model.SetParent(transform, false);
            model.localPosition = savedLocalPos;
            model.localRotation = savedLocalRot;
            model.localScale = savedLocalScale;
        }
        locked = false;
    }
}
