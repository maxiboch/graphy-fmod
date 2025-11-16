/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@tayx94)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            20-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Tayx.Graphy.Fmod;

namespace Tayx.Graphy
{
    public class GraphyMenuItem
    {
        [MenuItem( "Tools/Graphy/Create Prefab Variant" )]
        static void CreatePrefabVariant()
        {
            // Directory checking
            if( !AssetDatabase.IsValidFolder( "Assets/Graphy - Ultimate Stats Monitor" ) )
            {
                AssetDatabase.CreateFolder( "Assets", "Graphy - Ultimate Stats Monitor" );
            }

            if( !AssetDatabase.IsValidFolder( "Assets/Graphy - Ultimate Stats Monitor/Prefab Variants" ) )
            {
                AssetDatabase.CreateFolder( "Assets/Graphy - Ultimate Stats Monitor", "Prefab Variants" );
            }

            string graphyPrefabGuid = AssetDatabase.FindAssets( "[Graphy]" )[ 0 ];

            Object originalPrefab =
                (GameObject) AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( graphyPrefabGuid ),
                    typeof( GameObject ) );
            GameObject objectSource = PrefabUtility.InstantiatePrefab( originalPrefab ) as GameObject;

            int prefabVariantCount =
                AssetDatabase.FindAssets( "Graphy_Variant",
                    new[] { "Assets/Graphy - Ultimate Stats Monitor/Prefab Variants" } ).Length;

            GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset( objectSource,
                $"Assets/Graphy - Ultimate Stats Monitor/Prefab Variants/Graphy_Variant_{prefabVariantCount}.prefab" );

            Object.DestroyImmediate( objectSource );

            foreach( SceneView scene in SceneView.sceneViews )
            {
                scene.ShowNotification(
                    new GUIContent( "Prefab Variant Created at \"Assets/Graphy - Ultimate Stats Monitor/Prefab\"!" ) );
            }
        }

        [MenuItem( "Tools/Graphy/Import Graphy Customization Scene" )]
        static void ImportGraphyCustomizationScene()
        {
            string customizationSceneGuid = AssetDatabase.FindAssets( "Graphy_CustomizationScene" )[ 0 ];
            AssetDatabase.ImportPackage( AssetDatabase.GUIDToAssetPath( customizationSceneGuid ), true );
        }

        [MenuItem( "Tools/Graphy/Generate Missing Module Prefabs" )]
        static void GenerateMissingModulePrefabs()
        {
            GenerateFmodModulePrefabIfMissing();
        }

        [MenuItem( "Tools/Graphy/Generate FMOD Materials" )]
        static void GenerateFmodMaterials()
        {
            EnsureFolder( "Assets/graphy-fmod" );
            EnsureFolder( "Assets/graphy-fmod/Materials" );

            // Find the graph shader
            string shaderPath = "Packages/com.tayx.graphy.fmod/Shaders/GraphStandard.shader";
            Shader graphShader = AssetDatabase.LoadAssetAtPath<Shader>( shaderPath );

            if( graphShader == null )
            {
                // Try alternate path
                shaderPath = AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( "GraphStandard t:Shader" )[0] );
                graphShader = AssetDatabase.LoadAssetAtPath<Shader>( shaderPath );
            }

            if( graphShader == null )
            {
                Debug.LogError( "[Graphy] Could not find GraphStandard shader!" );
                return;
            }

            // Create FMOD materials
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FMOD_CPU_Graph.mat", graphShader, new Color( 1f, 0.8f, 0f, 1f ) ); // Yellow
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FMOD_Memory_Graph.mat", graphShader, new Color( 0f, 1f, 1f, 1f ) ); // Cyan
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FMOD_Channels_Graph.mat", graphShader, new Color( 1f, 0.5f, 1f, 1f ) ); // Pink
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FMOD_FileIO_Graph.mat", graphShader, new Color( 0.5f, 1f, 0.5f, 1f ) ); // Light Green

