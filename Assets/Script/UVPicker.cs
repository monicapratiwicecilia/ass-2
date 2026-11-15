using UnityEngine;
using UnityEngine.InputSystem;

public class UVPicker : MonoBehaviour
{
    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(pointer.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            Debug.Log($"UV: ({hit.textureCoord.x:F3}, {hit.textureCoord.y:F3})");
    }
}
