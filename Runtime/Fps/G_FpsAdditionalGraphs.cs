/* ---------------------------------------
 * Author:          Maxi Boch (@maxiboch)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            16-Nov-2024
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using Tayx.Graphy.Graph;
using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.Fps
{
    public class G_FpsAdditionalGraphs : G_Graph
    {
        #region Variables -> Serialized Private

        [SerializeField] private Image m_cpuGraph = null;
        [SerializeField] private Image m_gpuGraph = null;
        [SerializeField] private Image m_fileIOGraph = null;

        [SerializeField] private Shader m_graphShader = null;
        [SerializeField] private bool m_isInitialized = false;

        #endregion

        #region Variables -> Private

        private GraphyManager m_graphyManager = null;
        private G_FpsMonitor m_fpsMonitor = null;

        private int m_resolution = 150;

        private G_GraphShader m_cpuGraphShader = null;
        private G_GraphShader m_gpuGraphShader = null;
        private G_GraphShader m_fileIOGraphShader = null;

        private float[] m_cpuGraphArray;
        private float[] m_gpuGraphArray;
        private float[] m_fileIOGraphArray;

        private float m_highestCpuValue = 0f;
        private float m_highestGpuValue = 0f;
        private float m_highestFileIOValue = 0f;

        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            UpdateGraph();
        }

        #endregion

        #region Methods -> Public

        public void UpdateParameters()
        {
            if (m_graphyManager != null)
            {
                m_resolution = m_graphyManager.FpsGraphResolution;
                CreatePoints();
                UpdateGraph();
            }
        }

        #endregion

        #region Methods -> Protected Override

        protected override void UpdateGraph()
        {
            if (m_fpsMonitor == null)
            {
                return;
            }

            // Update CPU graph
            if (m_cpuGraphShader != null)
            {
                float cpuTime = m_fpsMonitor.CurrentCpuMs;
                m_highestCpuValue = Mathf.Max(m_highestCpuValue, cpuTime);

                for (int i = 0; i <= m_resolution - 1; i++)
                {
                    if (i >= m_resolution - 1)
                    {
                        m_cpuGraphArray[i] = cpuTime;
                    }
                    else
                    {
                        m_cpuGraphArray[i] = m_cpuGraphArray[i + 1];
                    }
                }

                m_cpuGraphShader.ShaderArrayValues = m_cpuGraphArray;
                m_cpuGraphShader.UpdatePoints();
                m_cpuGraphShader.UpdateArrayValuesLength();
                m_cpuGraphShader.Average = m_fpsMonitor.AverageCpuMs;
                m_cpuGraphShader.UpdateAverage();
                m_cpuGraphShader.GoodThreshold = 16.6f;  // 60 FPS
                m_cpuGraphShader.CautionThreshold = 33.3f;  // 30 FPS
                m_cpuGraphShader.UpdateThresholds();
            }

            // Update GPU graph
            if (m_gpuGraphShader != null)
            {
                float gpuTime = m_fpsMonitor.CurrentGpuMs;
                m_highestGpuValue = Mathf.Max(m_highestGpuValue, gpuTime);

                for (int i = 0; i <= m_resolution - 1; i++)
                {
                    if (i >= m_resolution - 1)
                    {
                        m_gpuGraphArray[i] = gpuTime;
                    }
                    else
                    {
                        m_gpuGraphArray[i] = m_gpuGraphArray[i + 1];
                    }
                }

                m_gpuGraphShader.ShaderArrayValues = m_gpuGraphArray;
                m_gpuGraphShader.UpdatePoints();
                m_gpuGraphShader.UpdateArrayValuesLength();
                m_gpuGraphShader.Average = m_fpsMonitor.AverageGpuMs;
                m_gpuGraphShader.UpdateAverage();
                m_gpuGraphShader.GoodThreshold = 16.6f;  // 60 FPS
                m_gpuGraphShader.CautionThreshold = 33.3f;  // 30 FPS
                m_gpuGraphShader.UpdateThresholds();
            }

            // Update File I/O graph (if FMOD is available)
            #if GRAPHY_FMOD
            if (m_fileIOGraphShader != null)
            {
                var fmodMonitor = GetComponent<Tayx.Graphy.Fmod.G_FmodMonitor>();
                if (fmodMonitor != null && fmodMonitor.IsAvailable)
                {
                    float fileIO = fmodMonitor.CurrentFileUsageKBps;
                    m_highestFileIOValue = Mathf.Max(m_highestFileIOValue, fileIO);

                    for (int i = 0; i <= m_resolution - 1; i++)
                    {
                        if (i >= m_resolution - 1)
                        {
                            m_fileIOGraphArray[i] = fileIO;
                        }
                        else
                        {
                            m_fileIOGraphArray[i] = m_fileIOGraphArray[i + 1];
                        }
                    }

                    m_fileIOGraphShader.ShaderArrayValues = m_fileIOGraphArray;
                    m_fileIOGraphShader.UpdatePoints();
                    m_fileIOGraphShader.UpdateArrayValuesLength();
                    m_fileIOGraphShader.Average = fmodMonitor.AverageFileUsageKBps;
                    m_fileIOGraphShader.UpdateAverage();
                    m_fileIOGraphShader.GoodThreshold = 1000f;  // Good < 1000 KB/s
                    m_fileIOGraphShader.CautionThreshold = 3000f;  // Caution 1000-3000 KB/s
                    m_fileIOGraphShader.UpdateThresholds();
                }
            }
            #endif
        }

        protected override void CreatePoints()
        {
            m_cpuGraphArray = new float[m_resolution];
            m_gpuGraphArray = new float[m_resolution];
            m_fileIOGraphArray = new float[m_resolution];

            for (int i = 0; i < m_resolution; i++)
            {
                m_cpuGraphArray[i] = 0;
                m_gpuGraphArray[i] = 0;
                m_fileIOGraphArray[i] = 0;
            }

            if (m_cpuGraphShader != null)
            {
                m_cpuGraphShader.ShaderArrayValues = m_cpuGraphArray;
                m_cpuGraphShader.UpdatePoints();
                m_cpuGraphShader.UpdateArrayValuesLength();
            }

            if (m_gpuGraphShader != null)
            {
                m_gpuGraphShader.ShaderArrayValues = m_gpuGraphArray;
                m_gpuGraphShader.UpdatePoints();
                m_gpuGraphShader.UpdateArrayValuesLength();
            }

            if (m_fileIOGraphShader != null)
            {
                m_fileIOGraphShader.ShaderArrayValues = m_fileIOGraphArray;
                m_fileIOGraphShader.UpdatePoints();
                m_fileIOGraphShader.UpdateArrayValuesLength();
            }
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            m_graphyManager = transform.root.GetComponentInChildren<GraphyManager>();
            m_fpsMonitor = GetComponent<G_FpsMonitor>();

            if (m_fpsMonitor == null)
            {
                m_fpsMonitor = gameObject.AddComponent<G_FpsMonitor>();
            }

            if (m_isInitialized) return;

            if (m_graphShader == null)
            {
                m_graphShader = Shader.Find("Graphy/Graph Standard");
            }

            if (m_cpuGraph != null && m_graphShader != null)
            {
                m_cpuGraphShader = new G_GraphShader
                {
                    Image = m_cpuGraph
                };
                m_cpuGraphShader.InitializeShader();
            }

            if (m_gpuGraph != null && m_graphShader != null)
            {
                m_gpuGraphShader = new G_GraphShader
                {
                    Image = m_gpuGraph
                };
                m_gpuGraphShader.InitializeShader();
            }

            if (m_fileIOGraph != null && m_graphShader != null)
            {
                m_fileIOGraphShader = new G_GraphShader
                {
                    Image = m_fileIOGraph
                };
                m_fileIOGraphShader.InitializeShader();
            }

            UpdateParameters();

            m_isInitialized = true;
        }

        #endregion
    }
}