            // Create FPS module CPU/GPU materials
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FPS_CPU_Graph.mat", graphShader, new Color( 1f, 0.65f, 0f, 1f ) ); // Orange
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FPS_GPU_Graph.mat", graphShader, new Color( 0.3f, 0.65f, 1f, 1f ) ); // Light Blue
            CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FPS_FileIO_Graph.mat", graphShader, new Color( 0.5f, 1f, 0.5f, 1f ) ); // Light Green

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log( "[Graphy] Generated FMOD and FPS graph materials!" );
        }

        [MenuItem( "Tools/Graphy/Setup FMOD Module with Materials" )]
        static void SetupFmodModuleWithMaterials()
        {
            // First generate materials
            GenerateFmodMaterials();

            // Find the FMOD module in the scene
            var fmodManager = Object.FindObjectOfType<G_FmodManager>();
            if( fmodManager == null )
            {
                Debug.LogError( "[Graphy] No FMOD module found in scene! Please add Graphy prefab first." );
                return;
            }

            // Load materials
            Material cpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_CPU_Graph.mat" );
            Material memMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Memory_Graph.mat" );
            Material channelsMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Channels_Graph.mat" );
            Material fileIOMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_FileIO_Graph.mat" );

            // Get the graph component
            var fmodGraph = fmodManager.GetComponent<G_FmodGraph>();
            if( fmodGraph == null )
            {
                Debug.LogError( "[Graphy] FMOD module has no G_FmodGraph component!" );
                return;
            }

            // Apply materials using SerializedObject
            SerializedObject graphSO = new SerializedObject( fmodGraph );

            var cpuGraphProp = graphSO.FindProperty( "m_cpuGraph" );
            var memGraphProp = graphSO.FindProperty( "m_memoryGraph" );
            var channelsGraphProp = graphSO.FindProperty( "m_channelsGraph" );
            var fileIOGraphProp = graphSO.FindProperty( "m_fileIOGraph" );

            if( cpuGraphProp.objectReferenceValue != null && cpuMat != null )
            {
                Image cpuImage = cpuGraphProp.objectReferenceValue as Image;
                cpuImage.material = cpuMat;
                Debug.Log( "[Graphy] Applied CPU material" );
            }

            if( memGraphProp.objectReferenceValue != null && memMat != null )
            {
                Image memImage = memGraphProp.objectReferenceValue as Image;
                memImage.material = memMat;
                Debug.Log( "[Graphy] Applied Memory material" );
            }

            if( channelsGraphProp.objectReferenceValue != null && channelsMat != null )
            {
                Image channelsImage = channelsGraphProp.objectReferenceValue as Image;
                channelsImage.material = channelsMat;
                Debug.Log( "[Graphy] Applied Channels material" );
            }

            if( fileIOGraphProp.objectReferenceValue != null && fileIOMat != null )
            {
                Image fileIOImage = fileIOGraphProp.objectReferenceValue as Image;
                fileIOImage.material = fileIOMat;
                Debug.Log( "[Graphy] Applied File I/O material" );
            }

            graphSO.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene() );

            Debug.Log( "[Graphy] FMOD module setup complete with materials!" );
        }

        static void CreateMaterialIfMissing( string path, Shader shader, Color color )
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>( path );
            if( existing != null )
            {
                Debug.Log( $"[Graphy] Material already exists at '{path}'." );
                return;
            }

            Material mat = new Material( shader );
            mat.SetColor( "_Color", color );
            mat.SetColor( "_GoodColor", new Color( 0.3f, 1f, 0.3f, 1f ) );
            mat.SetColor( "_CautionColor", new Color( 1f, 1f, 0f, 1f ) );
            mat.SetColor( "_CriticalColor", new Color( 1f, 0.3f, 0.3f, 1f ) );
            mat.SetFloat( "_GoodThreshold", 60f );
            mat.SetFloat( "_CautionThreshold", 30f );

            AssetDatabase.CreateAsset( mat, path );
            Debug.Log( $"[Graphy] Created material at '{path}'." );
        }

        static void GenerateFmodModulePrefabIfMissing()
        {
            const string prefabPath = "Assets/graphy-fmod/Prefab/Internal/FMOD - Module.prefab";

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>( prefabPath );
            if( existing != null )
            {
                Debug.Log( $"[Graphy] FMOD module prefab already exists at '{prefabPath}'." );
                return;
            }

            EnsureFolder( "Assets/graphy-fmod" );
            EnsureFolder( "Assets/graphy-fmod/Prefab" );
            EnsureFolder( "Assets/graphy-fmod/Prefab/Internal" );

            var root = new GameObject( "FMOD - Module", typeof( RectTransform ) );

            try
            {
                BuildFmodModulePrefab( root );

                var createdPrefab = PrefabUtility.SaveAsPrefabAsset( root, prefabPath );
                if( createdPrefab != null )
                {
                    Debug.Log( $"[Graphy] Created FMOD module prefab at '{prefabPath}'." );
                }
                else
                {
                    Debug.LogError( $"[Graphy] Failed to create FMOD module prefab at '{prefabPath}'." );
                }
            }
            finally
            {
                Object.DestroyImmediate( root );
            }
        }

        static void BuildFmodModulePrefab( GameObject root )
        {
            var rectTransform = root.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2( 1f, 1f );
            rectTransform.anchorMax = new Vector2( 1f, 1f );
            rectTransform.pivot = new Vector2( 0.5f, 0.5f );
            rectTransform.sizeDelta = new Vector2( 330f, 180f );
            rectTransform.anchoredPosition = new Vector2( -180f, -320f );

            var fmodManager = root.AddComponent<G_FmodManager>();
            var fmodGraph = root.AddComponent<G_FmodGraph>();
            var fmodText = root.AddComponent<G_FmodText>();
            var fmodMonitor = root.AddComponent<G_FmodMonitor>();

            // Background images (FULL, TEXT, BASIC)
            var fullBg = CreateImageChild( "BG_Image_FULL", root.transform, new Color( 0f, 0f, 0f, 0.33f ) );
            var textBg = CreateImageChild( "BG_Image_TEXT", root.transform, new Color( 0f, 0f, 0f, 0.33f ) );
            var basicBg = CreateImageChild( "BG_Image_BASIC", root.transform, new Color( 0f, 0f, 0f, 0.33f ) );

            textBg.SetActive( false );
            basicBg.SetActive( false );

            // Graph container and images
            var graphContainer = new GameObject( "FMOD_Graph", typeof( RectTransform ) );
            graphContainer.transform.SetParent( root.transform, false );
            var graphRect = graphContainer.GetComponent<RectTransform>();
            graphRect.anchorMin = new Vector2( 0f, 0.5f );
            graphRect.anchorMax = new Vector2( 1f, 1f );
            graphRect.pivot = new Vector2( 0.5f, 1f );
            graphRect.anchoredPosition = Vector2.zero;
            graphRect.sizeDelta = new Vector2( -10f, -10f );

            var cpuImage = CreateImageChild( "CPU_Graph", graphContainer.transform, Color.white );
            var memoryImage = CreateImageChild( "Memory_Graph", graphContainer.transform, Color.white );
            var channelsImage = CreateImageChild( "Channels_Graph", graphContainer.transform, Color.white );
            var fileIOImage = CreateImageChild( "FileIO_Graph", graphContainer.transform, Color.white );

            // Text container
            var textContainer = new GameObject( "FMOD_Text", typeof( RectTransform ) );
            textContainer.transform.SetParent( root.transform, false );
            var textRect = textContainer.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2( 0f, 0f );
            textRect.anchorMax = new Vector2( 1f, 0.5f );
            textRect.pivot = new Vector2( 0f, 0f );
            textRect.anchoredPosition = new Vector2( 10f, 10f );
            textRect.sizeDelta = new Vector2( -20f, -20f );

            Font font = null;
            var fontPath = AssetDatabase.GUIDToAssetPath( "76ed11d84beb10846a746b4259e26d39" );
            if( !string.IsNullOrEmpty( fontPath ) )
            {
                font = AssetDatabase.LoadAssetAtPath<Font>( fontPath );
            }

            if( font == null )
            {
                font = Resources.GetBuiltinResource<Font>( "Arial.ttf" );
            }

            float rowHeight = 20f;

            Text cpuCurrent = CreateTextChild( "FMOD_CPU_Current", textContainer.transform, font, new Vector2( 0f, 0f ), "FMOD CPU: N/A" );
            Text cpuAvg     = CreateTextChild( "FMOD_CPU_Avg", textContainer.transform, font, new Vector2( 160f, 0f ), "Avg: N/A" );
            Text cpuPeak    = CreateTextChild( "FMOD_CPU_Peak", textContainer.transform, font, new Vector2( 310f, 0f ), "Peak: N/A" );

            Text memCurrent = CreateTextChild( "FMOD_Memory_Current", textContainer.transform, font, new Vector2( 0f, -rowHeight ), "FMOD Mem: N/A" );
            Text memAvg     = CreateTextChild( "FMOD_Memory_Avg", textContainer.transform, font, new Vector2( 160f, -rowHeight ), "Avg: N/A" );
            Text memPeak    = CreateTextChild( "FMOD_Memory_Peak", textContainer.transform, font, new Vector2( 310f, -rowHeight ), "Peak: N/A" );

            Text chCurrent  = CreateTextChild( "FMOD_Channels_Current", textContainer.transform, font, new Vector2( 0f, -rowHeight * 2f ), "Channels: N/A" );
            Text chAvg      = CreateTextChild( "FMOD_Channels_Avg", textContainer.transform, font, new Vector2( 160f, -rowHeight * 2f ), "Avg: N/A" );
            Text chPeak     = CreateTextChild( "FMOD_Channels_Peak", textContainer.transform, font, new Vector2( 310f, -rowHeight * 2f ), "Peak: N/A" );

            Text fileCurrent = CreateTextChild( "FMOD_FileUsage_Current", textContainer.transform, font, new Vector2( 0f, -rowHeight * 3f ), "File I/O: N/A" );
            Text fileAvg     = CreateTextChild( "FMOD_FileUsage_Avg", textContainer.transform, font, new Vector2( 160f, -rowHeight * 3f ), "Avg: N/A" );
            Text filePeak    = CreateTextChild( "FMOD_FileUsage_Peak", textContainer.transform, font, new Vector2( 310f, -rowHeight * 3f ), "Peak: N/A" );

            var managerSO = new SerializedObject( fmodManager );
            managerSO.FindProperty( "m_fmodGraphGameObject" ).objectReferenceValue = graphContainer;
            managerSO.FindProperty( "m_fmodTextGameObject" ).objectReferenceValue = textContainer;

            var bgList = managerSO.FindProperty( "m_backgroundImages" );
            bgList.arraySize = 3;
            bgList.GetArrayElementAtIndex( 0 ).objectReferenceValue = fullBg;
            bgList.GetArrayElementAtIndex( 1 ).objectReferenceValue = textBg;
            bgList.GetArrayElementAtIndex( 2 ).objectReferenceValue = basicBg;

            var graphsList = managerSO.FindProperty( "m_graphsImages" );
            graphsList.arraySize = 4;
            graphsList.GetArrayElementAtIndex( 0 ).objectReferenceValue = cpuImage.GetComponent<Image>();
            graphsList.GetArrayElementAtIndex( 1 ).objectReferenceValue = memoryImage.GetComponent<Image>();
            graphsList.GetArrayElementAtIndex( 2 ).objectReferenceValue = channelsImage.GetComponent<Image>();
            graphsList.GetArrayElementAtIndex( 3 ).objectReferenceValue = fileIOImage.GetComponent<Image>();

            managerSO.ApplyModifiedPropertiesWithoutUndo();

            var graphSO = new SerializedObject( fmodGraph );
            graphSO.FindProperty( "m_cpuGraph" ).objectReferenceValue = cpuImage.GetComponent<Image>();
            graphSO.FindProperty( "m_memoryGraph" ).objectReferenceValue = memoryImage.GetComponent<Image>();
            graphSO.FindProperty( "m_channelsGraph" ).objectReferenceValue = channelsImage.GetComponent<Image>();
            graphSO.FindProperty( "m_fileIOGraph" ).objectReferenceValue = fileIOImage.GetComponent<Image>();
            graphSO.ApplyModifiedPropertiesWithoutUndo();

            var textSO = new SerializedObject( fmodText );
            textSO.FindProperty( "m_fmodCpuText" ).objectReferenceValue = cpuCurrent;
            textSO.FindProperty( "m_fmodMemoryText" ).objectReferenceValue = memCurrent;
            textSO.FindProperty( "m_channelsText" ).objectReferenceValue = chCurrent;
            textSO.FindProperty( "m_fileUsageText" ).objectReferenceValue = fileCurrent;

            textSO.FindProperty( "m_fmodCpuAvgText" ).objectReferenceValue = cpuAvg;
            textSO.FindProperty( "m_fmodMemoryAvgText" ).objectReferenceValue = memAvg;
            textSO.FindProperty( "m_channelsAvgText" ).objectReferenceValue = chAvg;
            textSO.FindProperty( "m_fileUsageAvgText" ).objectReferenceValue = fileAvg;

            textSO.FindProperty( "m_fmodCpuPeakText" ).objectReferenceValue = cpuPeak;
            textSO.FindProperty( "m_fmodMemoryPeakText" ).objectReferenceValue = memPeak;
            textSO.FindProperty( "m_channelsPeakText" ).objectReferenceValue = chPeak;
            textSO.FindProperty( "m_fileUsagePeakText" ).objectReferenceValue = filePeak;
            textSO.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureFolder( string path )
        {
            if( AssetDatabase.IsValidFolder( path ) )
            {
                return;
            }

            var segments = path.Split( '/' );
            string current = segments[0];

            for( int i = 1; i < segments.Length; i++ )
            {
                string next = current + "/" + segments[ i ];
                if( !AssetDatabase.IsValidFolder( next ) )
                {
                    AssetDatabase.CreateFolder( current, segments[ i ] );
                }

                current = next;
            }
        }

        static GameObject CreateImageChild( string name, Transform parent, Color color )
        {
            var go = new GameObject( name, typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Image ) );
            go.transform.SetParent( parent, false );

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2( 0.5f, 0.5f );
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            return go;
        }

        static Text CreateTextChild( string name, Transform parent, Font font, Vector2 anchoredPosition, string defaultText )
        {
            var go = new GameObject( name, typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Text ) );
            go.transform.SetParent( parent, false );

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2( 0f, 1f );
            rt.anchorMax = new Vector2( 0f, 1f );
            rt.pivot = new Vector2( 0f, 1f );
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2( 150f, 18f );

            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            text.text = defaultText;
            text.raycastTarget = false;

            return text;
        }
    }
}