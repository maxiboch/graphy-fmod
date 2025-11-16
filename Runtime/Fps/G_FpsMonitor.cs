/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@martinTayx), modified by Paul Sinnett (paul.sinnett@gmail.com) (@paulsinnett)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            15-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using Tayx.Graphy.Utils;
using UnityEngine;

namespace Tayx.Graphy.Fps
{
    public class G_FpsMonitor : MonoBehaviour
    {
        #region Variables -> Private

        private G_DoubleEndedQueue m_fpsSamples;
        private short[] m_fpsSamplesSorted;
        private short m_fpsSamplesCapacity = 1024;
        private short m_onePercentSamples = 10;
        private short m_fpsSamplesCount = 0;
        private float m_unscaledDeltaTime = 0f;
        private int m_fpsAverageWindowSum = 0;
        private G_Histogram m_histogram;

        // This cap prevents the histogram from re-allocating memory in the
        // case of an unexpectedly high frame rate. The limit is somewhat
        // arbitrary. The only real cost to a higher cap is memory.
        private const short m_histogramFpsCap = 999;

        // CPU and GPU tracking
        private G_DoubleEndedQueue m_cpuSamples;
        private G_DoubleEndedQueue m_gpuSamples;
        private float m_cpuAverageWindowSum = 0f;
        private float m_gpuAverageWindowSum = 0f;
        private FrameTiming[] m_frameTimings = new FrameTiming[1];

        #endregion

        #region Properties -> Public

        public short CurrentFPS { get; private set; } = 0;
        public short AverageFPS { get; private set; } = 0;
        public short OnePercentFPS { get; private set; } = 0;
        public short Zero1PercentFps { get; private set; } = 0;
        
        // CPU tracking properties (in ms)
        public float CurrentCPU { get; private set; } = 0;
        public float AverageCPU { get; private set; } = 0;
        public float OnePercentCPU { get; private set; } = 0;
        public float Zero1PercentCpu { get; private set; } = 0;
        
        // GPU tracking properties (in ms)
        public float CurrentGPU { get; private set; } = 0;
        public float AverageGPU { get; private set; } = 0;
        public float OnePercentGPU { get; private set; } = 0;
        public float Zero1PercentGpu { get; private set; } = 0;

        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            m_unscaledDeltaTime = Time.unscaledDeltaTime;

            // Capture frame timings for CPU/GPU
            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(1, m_frameTimings);
            float cpuTime = (float)m_frameTimings[0].cpuFrameTime;
            float gpuTime = (float)m_frameTimings[0].gpuFrameTime;

            // Update fps and ms
            CurrentFPS = (short) (Mathf.RoundToInt( 1f / m_unscaledDeltaTime ));
            CurrentCPU = cpuTime;
            CurrentGPU = gpuTime;

            // Update FPS statistics
            uint averageAddedFps = 0;
            m_fpsSamplesCount = UpdateStatistics( CurrentFPS );
            averageAddedFps = (uint) m_fpsAverageWindowSum;
            AverageFPS = (short) ((float) averageAddedFps / (float) m_fpsSamplesCount);

            // Update CPU statistics
            UpdateCpuStatistics( cpuTime );
            AverageCPU = m_cpuSamples.Count > 0 ? m_cpuAverageWindowSum / m_cpuSamples.Count : 0f;

            // Update GPU statistics
            UpdateGpuStatistics( gpuTime );
            AverageGPU = m_gpuSamples.Count > 0 ? m_gpuAverageWindowSum / m_gpuSamples.Count : 0f;

            // Update FPS percent lows
            short samplesBelowOnePercent = (short) Mathf.Min( m_fpsSamplesCount - 1, m_onePercentSamples );
            m_histogram.WriteToSortedArray( m_fpsSamplesSorted, samplesBelowOnePercent + 1 );

            // Calculate 0.1% and 1% quantiles for FPS
            Zero1PercentFps = (short) Mathf.RoundToInt( CalculateQuantile( 0.001f ) );
            OnePercentFPS = (short) Mathf.RoundToInt( CalculateQuantile( 0.01f ) );
            
            // Calculate CPU percentiles using standard deviation approach
            float cpuStdDev = CalculateCpuStandardDeviation();
            OnePercentCPU = AverageCPU + cpuStdDev * 2.58f;  // 99th percentile
            Zero1PercentCpu = AverageCPU + cpuStdDev * 3.29f;  // 99.9th percentile
            
            // Calculate GPU percentiles using standard deviation approach
            float gpuStdDev = CalculateGpuStandardDeviation();
            OnePercentGPU = AverageGPU + gpuStdDev * 2.58f;  // 99th percentile
            Zero1PercentGpu = AverageGPU + gpuStdDev * 3.29f;  // 99.9th percentile
        }

        #endregion

        #region Methods -> Public

        public void UpdateParameters()
        {
            m_onePercentSamples = (short) (m_fpsSamplesCapacity / 100);
            if( m_onePercentSamples + 1 > m_fpsSamplesSorted.Length )
            {
                m_fpsSamplesSorted = new short[ m_onePercentSamples + 1 ];
            }
        }

