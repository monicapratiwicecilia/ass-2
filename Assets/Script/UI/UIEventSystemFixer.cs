using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-1000)]
public class UIEventSystemFixer : MonoBehaviour
{
    private void Awake()
    {
        EnsureEventSystemWorking();
    }

    private void OnEnable()
    {
        EnsureEventSystemWorking();
    }

    public void EnsureEventSystemWorking()
    {
        var es = GetComponent<EventSystem>();
        if (es == null)
        {
            es = gameObject.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        var inputModule = GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            // Remove legacy module if present
            var legacyModule = GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                DestroyImmediate(legacyModule);
            }
            inputModule = gameObject.AddComponent<InputSystemUIInputModule>();
        }

        // Always ensure default actions are assigned so clicks & touches work!
        inputModule.AssignDefaultActions();
        inputModule.enabled = true;
#else
        var legacy = GetComponent<StandaloneInputModule>();
        if (legacy == null)
        {
            legacy = gameObject.AddComponent<StandaloneInputModule>();
        }
        legacy.enabled = true;
#endif

        es.enabled = true;
    }
}
