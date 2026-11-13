using UnityEngine;
using Vuforia;

public class CardScanTrigger : MonoBehaviour
{
    public CaseColorManager manager;
    public BodyRegion region;
    ObserverBehaviour obs;

    void Awake()
    {
        obs = GetComponent<ObserverBehaviour>();
        obs.OnTargetStatusChanged += OnStatus;
    }
    void OnDestroy() { if (obs) obs.OnTargetStatusChanged -= OnStatus; }

    void OnStatus(ObserverBehaviour b, TargetStatus s)
    {
        if (s.Status == Status.TRACKED) manager.RevealRegion(region);
    }
}