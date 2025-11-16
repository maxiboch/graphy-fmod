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

using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.Fmod
{
    public class G_FmodSpectrum : MonoBehaviour
    {
        #region Variables -> Serialized Private

        [SerializeField] private Image m_spectrumImage = null;
        [SerializeField] private Material m_spectrumMaterial = null;
        [SerializeField] private int m_barCount = 64;
        [SerializeField] private float m_minDb = -80f;
        [SerializeField] private float m_maxDb = 0f;

        #endregion

        #region Variables -> Private

        private G_FmodMonitor m_fmodMonitor = null;
        private float[] m_barHeights = null;

        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            if (m_fmodMonitor == null || !m_fmodMonitor.IsAvailable)
            {
                return;
            }

            UpdateSpectrum();
        }

        #endregion

        #region Methods -> Public

        public void UpdateParameters()
        {
            // Can be called to refresh settings
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            m_fmodMonitor = GetComponent<G_FmodMonitor>();
            if (m_fmodMonitor == null)
            {
                m_fmodMonitor = gameObject.AddComponent<G_FmodMonitor>();
            }

            m_barHeights = new float[m_barCount];

            // Initialize all bar heights to 0
            for (int i = 0; i < m_barCount; i++)
            {
                m_barHeights[i] = 0f;
            }

            if (m_spectrumImage != null)
            {
                // If no material assigned, try to find the spectrum shader
                if (m_spectrumMaterial == null)
                {
                    Shader spectrumShader = Shader.Find("Graphy/Spectrum Bars");
                    if (spectrumShader != null)
                    {
                        m_spectrumMaterial = new Material(spectrumShader);
                    }
                }

                if (m_spectrumMaterial != null)
                {
                    m_spectrumImage.material = m_spectrumMaterial;
                    m_spectrumMaterial.SetInt("_BarCount", m_barCount);
                }
            }
        }

        private void UpdateSpectrum()
        {
            float[] spectrumData = m_fmodMonitor.SpectrumData;
            if (spectrumData == null || spectrumData.Length == 0)
            {
                return;
            }

            // Group spectrum data into bars
            int samplesPerBar = Mathf.Max(1, spectrumData.Length / m_barCount);

            for (int i = 0; i < m_barCount; i++)
            {
                float sum = 0f;
                int startIdx = i * samplesPerBar;
                int endIdx = Mathf.Min(startIdx + samplesPerBar, spectrumData.Length);

                for (int j = startIdx; j < endIdx; j++)
                {
                    sum += spectrumData[j];
                }

                float avgDb = sum / (endIdx - startIdx);
                
                // Normalize to 0-1 range
                m_barHeights[i] = Mathf.InverseLerp(m_minDb, m_maxDb, avgDb);
            }

            // Update material with bar heights
            if (m_spectrumMaterial != null)
            {
                m_spectrumMaterial.SetFloatArray("_BarHeights", m_barHeights);
                m_spectrumMaterial.SetInt("_BarCount", m_barCount);
            }
        }

        #endregion
    }
}
