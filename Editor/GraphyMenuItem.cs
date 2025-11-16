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
        [MenuItem( "Tools/Graphy/Complete Setup (All Modules)" )]
        static void CompleteSetup()
        {
            // Find Graphy in the scene
            GraphyManager graphyManager = Object.FindObjectOfType<GraphyManager>();
            if( graphyManager == null )
            {
                Debug.LogError( "[Graphy] No GraphyManager found in scene. Please add Graphy to the scene first." );
                return;
            }

            Debug.Log( "[Graphy] Starting complete setup..." );

            // 1. Generate materials if they don't exist
            GenerateFmodMaterials();
            GenerateFpsMaterials();

            // 2. Setup FPS module with CPU/GPU graphs
            SetupFpsModule( graphyManager );

            // 3. Setup FMOD module with all graphs and components
            SetupFmodModule( graphyManager );

            Debug.Log( "[Graphy] Complete setup finished!" );
        }

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

        static void GenerateFpsMaterials()
        {
            // This is now handled in GenerateFmodMaterials() above
            // Keeping this as a separate function for clarity
        }

        static void SetupFpsModule( GraphyManager graphyManager )
        {
            // Find FPS module. Prefer name match, but fall back to a child that has G_FpsManager.
            Transform fpsModule = graphyManager.transform.Find( "FPS - Module" );

            if( fpsModule == null )
            {
                var fpsManager = graphyManager.GetComponentInChildren<Tayx.Graphy.Fps.G_FpsManager>( true );
                if( fpsManager != null )
                {
                    fpsModule = fpsManager.transform;
                }
            }

            if( fpsModule == null )
            {
                Debug.LogWarning( "[Graphy] FPS module not found! Tried name 'FPS - Module' and searching for G_FpsManager in children." );
                return;
            }

            // Find the graph container - try different possible names
            Transform graphContainer = fpsModule.Find( "FPS_Graph_Container" );
            if( graphContainer == null )
            {
                graphContainer = fpsModule.Find( "Graph_Container" );
            }
            if( graphContainer == null )
            {
                // Look for fps_graph as parent
                Transform fpsGraph = fpsModule.Find( "fps_graph" );
                if( fpsGraph != null )
                {
                    graphContainer = fpsGraph;
                }
            }

            if( graphContainer == null )
            {
                Debug.LogWarning( "[Graphy] FPS graph container not found! Tried: FPS_Graph_Container, Graph_Container, fps_graph" );
                return;
            }

            Debug.Log( $"[Graphy] Found FPS graph container: {graphContainer.name}" );

            // Load materials
            Material cpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FPS_CPU_Graph.mat" );
            Material gpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FPS_GPU_Graph.mat" );

            if( cpuMat == null || gpuMat == null )
            {
                Debug.LogWarning( "[Graphy] FPS materials not found! Generating..." );
                GenerateFpsMaterials();
                cpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FPS_CPU_Graph.mat" );
                gpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FPS_GPU_Graph.mat" );
            }

            // Lay out FPS, CPU and GPU graphs as 3 stacked rows, filling the available height
            RectTransform graphRect = graphContainer.GetComponent<RectTransform>();
            float totalHeight = graphRect != null ? graphRect.rect.height : 0f;
            if( totalHeight <= 0f )
            {
                // Fallback to a reasonable default if the rect height is not yet initialized
                totalHeight = 90f;
            }

            float rowHeight   = totalHeight / 3f;
            float graphHeight = rowHeight; // Each graph takes one row

            // Main FPS graph at the very top row
            Transform mainFpsGraph = graphContainer.Find( "Image_Graph" );
            if( mainFpsGraph != null )
            {
                RectTransform mainRect = mainFpsGraph.GetComponent<RectTransform>();
                mainRect.anchorMin  = new Vector2( 0f, 0f );
                mainRect.anchorMax  = new Vector2( 1f, 0f );
                mainRect.pivot      = new Vector2( 0.5f, 0.5f );
                mainRect.sizeDelta  = new Vector2( 0f, graphHeight );
                // Center of the top row (rows from bottom: 0 = GPU, 1 = CPU, 2 = FPS)
                mainRect.anchoredPosition = new Vector2( 0f, rowHeight * 2.5f );
            }

            // CPU graph in the middle row (directly below FPS)
            Image cpuImage = SetupGraphImage( graphContainer, "FPS_CPU_Graph", new Vector2( 0f, rowHeight * 1.5f ), new Vector2( 0f, graphHeight ), cpuMat );

            // GPU graph in the bottom row
            Image gpuImage = SetupGraphImage( graphContainer, "FPS_GPU_Graph", new Vector2( 0f, rowHeight * 0.5f ), new Vector2( 0f, graphHeight ), gpuMat );

            // Wire up the G_FpsAdditionalGraphs component
            var additionalGraphs = fpsModule.GetComponent<Tayx.Graphy.Fps.G_FpsAdditionalGraphs>();
            if( additionalGraphs == null )
            {
                additionalGraphs = fpsModule.gameObject.AddComponent<Tayx.Graphy.Fps.G_FpsAdditionalGraphs>();
                Debug.Log( "[Graphy] Added G_FpsAdditionalGraphs component" );
            }

            SerializedObject so = new SerializedObject( additionalGraphs );
            so.FindProperty( "m_cpuGraph" ).objectReferenceValue = cpuImage;
            so.FindProperty( "m_gpuGraph" ).objectReferenceValue = gpuImage;
            so.ApplyModifiedProperties();

            // Fix text field widths - make them wider to avoid "###"
            FixFpsTextWidths( fpsModule );

            Debug.Log( "[Graphy] FPS module setup complete!" );
        }

        static void FixFpsTextWidths( Transform fpsModule )
        {
            // Find all the CPU/GPU text fields and make them wider
            string[] textNames = new string[]
            {
                "cpu_ms_text_value",
                "gpu_ms_text_value",
                "avg_cpu_ms_value",
                "avg_gpu_ms_value",
                "1%_cpu_ms_value",
                "1%_gpu_ms_value",
                "0.1%_cpu_ms_value",
                "0.1%_gpu_ms_value"
            };

            foreach( string textName in textNames )
            {
                Transform textTransform = FindDeepChild( fpsModule, textName );
                if( textTransform != null )
                {
                    RectTransform rect = textTransform.GetComponent<RectTransform>();
                    if( rect != null )
                    {
                        // Make text fields wider (increase width by 20-30 pixels)
                        rect.sizeDelta = new Vector2( rect.sizeDelta.x + 30, rect.sizeDelta.y );
                    }
                }
            }
        }

        static Transform FindDeepChild( Transform parent, string name )
        {
            foreach( Transform child in parent )
            {
                if( child.name == name )
                    return child;
                Transform result = FindDeepChild( child, name );
                if( result != null )
                    return result;
            }
            return null;
        }

        static void SetupFmodModule( GraphyManager graphyManager )
        {
            // Find FMOD module. Prefer name match, but fall back to a child that has G_FmodManager.
            Transform fmodModule = graphyManager.transform.Find( "FMOD - Module" );

            if( fmodModule == null )
            {
                var fmodManagerInChildren = graphyManager.GetComponentInChildren<Tayx.Graphy.Fmod.G_FmodManager>( true );
                if( fmodManagerInChildren != null )
                {
                    fmodModule = fmodManagerInChildren.transform;
                }
            }

            if( fmodModule == null )
            {
                Debug.LogWarning( "[Graphy] FMOD module not found! Tried name 'FMOD - Module' and searching for G_FmodManager in children." );
                return;
            }

            var fmodManager = fmodModule.GetComponent<G_FmodManager>();
            if( fmodManager == null )
            {
                Debug.LogError( "[Graphy] FMOD module has no G_FmodManager component!" );
                return;
            }

            Debug.Log( "[Graphy] Setting up FMOD module..." );

            // 1. Setup graphs with materials
            SetupFmodGraphs( fmodManager );

            // 2. Add spectrum and audio levels components
            AddSpectrumAndAudioLevels( fmodManager );

            // 3. Fix module size and position
            FixFmodModuleLayout( fmodModule );

            // 4. Wire up text references
            WireFmodTextReferences( fmodManager );

            Debug.Log( "[Graphy] FMOD module setup complete!" );
        }

        static void SetupFmodGraphs( G_FmodManager fmodManager )
        {
            GameObject fmodModule = fmodManager.gameObject;

            // Load materials
            Material cpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_CPU_Graph.mat" );
            Material memMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Memory_Graph.mat" );
            Material channelsMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Channels_Graph.mat" );
            Material fileIOMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_FileIO_Graph.mat" );

            if( cpuMat == null )
            {
                Debug.LogWarning( "[Graphy] FMOD materials not found! Generating..." );
                GenerateFmodMaterials();
                cpuMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_CPU_Graph.mat" );
                memMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Memory_Graph.mat" );
                channelsMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Channels_Graph.mat" );
                fileIOMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_FileIO_Graph.mat" );
            }

            // Get the graph component
            var fmodGraph = fmodManager.GetComponent<G_FmodGraph>();
            if( fmodGraph == null )
            {
                fmodGraph = fmodModule.AddComponent<G_FmodGraph>();
                Debug.Log( "[Graphy] Added G_FmodGraph component" );
            }

            // Find or create graph container
            Transform graphContainer = fmodModule.transform.Find( "FMOD_Graph" );
            if( graphContainer == null )
            {
                graphContainer = fmodModule.transform.Find( "Graph_Container" );
            }
            if( graphContainer == null )
            {
                GameObject containerObj = new GameObject( "Graph_Container" );
                containerObj.transform.SetParent( fmodModule.transform, false );
                RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;
                graphContainer = containerObj.transform;
                Debug.Log( "[Graphy] Created Graph_Container" );
            }

            // Create graphs stacked vertically - start at 280 from top
            Image cpuImage = SetupGraphImage( graphContainer, "CPU_Graph", new Vector2( 0, 280 ), new Vector2( -10, 70 ), cpuMat );
            Image memImage = SetupGraphImage( graphContainer, "Memory_Graph", new Vector2( 0, 205 ), new Vector2( -10, 70 ), memMat );
            Image channelsImage = SetupGraphImage( graphContainer, "Channels_Graph", new Vector2( 0, 130 ), new Vector2( -10, 70 ), channelsMat );
            Image fileIOImage = SetupGraphImage( graphContainer, "FileIO_Graph", new Vector2( 0, 55 ), new Vector2( -10, 70 ), fileIOMat );

            // Wire up the references in G_FmodGraph
            SerializedObject graphSO = new SerializedObject( fmodGraph );
            graphSO.FindProperty( "m_cpuGraph" ).objectReferenceValue = cpuImage;
            graphSO.FindProperty( "m_memoryGraph" ).objectReferenceValue = memImage;
            graphSO.FindProperty( "m_channelsGraph" ).objectReferenceValue = channelsImage;
            graphSO.FindProperty( "m_fileIOGraph" ).objectReferenceValue = fileIOImage;
            graphSO.ApplyModifiedProperties();

            Debug.Log( "[Graphy] FMOD graphs created and wired up" );
        }

        static void AddSpectrumAndAudioLevels( G_FmodManager fmodManager )
        {
            GameObject fmodModule = fmodManager.gameObject;

            // Load spectrum material
            Material spectrumMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Spectrum.mat" );
            if( spectrumMat == null )
            {
                Debug.LogWarning( "[Graphy] Spectrum material not found! Generating..." );
                GenerateFmodMaterials();
                spectrumMat = AssetDatabase.LoadAssetAtPath<Material>( "Assets/graphy-fmod/Materials/FMOD_Spectrum.mat" );
            }

            // Find or create graph container
            Transform graphContainer = fmodModule.transform.Find( "Graph_Container" );
            if( graphContainer == null )
            {
                graphContainer = fmodModule.transform.Find( "FMOD_Graph" );
            }

            if( graphContainer != null )
            {
                // Use the available height (or screen height) to scale spectrum and audio levels
                RectTransform graphRect = graphContainer.GetComponent<RectTransform>();
                float baseHeight = graphRect != null ? Mathf.Abs( graphRect.rect.height ) : (float)Screen.height;
                if( baseHeight <= 0f )
                {
                    baseHeight = Screen.height;
                }

                float spectrumHeight = Mathf.Clamp( baseHeight * 0.25f, 80f, baseHeight * 0.5f );
                float audioHeight    = Mathf.Clamp( baseHeight * 0.15f, 50f, baseHeight * 0.3f );

                // Create spectrum visualization at the bottom (below FileIO graph)
                Transform spectrumTransform = graphContainer.Find( "Spectrum_Graph" );
                if( spectrumTransform == null )
                {
                    GameObject spectrumObj = new GameObject( "Spectrum_Graph", typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Image ) );
                    spectrumObj.transform.SetParent( graphContainer, false );

                    RectTransform spectrumRect = spectrumObj.GetComponent<RectTransform>();
                    spectrumRect.anchorMin = new Vector2( 0f, 0f );
                    spectrumRect.anchorMax = new Vector2( 1f, 0f );
                    spectrumRect.pivot = new Vector2( 0.5f, 0.5f );
                    spectrumRect.anchoredPosition = new Vector2( 0f, -30f );  // Keep it just under FileIO, but taller
                    spectrumRect.sizeDelta = new Vector2( -10f, spectrumHeight );

                    Image spectrumImage = spectrumObj.GetComponent<Image>();
                    spectrumImage.material = spectrumMat;

                    spectrumTransform = spectrumObj.transform;
                    Debug.Log( "[Graphy] Created Spectrum_Graph" );
                }
                else
                {
                    // If it already exists, still ensure it uses the scaled height
                    RectTransform spectrumRect = spectrumTransform.GetComponent<RectTransform>();
                    if( spectrumRect != null )
                    {
                        spectrumRect.sizeDelta = new Vector2( spectrumRect.sizeDelta.x, spectrumHeight );
                    }
                }

                // Create audio levels container
                Transform audioLevelsContainer = graphContainer.Find( "AudioLevels_Container" );
                if( audioLevelsContainer == null )
                {
                    GameObject containerObj = new GameObject( "AudioLevels_Container", typeof( RectTransform ) );
                    containerObj.transform.SetParent( graphContainer, false );

                    RectTransform containerRect = containerObj.GetComponent<RectTransform>();
                    containerRect.anchorMin = new Vector2( 0f, 0f );
                    containerRect.anchorMax = new Vector2( 1f, 0f );
                    containerRect.pivot = new Vector2( 0.5f, 0.5f );
                    containerRect.anchoredPosition = new Vector2( 0f, -105f );  // Keep under spectrum
                    containerRect.sizeDelta = new Vector2( -10f, audioHeight );

                    audioLevelsContainer = containerObj.transform;

                    // Create 4 bars: Left RMS, Right RMS, Left Peak, Right Peak
                    CreateAudioBar( audioLevelsContainer, "LeftRMS_Bar",  new Vector2( 5f,   0f ), new Vector2( 75f, audioHeight * 0.875f ), new Color( 0.3f, 1f, 0.3f, 0.5f ) );
                    CreateAudioBar( audioLevelsContainer, "RightRMS_Bar", new Vector2( 85f,  0f ), new Vector2( 75f, audioHeight * 0.875f ), new Color( 0.3f, 1f, 0.3f, 0.5f ) );
                    CreateAudioBar( audioLevelsContainer, "LeftPeak_Bar", new Vector2( 165f, 0f ), new Vector2( 75f, audioHeight * 0.875f ), new Color( 1f, 1f, 0f, 0.8f ) );
                    CreateAudioBar( audioLevelsContainer, "RightPeak_Bar",new Vector2( 245f, 0f ), new Vector2( 75f, audioHeight * 0.875f ), new Color( 1f, 1f, 0f, 0.8f ) );

                    Debug.Log( "[Graphy] Created AudioLevels_Container with 4 bars" );
                }
                else
                {
                    RectTransform containerRect = audioLevelsContainer.GetComponent<RectTransform>();
                    if( containerRect != null )
                    {
                        containerRect.sizeDelta = new Vector2( containerRect.sizeDelta.x, audioHeight );
                    }

                    // Bars already exist; we leave their positions alone but they will stretch with the new height.
                }
            }

            // Add components using reflection to avoid compile-time dependency
            System.Type spectrumType = System.Type.GetType("Tayx.Graphy.Fmod.G_FmodSpectrum, Tayx.Graphy");
            System.Type audioLevelsType = System.Type.GetType("Tayx.Graphy.Fmod.G_FmodAudioLevels, Tayx.Graphy");

            if( spectrumType != null )
            {
                var spectrum = fmodModule.GetComponent(spectrumType);
                if( spectrum == null )
                {
                    spectrum = fmodModule.AddComponent(spectrumType);
                    Debug.Log( "[Graphy] Added G_FmodSpectrum component" );
                }

                // Wire up the spectrum image
                if( graphContainer != null )
                {
                    Transform spectrumTransform = graphContainer.Find( "Spectrum_Graph" );
                    if( spectrumTransform != null )
                    {
                        SerializedObject spectrumSO = new SerializedObject( (UnityEngine.Object)spectrum );
                        spectrumSO.FindProperty( "m_spectrumImage" ).objectReferenceValue = spectrumTransform.GetComponent<Image>();
                        spectrumSO.FindProperty( "m_spectrumMaterial" ).objectReferenceValue = spectrumMat;
                        spectrumSO.ApplyModifiedProperties();
                        Debug.Log( "[Graphy] Wired up spectrum image" );
                    }
                }
            }

            if( audioLevelsType != null )
            {
                var audioLevels = fmodModule.GetComponent(audioLevelsType);
                if( audioLevels == null )
                {
                    audioLevels = fmodModule.AddComponent(audioLevelsType);
                    Debug.Log( "[Graphy] Added G_FmodAudioLevels component" );
                }

                // Wire up the audio levels bars
                if( graphContainer != null )
                {
                    Transform audioLevelsContainer = graphContainer.Find( "AudioLevels_Container" );
                    if( audioLevelsContainer != null )
                    {
                        SerializedObject audioSO = new SerializedObject( (UnityEngine.Object)audioLevels );

                        Transform leftRms = audioLevelsContainer.Find( "LeftRMS_Bar" );
                        Transform rightRms = audioLevelsContainer.Find( "RightRMS_Bar" );
                        Transform leftPeak = audioLevelsContainer.Find( "LeftPeak_Bar" );
                        Transform rightPeak = audioLevelsContainer.Find( "RightPeak_Bar" );

                        if( leftRms != null ) audioSO.FindProperty( "m_leftRmsBar" ).objectReferenceValue = leftRms.GetComponent<Image>();
                        if( rightRms != null ) audioSO.FindProperty( "m_rightRmsBar" ).objectReferenceValue = rightRms.GetComponent<Image>();
                        if( leftPeak != null ) audioSO.FindProperty( "m_leftPeakBar" ).objectReferenceValue = leftPeak.GetComponent<Image>();
                        if( rightPeak != null ) audioSO.FindProperty( "m_rightPeakBar" ).objectReferenceValue = rightPeak.GetComponent<Image>();

                        audioSO.ApplyModifiedProperties();
                        Debug.Log( "[Graphy] Wired up audio levels bars" );
                    }
                }
            }
        }

        static void CreateAudioBar( Transform parent, string name, Vector2 anchoredPos, Vector2 sizeDelta, Color color )
        {
            GameObject barObj = new GameObject( name, typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Image ) );
            barObj.transform.SetParent( parent, false );

            RectTransform rect = barObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2( 0f, 0f );
            rect.anchorMax = new Vector2( 0f, 0f );
            rect.pivot = new Vector2( 0f, 0.5f );
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            Image image = barObj.GetComponent<Image>();
            image.color = color;
        }

        static void FixFmodModuleLayout( Transform fmodModule )
        {
            RectTransform rect = fmodModule.GetComponent<RectTransform>();
            if( rect != null )
            {
                // Make module much taller to fit all graphs including spectrum and audio levels
                rect.sizeDelta = new Vector2( 330f, 450f );  // Increased height to 450
                rect.anchoredPosition = new Vector2( -180f, -520f );  // Moved down to -520 to not overlap RAM

                Debug.Log( "[Graphy] Fixed FMOD module size and position" );
            }

            // Fix background images to contain all graphs
            Transform fullBg = fmodModule.Find( "BG_Image_FULL" );
            Transform textBg = fmodModule.Find( "BG_Image_TEXT" );
            Transform basicBg = fmodModule.Find( "BG_Image_BASIC" );

            if( fullBg != null )
            {
                RectTransform bgRect = fullBg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }
            if( textBg != null )
            {
                RectTransform bgRect = textBg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }
            if( basicBg != null )
            {
                RectTransform bgRect = basicBg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }
        }

        static void WireFmodTextReferences( G_FmodManager fmodManager )
        {
            var fmodText = fmodManager.GetComponent<G_FmodText>();
            if( fmodText == null )
            {
                Debug.LogWarning( "[Graphy] No G_FmodText component found" );
                return;
            }

            Transform fmodModule = fmodManager.transform;
            Transform textContainer = fmodModule.Find( "FMOD_Text" );
            if( textContainer == null )
            {
                Debug.LogWarning( "[Graphy] No FMOD_Text container found" );
                return;
            }

            SerializedObject textSO = new SerializedObject( fmodText );

            // Find and wire up text components
            Transform cpuCurrent = textContainer.Find( "FMOD_CPU_Current" );
            Transform cpuAvg = textContainer.Find( "FMOD_CPU_Avg" );
            Transform cpuPeak = textContainer.Find( "FMOD_CPU_Peak" );
            Transform memCurrent = textContainer.Find( "FMOD_Memory_Current" );
            Transform memAvg = textContainer.Find( "FMOD_Memory_Avg" );
            Transform memPeak = textContainer.Find( "FMOD_Memory_Peak" );
            Transform chCurrent = textContainer.Find( "FMOD_Channels_Current" );
            Transform chAvg = textContainer.Find( "FMOD_Channels_Avg" );
            Transform chPeak = textContainer.Find( "FMOD_Channels_Peak" );
            Transform fileCurrent = textContainer.Find( "FMOD_FileUsage_Current" );
            Transform fileAvg = textContainer.Find( "FMOD_FileUsage_Avg" );
            Transform filePeak = textContainer.Find( "FMOD_FileUsage_Peak" );

            if( cpuCurrent != null ) textSO.FindProperty( "m_fmodCpuText" ).objectReferenceValue = cpuCurrent.GetComponent<Text>();
            if( cpuAvg != null ) textSO.FindProperty( "m_fmodCpuAvgText" ).objectReferenceValue = cpuAvg.GetComponent<Text>();
            if( cpuPeak != null ) textSO.FindProperty( "m_fmodCpuPeakText" ).objectReferenceValue = cpuPeak.GetComponent<Text>();
            if( memCurrent != null ) textSO.FindProperty( "m_fmodMemoryText" ).objectReferenceValue = memCurrent.GetComponent<Text>();
            if( memAvg != null ) textSO.FindProperty( "m_fmodMemoryAvgText" ).objectReferenceValue = memAvg.GetComponent<Text>();
            if( memPeak != null ) textSO.FindProperty( "m_fmodMemoryPeakText" ).objectReferenceValue = memPeak.GetComponent<Text>();
            if( chCurrent != null ) textSO.FindProperty( "m_channelsText" ).objectReferenceValue = chCurrent.GetComponent<Text>();
            if( chAvg != null ) textSO.FindProperty( "m_channelsAvgText" ).objectReferenceValue = chAvg.GetComponent<Text>();
            if( chPeak != null ) textSO.FindProperty( "m_channelsPeakText" ).objectReferenceValue = chPeak.GetComponent<Text>();
            if( fileCurrent != null ) textSO.FindProperty( "m_fileUsageText" ).objectReferenceValue = fileCurrent.GetComponent<Text>();
            if( fileAvg != null ) textSO.FindProperty( "m_fileUsageAvgText" ).objectReferenceValue = fileAvg.GetComponent<Text>();
            if( filePeak != null ) textSO.FindProperty( "m_fileUsagePeakText" ).objectReferenceValue = filePeak.GetComponent<Text>();

            textSO.ApplyModifiedProperties();
            Debug.Log( "[Graphy] Wired up FMOD text references" );
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

            // Find graph container (try both names for compatibility)
            Transform graphContainer = fmodModule.transform.Find( "FMOD_Graph" );
            if( graphContainer == null )
            {
                graphContainer = fmodModule.transform.Find( "Graph_Container" );
            }
            if( graphContainer == null )
            {
                Debug.LogError( "[Graphy] No FMOD_Graph or Graph_Container found in FMOD module!" );
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

            // Also wire up text references if they exist
            var fmodText = fmodManager.GetComponent<G_FmodText>();
            if( fmodText != null )
            {
                Transform textContainer = fmodModule.transform.Find( "FMOD_Text" );
                if( textContainer != null )
                {
                    SerializedObject textSO = new SerializedObject( fmodText );

                    // Find and wire up text components
                    Transform cpuCurrent = textContainer.Find( "FMOD_CPU_Current" );
                    Transform cpuAvg = textContainer.Find( "FMOD_CPU_Avg" );
                    Transform cpuPeak = textContainer.Find( "FMOD_CPU_Peak" );
                    Transform memCurrent = textContainer.Find( "FMOD_Memory_Current" );
                    Transform memAvg = textContainer.Find( "FMOD_Memory_Avg" );
                    Transform memPeak = textContainer.Find( "FMOD_Memory_Peak" );
                    Transform chCurrent = textContainer.Find( "FMOD_Channels_Current" );
                    Transform chAvg = textContainer.Find( "FMOD_Channels_Avg" );
                    Transform chPeak = textContainer.Find( "FMOD_Channels_Peak" );
                    Transform fileCurrent = textContainer.Find( "FMOD_FileUsage_Current" );
                    Transform fileAvg = textContainer.Find( "FMOD_FileUsage_Avg" );
                    Transform filePeak = textContainer.Find( "FMOD_FileUsage_Peak" );

                    if( cpuCurrent != null ) textSO.FindProperty( "m_fmodCpuText" ).objectReferenceValue = cpuCurrent.GetComponent<Text>();
                    if( cpuAvg != null ) textSO.FindProperty( "m_fmodCpuAvgText" ).objectReferenceValue = cpuAvg.GetComponent<Text>();
                    if( cpuPeak != null ) textSO.FindProperty( "m_fmodCpuPeakText" ).objectReferenceValue = cpuPeak.GetComponent<Text>();
                    if( memCurrent != null ) textSO.FindProperty( "m_fmodMemoryText" ).objectReferenceValue = memCurrent.GetComponent<Text>();
                    if( memAvg != null ) textSO.FindProperty( "m_fmodMemoryAvgText" ).objectReferenceValue = memAvg.GetComponent<Text>();
                    if( memPeak != null ) textSO.FindProperty( "m_fmodMemoryPeakText" ).objectReferenceValue = memPeak.GetComponent<Text>();
                    if( chCurrent != null ) textSO.FindProperty( "m_channelsText" ).objectReferenceValue = chCurrent.GetComponent<Text>();
                    if( chAvg != null ) textSO.FindProperty( "m_channelsAvgText" ).objectReferenceValue = chAvg.GetComponent<Text>();
                    if( chPeak != null ) textSO.FindProperty( "m_channelsPeakText" ).objectReferenceValue = chPeak.GetComponent<Text>();
                    if( fileCurrent != null ) textSO.FindProperty( "m_fileUsageText" ).objectReferenceValue = fileCurrent.GetComponent<Text>();
                    if( fileAvg != null ) textSO.FindProperty( "m_fileUsageAvgText" ).objectReferenceValue = fileAvg.GetComponent<Text>();
                    if( filePeak != null ) textSO.FindProperty( "m_fileUsagePeakText" ).objectReferenceValue = filePeak.GetComponent<Text>();

                    textSO.ApplyModifiedProperties();
                    Debug.Log( "[Graphy] Wired up FMOD text references" );
                }
            }

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

        [MenuItem( "Tools/Graphy/Add Spectrum and Audio Levels to FMOD Module" )]
        static void AddSpectrumAndAudioLevelsToFmodModule()
        {
            // Find the FMOD module in the scene
            var fmodManager = Object.FindObjectOfType<G_FmodManager>();
            if( fmodManager == null )
            {
                Debug.LogError( "[Graphy] No FMOD module found in scene! Please add Graphy prefab first." );
                return;
            }

            GameObject fmodModule = fmodManager.gameObject;

            // Check if components already exist (use typeof to avoid compile-time dependency)
            System.Type spectrumType = System.Type.GetType("Tayx.Graphy.Fmod.G_FmodSpectrum, Tayx.Graphy");
            System.Type audioLevelsType = System.Type.GetType("Tayx.Graphy.Fmod.G_FmodAudioLevels, Tayx.Graphy");

            if( spectrumType != null )
            {
                var spectrum = fmodModule.GetComponent(spectrumType);
                if( spectrum == null )
                {
                    fmodModule.AddComponent(spectrumType);
                    Debug.Log( "[Graphy] Added G_FmodSpectrum component" );
                }
                else
                {
                    Debug.LogWarning( "[Graphy] G_FmodSpectrum component already exists" );
                }
            }

            if( audioLevelsType != null )
            {
                var audioLevels = fmodModule.GetComponent(audioLevelsType);
                if( audioLevels == null )
                {
                    fmodModule.AddComponent(audioLevelsType);
                    Debug.Log( "[Graphy] Added G_FmodAudioLevels component" );
                }
                else
                {
                    Debug.LogWarning( "[Graphy] G_FmodAudioLevels component already exists" );
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene() );

            Debug.Log( "[Graphy] Spectrum and Audio Levels components added to FMOD module!" );
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
            // Position them to the RIGHT of the main FPS graph, stacked vertically
            // These use different anchoring - anchored to the right side
            var cpuGraphGO = CreateGraphChildRightSide( "FPS_CPU_Graph", graphContainer, new Vector2( -5f, 30f ), new Vector2( 80f, 40f ) );
            var gpuGraphGO = CreateGraphChildRightSide( "FPS_GPU_Graph", graphContainer, new Vector2( -5f, -15f ), new Vector2( 80f, 40f ) );

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

            // Add spectrum and audio levels using reflection to avoid compile-time dependency
            System.Type spectrumType = System.Type.GetType("Tayx.Graphy.Fmod.G_FmodSpectrum, Tayx.Graphy");
            System.Type audioLevelsType = System.Type.GetType("Tayx.Graphy.Fmod.G_FmodAudioLevels, Tayx.Graphy");
            Component fmodSpectrum = spectrumType != null ? root.AddComponent(spectrumType) : null;
            Component fmodAudioLevels = audioLevelsType != null ? root.AddComponent(audioLevelsType) : null;

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

            // Spectrum Visualization - positioned below the graphs
            var spectrumContainer = new GameObject( "Spectrum_Container", typeof( RectTransform ) );
            spectrumContainer.transform.SetParent( root.transform, false );
            var spectrumRect = spectrumContainer.GetComponent<RectTransform>();
            spectrumRect.anchorMin = new Vector2( 0f, 0f );
            spectrumRect.anchorMax = new Vector2( 1f, 0f );
            spectrumRect.pivot = new Vector2( 0.5f, 0f );
            spectrumRect.anchoredPosition = new Vector2( 0f, -80f );
            spectrumRect.sizeDelta = new Vector2( -20f, 60f );

            var spectrumImage = spectrumContainer.AddComponent<Image>();
            spectrumImage.color = Color.white;
            spectrumImage.raycastTarget = false;

            // Audio Level Meters - positioned to the right of the graphs
            var audioLevelsContainer = new GameObject( "AudioLevels_Container", typeof( RectTransform ) );
            audioLevelsContainer.transform.SetParent( root.transform, false );
            var audioLevelsRect = audioLevelsContainer.GetComponent<RectTransform>();
            audioLevelsRect.anchorMin = new Vector2( 1f, 0f );
            audioLevelsRect.anchorMax = new Vector2( 1f, 1f );
            audioLevelsRect.pivot = new Vector2( 1f, 0.5f );
            audioLevelsRect.anchoredPosition = new Vector2( -10f, 0f );
            audioLevelsRect.sizeDelta = new Vector2( 40f, -20f );

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

            // Wire up spectrum component
            var spectrumSO = new SerializedObject( fmodSpectrum );
            spectrumSO.FindProperty( "m_spectrumImage" ).objectReferenceValue = spectrumImage;
            spectrumSO.ApplyModifiedPropertiesWithoutUndo();

            // Wire up audio levels component (bars will be created as children)
            // Note: Audio level bars are not created here - they need to be added manually or via another function
            // The component will work without them, just won't display anything
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

        static GameObject CreateGraphChildRightSide( string name, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta )
        {
            var go = new GameObject( name, typeof( RectTransform ), typeof( CanvasRenderer ), typeof( Image ) );
            go.transform.SetParent( parent, false );

            var rt = go.GetComponent<RectTransform>();
            // Anchor to the right side, centered vertically
            rt.anchorMin = new Vector2( 1f, 0.5f );
            rt.anchorMax = new Vector2( 1f, 0.5f );
            rt.pivot = new Vector2( 1f, 0.5f );
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