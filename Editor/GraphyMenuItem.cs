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

            // Create spectrum material
            string spectrumShaderPath = "Packages/com.tayx.graphy.fmod/Shaders/SpectrumBars.shader";
            Shader spectrumShader = AssetDatabase.LoadAssetAtPath<Shader>( spectrumShaderPath );

            if( spectrumShader == null )
            {
                // Try alternate path
                var spectrumGuids = AssetDatabase.FindAssets( "SpectrumBars t:Shader" );
                if( spectrumGuids.Length > 0 )
                {
                    spectrumShaderPath = AssetDatabase.GUIDToAssetPath( spectrumGuids[0] );
                    spectrumShader = AssetDatabase.LoadAssetAtPath<Shader>( spectrumShaderPath );
                }
            }

            if( spectrumShader != null )
            {
                CreateMaterialIfMissing( "Assets/graphy-fmod/Materials/FMOD_Spectrum.mat", spectrumShader, new Color( 0f, 1f, 0f, 1f ) ); // Green
            }
            else
            {
                Debug.LogWarning( "[Graphy] Could not find SpectrumBars shader - spectrum material not created" );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log( "[Graphy] Generated FMOD and FPS graph materials!" );
        }

        [MenuItem( "Tools/Graphy/Setup FMOD Module with Materials and Layout" )]
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

            GameObject fmodModule = fmodManager.gameObject;

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

            // Find or create graph container
            Transform graphContainer = fmodModule.transform.Find( "Graph_Container" );
            if( graphContainer == null )
            {
                Debug.LogError( "[Graphy] No Graph_Container found in FMOD module!" );
                return;
            }

            // Find or create the 4 graph images with correct layout
            Image cpuImage = SetupGraphImage( graphContainer, "CPU_Graph", new Vector2( 13f, 175.55f ), new Vector2( -5.32f, 94.713f ), cpuMat );
            Image memImage = SetupGraphImage( graphContainer, "Memory_Graph", new Vector2( 13.52f, 85f ), new Vector2( -6.36f, 71.13f ), memMat );
            Image channelsImage = SetupGraphImage( graphContainer, "Channels_Graph", new Vector2( 13.52f, 5.685f ), new Vector2( -6.36f, 71.13f ), channelsMat );

            // Check if FileIO graph exists, if not create it
            Image fileIOImage = SetupGraphImage( graphContainer, "FileIO_Graph", new Vector2( 13.52f, -65f ), new Vector2( -6.36f, 71.13f ), fileIOMat );

            // Wire up the references in G_FmodGraph
            SerializedObject graphSO = new SerializedObject( fmodGraph );
            graphSO.FindProperty( "m_cpuGraph" ).objectReferenceValue = cpuImage;
            graphSO.FindProperty( "m_memoryGraph" ).objectReferenceValue = memImage;
            graphSO.FindProperty( "m_channelsGraph" ).objectReferenceValue = channelsImage;
            graphSO.FindProperty( "m_fileIOGraph" ).objectReferenceValue = fileIOImage;
            graphSO.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene() );

            Debug.Log( "[Graphy] FMOD module setup complete with materials and layout!" );
        }

        static Image SetupGraphImage( Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, Material material )
        {
            Transform existingTransform = parent.Find( name );
            GameObject graphGO;

            if( existingTransform != null )
            {
                graphGO = existingTransform.gameObject;
            }
            else
            {
                graphGO = new GameObject( name, typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Image ) );
                graphGO.transform.SetParent( parent, false );
            }

            RectTransform rect = graphGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2( 0f, 0f );
            rect.anchorMax = new Vector2( 1f, 0f );
            rect.pivot = new Vector2( 0.5f, 0.5f );
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            Image image = graphGO.GetComponent<Image>();
            if( material != null )
            {
                image.material = material;
            }

            return image;
        }

        [MenuItem( "Tools/Graphy/Add CPU/GPU Graphs to FPS Module" )]
        static void AddAdditionalGraphsToFpsModule()
        {
            // First generate materials
            GenerateFmodMaterials();

            // Find the FPS module in the scene
            var fpsManager = Object.FindObjectOfType<Tayx.Graphy.Fps.G_FpsManager>();
            if( fpsManager == null )
            {
                Debug.LogError( "[Graphy] No FPS module found in scene! Please add Graphy prefab first." );
                return;
            }

            GameObject fpsModule = fpsManager.gameObject;

            // Check if additional graphs component already exists
            var additionalGraphs = fpsModule.GetComponent<Tayx.Graphy.Fps.G_FpsAdditionalGraphs>();
            if( additionalGraphs != null )
            {
                Debug.LogWarning( "[Graphy] FPS module already has additional graphs component!" );
                return;
            }

            // Find or create graph container
            Transform graphContainer = fpsModule.transform.Find( "FPS_Graph_Container" );
            if( graphContainer == null )
            {
                var containerGO = new GameObject( "FPS_Graph_Container", typeof( RectTransform ) );
                containerGO.transform.SetParent( fpsModule.transform, false );
                graphContainer = containerGO.transform;

                var containerRect = containerGO.GetComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.pivot = new Vector2( 0.5f, 0.5f );
                containerRect.anchoredPosition = Vector2.zero;
                containerRect.sizeDelta = Vector2.zero;
            }

            // Create CPU and GPU graph images
            var cpuGraphGO = CreateGraphChild( "FPS_CPU_Graph", graphContainer, new Vector2( 0f, 60f ), new Vector2( 0f, 50f ) );
            var gpuGraphGO = CreateGraphChild( "FPS_GPU_Graph", graphContainer, new Vector2( 0f, 5f ), new Vector2( 0f, 50f ) );

            // Load materials
            Material cpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FPS_CPU_Graph.mat" );
            Material gpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FPS_GPU_Graph.mat" );

            // Apply materials
            if( cpuMat != null ) cpuGraphGO.GetComponent<Image>().material = cpuMat;
            if( gpuMat != null ) gpuGraphGO.GetComponent<Image>().material = gpuMat;

            // Add the additional graphs component
            additionalGraphs = fpsModule.AddComponent<Tayx.Graphy.Fps.G_FpsAdditionalGraphs>();

            // Wire up the references
            SerializedObject graphsSO = new SerializedObject( additionalGraphs );
            graphsSO.FindProperty( "m_cpuGraph" ).objectReferenceValue = cpuGraphGO.GetComponent<Image>();
            graphsSO.FindProperty( "m_gpuGraph" ).objectReferenceValue = gpuGraphGO.GetComponent<Image>();
            graphsSO.ApplyModifiedPropertiesWithoutUndo();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene() );

            Debug.Log( "[Graphy] Added CPU/GPU graphs to FPS module!" );
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
            rectTransform.sizeDelta = new Vector2( 330f, 280f );  // Increased height for 4 graphs
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

            // Graph container
            var graphContainer = new GameObject( "FMOD_Graph", typeof( RectTransform ) );
            graphContainer.transform.SetParent( root.transform, false );
            var graphRect = graphContainer.GetComponent<RectTransform>();
            graphRect.anchorMin = new Vector2( 0f, 0f );
            graphRect.anchorMax = new Vector2( 1f, 1f );
            graphRect.pivot = new Vector2( 0.5f, 0.5f );
            graphRect.anchoredPosition = Vector2.zero;
            graphRect.sizeDelta = Vector2.zero;

            // Create 4 graphs stacked vertically with proper spacing
            // Layout: CPU (top), Memory, Channels, FileIO (bottom)
            var cpuImage = CreateGraphChild( "CPU_Graph", graphContainer.transform, new Vector2( 13f, 235f ), new Vector2( -5.32f, 60f ) );
            var memoryImage = CreateGraphChild( "Memory_Graph", graphContainer.transform, new Vector2( 13f, 165f ), new Vector2( -5.32f, 60f ) );
            var channelsImage = CreateGraphChild( "Channels_Graph", graphContainer.transform, new Vector2( 13f, 95f ), new Vector2( -5.32f, 60f ) );
            var fileIOImage = CreateGraphChild( "FileIO_Graph", graphContainer.transform, new Vector2( 13f, 25f ), new Vector2( -5.32f, 60f ) );

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

            // Audio Level Meters (optional - can be added later)
            // TODO: Add audio level meter UI elements here

            // Spectrum Visualization (optional - can be added later)
            // TODO: Add spectrum visualization UI elements here

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

        static GameObject CreateGraphChild( string name, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta )
        {
            var go = new GameObject( name, typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Image ) );
            go.transform.SetParent( parent, false );

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2( 0f, 0f );
            rt.anchorMax = new Vector2( 1f, 0f );
            rt.pivot = new Vector2( 0.5f, 0.5f );
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var img = go.GetComponent<Image>();
            img.color = Color.white;
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