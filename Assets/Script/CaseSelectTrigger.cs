using UnityEngine;
using Vuforia;

public class CaseSelectTrigger : MonoBehaviour
{
    public CaseColorManager manager;
    public int caseIndex;   // 0 sampai 6
    ObserverBehaviour obs;

    void Awake()
    {
        obs = GetComponent<ObserverBehaviour>();
        obs.OnTargetStatusChanged += OnStatus;
    }
    void OnDestroy() { if (obs) obs.OnTargetStatusChanged -= OnStatus; }

    void OnStatus(ObserverBehaviour b, TargetStatus s)
    {
        if (s.Status == Status.TRACKED) manager.SetActiveCase(caseIndex);
    }
}