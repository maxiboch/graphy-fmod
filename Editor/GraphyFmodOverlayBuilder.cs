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

            var fitter = rootGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Graphy manager
            var graphyManagerGo = new GameObject("GraphyManager", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(graphyManagerGo, "Create GraphyManager");
            graphyManagerGo.transform.SetParent(rootGo.transform, false);
            var graphyManager = graphyManagerGo.AddComponent<GraphyManager>();

#if GRAPHY_FMOD || UNITY_EDITOR
            var fmodManagerGo = new GameObject("FMOD Module", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(fmodManagerGo, "Create FMOD Module");
            fmodManagerGo.transform.SetParent(rootGo.transform, false);

            var fmodManager = fmodManagerGo.AddComponent<G_FmodManager>();
            var fmodMonitor = fmodManagerGo.AddComponent<G_FmodMonitor>();
            var fmodText = fmodManagerGo.AddComponent<G_FmodText>();
            var fmodGraph = fmodManagerGo.AddComponent<G_FmodGraph>();

            // Simple stacked layout for FMOD section so user gets an immediate clean view.
            var fmodLayout = fmodManagerGo.AddComponent<VerticalLayoutGroup>();
            fmodLayout.childAlignment = TextAnchor.UpperLeft;
            fmodLayout.spacing = 2f;
            fmodLayout.padding = new RectOffset(4, 4, 4, 4);

            // Background
            var bgImage = fmodManagerGo.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.5f);

            // Title
            var title = CreateText("FMOD", fmodManagerGo.transform, 12, FontStyle.Bold, TextAnchor.MiddleLeft);
            // Current lines
            var cpuText = CreateText("FMOD CPU:", fmodManagerGo.transform, 11, FontStyle.Normal, TextAnchor.MiddleLeft);
            var memText = CreateText("FMOD Mem:", fmodManagerGo.transform, 11, FontStyle.Normal, TextAnchor.MiddleLeft);
            var chText = CreateText("Channels:", fmodManagerGo.transform, 11, FontStyle.Normal, TextAnchor.MiddleLeft);
            var fileText = CreateText("File I/O:", fmodManagerGo.transform, 11, FontStyle.Normal, TextAnchor.MiddleLeft);

            // Avg / Peak rows
            var cpuAvg = CreateText("Avg:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var memAvg = CreateText("Avg:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var chAvg = CreateText("Avg:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var fileAvg = CreateText("Avg:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);

            var cpuPeak = CreateText("Peak:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var memPeak = CreateText("Peak:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var chPeak = CreateText("Peak:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);
            var filePeak = CreateText("Peak:", fmodManagerGo.transform, 10, FontStyle.Italic, TextAnchor.MiddleLeft);

            // Wire up FMOD text component
            fmodText.GetType().GetField("m_fmodCpuText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, cpuText);
            fmodText.GetType().GetField("m_fmodMemoryText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, memText);
            fmodText.GetType().GetField("m_channelsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, chText);
            fmodText.GetType().GetField("m_fileUsageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, fileText);

            fmodText.GetType().GetField("m_fmodCpuAvgText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, cpuAvg);
            fmodText.GetType().GetField("m_fmodMemoryAvgText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, memAvg);
            fmodText.GetType().GetField("m_channelsAvgText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, chAvg);
            fmodText.GetType().GetField("m_fileUsageAvgText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, fileAvg);

            fmodText.GetType().GetField("m_fmodCpuPeakText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, cpuPeak);
            fmodText.GetType().GetField("m_fmodMemoryPeakText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, memPeak);
            fmodText.GetType().GetField("m_channelsPeakText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, chPeak);
            fmodText.GetType().GetField("m_fileUsagePeakText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(fmodText, filePeak);

            // Make sure GraphyManager can find the FMOD manager/monitor
            graphyManagerGo.transform.SetParent(rootGo.transform, false);
#endif

            Selection.activeGameObject = rootGo;
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
