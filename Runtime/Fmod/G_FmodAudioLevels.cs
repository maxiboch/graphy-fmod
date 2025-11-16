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
    public class G_FmodAudioLevels : MonoBehaviour
    {
        #region Variables -> Serialized Private

        [SerializeField] private Image m_leftRmsBar = null;
        [SerializeField] private Image m_rightRmsBar = null;
        [SerializeField] private Image m_leftPeakBar = null;
        [SerializeField] private Image m_rightPeakBar = null;

        [SerializeField] private Color m_goodColor = new Color(0.3f, 1f, 0.3f, 1f);
        [SerializeField] private Color m_cautionColor = new Color(1f, 1f, 0f, 1f);
        [SerializeField] private Color m_criticalColor = new Color(1f, 0.3f, 0.3f, 1f);

        [SerializeField] private float m_goodThreshold = -20f;  // dB
        [SerializeField] private float m_cautionThreshold = -6f;  // dB
        [SerializeField] private float m_minDb = -60f;
        [SerializeField] private float m_maxDb = 0f;

        #endregion

        #region Variables -> Private

        private G_FmodMonitor m_fmodMonitor = null;

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

            UpdateLevels();
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
        }

        private void UpdateLevels()
        {
            // Update RMS levels
            if (m_leftRmsBar != null)
            {
                float leftRms = m_fmodMonitor.CurrentLeftRMS;
                UpdateBar(m_leftRmsBar, leftRms);
            }

            if (m_rightRmsBar != null)
            {
                float rightRms = m_fmodMonitor.CurrentRightRMS;
                UpdateBar(m_rightRmsBar, rightRms);
            }

            // Update Peak levels
            if (m_leftPeakBar != null)
            {
                float leftPeak = m_fmodMonitor.CurrentLeftPeak;
                UpdateBar(m_leftPeakBar, leftPeak);
            }

            if (m_rightPeakBar != null)
            {
                float rightPeak = m_fmodMonitor.CurrentRightPeak;
                UpdateBar(m_rightPeakBar, rightPeak);
            }
        }

        private void UpdateBar(Image bar, float dbValue)
        {
            // Normalize dB value to 0-1 range
            float normalized = Mathf.InverseLerp(m_minDb, m_maxDb, dbValue);
            bar.fillAmount = normalized;

            // Set color based on threshold
            if (dbValue >= m_cautionThreshold)
            {
                bar.color = m_criticalColor;
            }
            else if (dbValue >= m_goodThreshold)
            {
                bar.color = m_cautionColor;
            }
            else
            {
                bar.color = m_goodColor;
            }
        }

        #endregion
    }
}