        public void Reset()
        {
            m_fpsSamples.Clear();
            m_cpuSamples.Clear();
            m_gpuSamples.Clear();
            m_fpsSamplesCount = 0;
            m_fpsAverageWindowSum = 0;
            m_cpuAverageWindowSum = 0f;
            m_gpuAverageWindowSum = 0f;
            m_histogram.Clear();
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            m_fpsSamples = new G_DoubleEndedQueue( m_fpsSamplesCapacity );
            m_fpsSamplesSorted = new short[ m_onePercentSamples + 1 ];
            m_histogram = new G_Histogram( 0, m_histogramFpsCap );
            
            // Initialize CPU and GPU sample queues
            m_cpuSamples = new G_DoubleEndedQueue( m_fpsSamplesCapacity );
            m_gpuSamples = new G_DoubleEndedQueue( m_fpsSamplesCapacity );
            
            UpdateParameters();
        }

        private short UpdateStatistics( short fps )
        {
            if( m_fpsSamples.Full )
            {
                short remove = m_fpsSamples.PopFront();
                m_fpsAverageWindowSum -= remove;
                m_histogram.RemoveSample( remove );
            }
            m_fpsSamples.PushBack( fps );
            m_fpsAverageWindowSum += fps;
            m_histogram.AddSample( fps );
            return m_fpsSamples.Count;
        }

        private float CalculateQuantile( float quantile )
        {
            // If there aren't enough samples to calculate the quantile yet,
            // this function will instead return the lowest value in the
            // histogram.

            short samples = m_fpsSamples.Count;
            float position = ( samples + 1 ) * quantile - 1;
            short indexLow = (short) ( position > 0 ? Mathf.FloorToInt( position ) : 0 );
            short indexHigh = (short) ( indexLow + 1 < samples? indexLow + 1 : indexLow );
            float valueLow = m_fpsSamplesSorted[ indexLow ];
            float valueHigh = m_fpsSamplesSorted[ indexHigh ];
            float lerp = Mathf.Max( position - indexLow, 0 );
            return Mathf.Lerp( valueLow, valueHigh, lerp );
        }
        
        private void UpdateCpuStatistics( float cpuTime )
        {
            if( m_cpuSamples.Full )
            {
                float remove = (float)m_cpuSamples.PopFront();
                m_cpuAverageWindowSum -= remove;
            }
            m_cpuSamples.PushBack( (short)(cpuTime * 1000) );  // Store as int milliseconds
            m_cpuAverageWindowSum += cpuTime;
        }
        
        private void UpdateGpuStatistics( float gpuTime )
        {
            if( m_gpuSamples.Full )
            {
                float remove = (float)m_gpuSamples.PopFront();
                m_gpuAverageWindowSum -= remove;
            }
            m_gpuSamples.PushBack( (short)(gpuTime * 1000) );  // Store as int milliseconds  
            m_gpuAverageWindowSum += gpuTime;
        }
        
        private float CalculateCpuStandardDeviation()
        {
            if( m_cpuSamples.Count < 2 ) return 0f;
            
            float mean = AverageCPU;
            float sumSquaredDiffs = 0f;
            int count = m_cpuSamples.Count;
            
            // We need to temporarily extract values to calculate standard deviation
            // Since DoubleEndedQueue doesn't support indexing, we'll pop and re-push
            short[] tempValues = new short[count];
            for( int i = 0; i < count; i++ )
            {
                tempValues[i] = m_cpuSamples.PopFront();
            }
            
            // Calculate standard deviation and re-push values
            for( int i = 0; i < count; i++ )
            {
                float value = tempValues[i] / 1000f;  // Convert back to seconds
                float diff = value - mean;
                sumSquaredDiffs += diff * diff;
                m_cpuSamples.PushBack( tempValues[i] );
            }
            
            return Mathf.Sqrt( sumSquaredDiffs / (count - 1) );
        }
        
        private float CalculateGpuStandardDeviation()
        {
            if( m_gpuSamples.Count < 2 ) return 0f;
            
            float mean = AverageGPU;
            float sumSquaredDiffs = 0f;
            int count = m_gpuSamples.Count;
            
            // We need to temporarily extract values to calculate standard deviation
            // Since DoubleEndedQueue doesn't support indexing, we'll pop and re-push
            short[] tempValues = new short[count];
            for( int i = 0; i < count; i++ )
            {
                tempValues[i] = m_gpuSamples.PopFront();
            }
            
            // Calculate standard deviation and re-push values
            for( int i = 0; i < count; i++ )
            {
                float value = tempValues[i] / 1000f;  // Convert back to seconds
                float diff = value - mean;
                sumSquaredDiffs += diff * diff;
                m_gpuSamples.PushBack( tempValues[i] );
            }
            
            return Mathf.Sqrt( sumSquaredDiffs / (count - 1) );
        }

        #endregion
    }
}