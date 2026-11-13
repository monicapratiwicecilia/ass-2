using UnityEngine;
using UnityEngine.InputSystem;

public class TestCase : MonoBehaviour
{
    public CaseColorManager manager;
    void Update()
    {
        var k = Keyboard.current; if (k == null) return;
        if (k.digit1Key.wasPressedThisFrame) manager.SetActiveCase(0);
        if (k.digit2Key.wasPressedThisFrame) manager.SetActiveCase(1);
        if (k.digit3Key.wasPressedThisFrame) manager.SetActiveCase(2);
        if (k.digit4Key.wasPressedThisFrame) manager.SetActiveCase(3);
        if (k.digit5Key.wasPressedThisFrame) manager.SetActiveCase(4);
        if (k.digit6Key.wasPressedThisFrame) manager.SetActiveCase(5);
        if (k.digit7Key.wasPressedThisFrame) manager.SetActiveCase(6);
        if (k.qKey.wasPressedThisFrame) manager.RevealRegion(BodyRegion.Kepala);
        if (k.wKey.wasPressedThisFrame) manager.RevealRegion(BodyRegion.Dada);
        if (k.eKey.wasPressedThisFrame) manager.RevealRegion(BodyRegion.Perut);
        if (k.rKey.wasPressedThisFrame) manager.RevealRegion(BodyRegion.Pelvis);
    }
}