using System;
using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.Rendering
{
    class DebugUpdater : MonoBehaviour
    {
        static DebugUpdater s_Instance = null;

        ScreenOrientation m_Orientation;
        bool m_RuntimeUiWasVisibleLastFrame = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInit()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (DebugManager.instance.enableRuntimeUI)
                EnableRuntime();
#endif
        }

        internal static void SetEnabled(bool enabled)
        {
            if (enabled)
                EnableRuntime();
            else
                DisableRuntime();
        }

        static void EnableRuntime()
        {
            if (s_Instance != null)
                return;

            var go = new GameObject { name = "[Debug Updater]" };
            s_Instance = go.AddComponent<DebugUpdater>();
            s_Instance.m_Orientation = Screen.orientation;

            DontDestroyOnLoad(go);
        }

        static void DisableRuntime()
        {
            DebugManager debugManager = DebugManager.instance;
            debugManager.displayRuntimeUI = false;
            debugManager.displayPersistentRuntimeUI = false;

            if (s_Instance != null)
            {
                CoreUtils.Destroy(s_Instance.gameObject);
                s_Instance = null;
            }
        }

        internal static void HandleInternalEventSystemComponents(bool uiEnabled)
        {
            if (s_Instance == null)
                return;

            if (uiEnabled)
                s_Instance.EnsureExactlyOneEventSystem();
            else
                s_Instance.DestroyDebugEventSystem();
        }

        void EnsureExactlyOneEventSystem()
        {
            var eventSystems = FindObjectsOfType<EventSystem>();
            var debugEventSystem = GetComponent<EventSystem>();

            if (eventSystems.Length > 1 && debugEventSystem != null)
            {
                DestroyDebugEventSystem();
            }
            else if (eventSystems.Length == 0)
            {
                CreateDebugEventSystem();
            }
        }
        
        void CreateDebugEventSystem()
        {
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
        }

        void DestroyDebugEventSystem()
        {
            var eventSystem = GetComponent<EventSystem>();
            CoreUtils.Destroy(GetComponent<StandaloneInputModule>());
            CoreUtils.Destroy(GetComponent<BaseInput>());
            CoreUtils.Destroy(eventSystem);
        }

        void Update()
        {
            DebugManager debugManager = DebugManager.instance;

            // Runtime UI visibility can change i.e. due to scene unload - allow component cleanup in this case.
            if (m_RuntimeUiWasVisibleLastFrame != debugManager.displayRuntimeUI)
            {
                HandleInternalEventSystemComponents(debugManager.displayRuntimeUI);
            }

            debugManager.UpdateActions();

            if (debugManager.GetAction(DebugAction.EnableDebugMenu) != 0.0f ||
                debugManager.GetActionToggleDebugMenuWithTouch())
            {
                debugManager.displayRuntimeUI = !debugManager.displayRuntimeUI;
            }

            if (debugManager.displayRuntimeUI)
            {
                if (debugManager.GetAction(DebugAction.ResetAll) != 0.0f)
                    debugManager.Reset();

                if (debugManager.GetActionReleaseScrollTarget())
                    debugManager.SetScrollTarget(null); // Allow mouse wheel scroll without causing auto-scroll
            }

            if (m_Orientation != Screen.orientation)
            {
                StartCoroutine(RefreshRuntimeUINextFrame());
                m_Orientation = Screen.orientation;
            }

            m_RuntimeUiWasVisibleLastFrame = debugManager.displayRuntimeUI;
        }

        static IEnumerator RefreshRuntimeUINextFrame()
        {
            yield return null; // Defer runtime UI refresh to next frame to allow canvas to update first.
            DebugManager.instance.ReDrawOnScreenDebug();
        }
    }
}
