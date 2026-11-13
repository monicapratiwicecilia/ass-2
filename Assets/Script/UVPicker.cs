using UnityEngine;

public class UVPicker : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            Debug.Log($"UV: ({hit.textureCoord.x:F3}, {hit.textureCoord.y:F3})");
    }
}