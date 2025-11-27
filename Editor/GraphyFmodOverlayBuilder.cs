using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Tayx.Graphy;
using Tayx.Graphy.Fmod;

namespace Tayx.Graphy.FmodEditor
{
    internal static class GraphyFmodOverlayBuilder
    {
        private const string RootName = "[Graphy]";

        [MenuItem("Tools/Graphy/Create Clean FMOD Overlay", false, 1000)]
        private static void CreateCleanOverlay()
        {
            var canvas = GetOrCreateCanvas();

            if (canvas == null)
            {
                Debug.LogError("[Graphy] Could not find or create a Canvas.");
                return;
            }

            // Root object
            var rootGo = new GameObject(RootName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(rootGo, "Create Graphy FMOD Overlay");
            rootGo.transform.SetParent(canvas.transform, false);

            var rootRect = (RectTransform)rootGo.transform;
            rootRect.anchorMin = new Vector2(1f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(1f, 1f);
            rootRect.anchoredPosition = new Vector2(-10f, -10f);
            rootRect.sizeDelta = new Vector2(260f, 600f);

            var vertical = rootGo.AddComponent<VerticalLayoutGroup>();
            vertical.childAlignment = TextAnchor.UpperLeft;
            vertical.spacing = 4f;
            vertical.padding = new RectOffset(4, 4, 4, 4);
            vertical.childForceExpandHeight = false;
            vertical.childControlHeight = true;

            // Graphy manager
            var graphyManagerGo = new GameObject("GraphyManager", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(graphyManagerGo, "Create GraphyManager");
            graphyManagerGo.transform.SetParent(rootGo.transform, false);
            var graphyManager = graphyManagerGo.AddComponent<GraphyManager>();

#if GRAPHY_FMOD || UNITY_EDITOR
            var fmodManagerGo = new GameObject("FMOD - Module", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(fmodManagerGo, "Create FMOD Module");
            fmodManagerGo.transform.SetParent(rootGo.transform, false);

            var fmodManager = fmodManagerGo.AddComponent<G_FmodManager>();
            var fmodMonitor = fmodManagerGo.AddComponent<G_FmodMonitor>();
            var fmodText = fmodManagerGo.AddComponent<G_FmodText>();
            var fmodGraph = fmodManagerGo.AddComponent<G_FmodGraph>();
            BuildFmodSection(fmodManagerGo.transform, fmodText);

            // Make sure GraphyManager can find the FMOD manager/monitor
            graphyManagerGo.transform.SetParent(rootGo.transform, false);
#endif

            Selection.activeGameObject = rootGo;
        }

        [MenuItem("Tools/Graphy/Clean Selected Graphy FMOD Layout", false, 1010)]
        private static void CleanSelectedFmodLayout()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("[Graphy] No GameObject selected. Select your [Graphy] instance or a child of it first.");
                return;
            }

            var graphyManager = selected.GetComponentInParent<GraphyManager>();
            if (graphyManager == null)
            {
                graphyManager = selected.GetComponentInChildren<GraphyManager>();
            }

            if (graphyManager == null)
            {
                Debug.LogWarning("[Graphy] Selected object is not part of a Graphy hierarchy.");
                return;
            }

#if GRAPHY_FMOD || UNITY_EDITOR
            var fmodManager = graphyManager.GetComponentInChildren<G_FmodManager>(true);
            if (fmodManager == null)
            {
                Debug.LogWarning("[Graphy] Could not find G_FmodManager under the selected Graphy instance.");
                return;
            }

            var fmodManagerGo = fmodManager.gameObject;
            Undo.RegisterFullObjectHierarchyUndo(fmodManagerGo, "Clean Graphy FMOD Layout");

            // Prefer an explicit FMOD_Text container if present so we don't touch graphs/spectrum/audio levels.
            Transform fmodTextRoot = fmodManagerGo.transform.Find("FMOD_Text");

            var fmodText = fmodManagerGo.GetComponentInChildren<G_FmodText>(true);
            if (fmodText == null)
            {
                if (fmodTextRoot == null)
                {
                    fmodTextRoot = new GameObject("FMOD_Text", typeof(RectTransform)).transform;
                    fmodTextRoot.SetParent(fmodManagerGo.transform, false);
                }

                fmodText = Undo.AddComponent<G_FmodText>(fmodTextRoot.gameObject);
            }
            else if (fmodTextRoot == null)
            {
                fmodTextRoot = fmodText.transform;
            }

            // Clear only the text container's children, leave FMOD_Graph, Spectrum_Graph, AudioLevels_Container intact.
            for (int i = fmodTextRoot.childCount - 1; i >= 0; i--)
            {
                var child = fmodTextRoot.GetChild(i);
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            BuildFmodSection(fmodTextRoot, fmodText);
            Selection.activeGameObject = fmodTextRoot.gameObject;
#else
            Debug.LogWarning("[Graphy] FMOD support is not enabled in this build.");
#endif
        }

        private static Canvas GetOrCreateCanvas()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            return canvas;
        }

        private static void BuildFmodSection(Transform parent, G_FmodText fmodText)
        {
            // Simple stacked layout for FMOD section so user gets an immediate clean view.
            var layoutHost = parent.gameObject;

            var existingLayout = layoutHost.GetComponent<VerticalLayoutGroup>();
            if (existingLayout == null)
            {
                existingLayout = layoutHost.AddComponent<VerticalLayoutGroup>();
            }

            var fmodLayout = existingLayout;
            fmodLayout.childAlignment = TextAnchor.UpperLeft;
            fmodLayout.spacing = 2f;
            fmodLayout.padding = new RectOffset(4, 4, 4, 4);
            fmodLayout.childForceExpandHeight = false;
            fmodLayout.childControlHeight = true;

            // Background
            var bgImage = layoutHost.GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = layoutHost.AddComponent<Image>();
            }
            bgImage.color = new Color(0f, 0f, 0f, 0.5f);

            // Title
            var title = CreateText("FMOD", parent, 12, FontStyle.Bold, TextAnchor.MiddleLeft);
            // Current lines
            var cpuText = CreateText("FMOD CPU:", parent, 11, FontStyle.Normal, TextAnchor.MiddleLeft);
            var memText = CreateText("FMOD Mem:", parent, 11, FontStyle.Normal, TextAnchor.MiddleLeft);
            var chText = CreateText("Channels:", parent, 11, FontStyle.Normal, TextAnchor.MiddleLeft);
            var fileText = CreateText("File I/O:", parent, 11, FontStyle.Normal, TextAnchor.MiddleLeft);

            // Avg / Peak rows
            var cpuAvg = CreateText("Avg:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var memAvg = CreateText("Avg:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var chAvg = CreateText("Avg:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var fileAvg = CreateText("Avg:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);

            var cpuPeak = CreateText("Peak:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var memPeak = CreateText("Peak:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var chPeak = CreateText("Peak:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var filePeak = CreateText("Peak:", parent, 10, FontStyle.Italic, TextAnchor.MiddleLeft);

            // Wire up FMOD text component
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            fmodText.GetType().GetField("m_fmodCpuText", flags)?.SetValue(fmodText, cpuText);
            fmodText.GetType().GetField("m_fmodMemoryText", flags)?.SetValue(fmodText, memText);
            fmodText.GetType().GetField("m_channelsText", flags)?.SetValue(fmodText, chText);
            fmodText.GetType().GetField("m_fileUsageText", flags)?.SetValue(fmodText, fileText);

            fmodText.GetType().GetField("m_fmodCpuAvgText", flags)?.SetValue(fmodText, cpuAvg);
            fmodText.GetType().GetField("m_fmodMemoryAvgText", flags)?.SetValue(fmodText, memAvg);
            fmodText.GetType().GetField("m_channelsAvgText", flags)?.SetValue(fmodText, chAvg);
            fmodText.GetType().GetField("m_fileUsageAvgText", flags)?.SetValue(fmodText, fileAvg);

            fmodText.GetType().GetField("m_fmodCpuPeakText", flags)?.SetValue(fmodText, cpuPeak);
            fmodText.GetType().GetField("m_fmodMemoryPeakText", flags)?.SetValue(fmodText, memPeak);
            fmodText.GetType().GetField("m_channelsPeakText", flags)?.SetValue(fmodText, chPeak);
            fmodText.GetType().GetField("m_fileUsagePeakText", flags)?.SetValue(fmodText, filePeak);
        }

        private static Text CreateText(string initialText, Transform parent, int fontSize, FontStyle style, TextAnchor alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create Text");
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.text = initialText;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            // Try to use any default font Unity assigns; user can swap later.
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return text;
        }
    }
}
