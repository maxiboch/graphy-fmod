/* ---------------------------------------
 * Author:          Martin Pane (martintayx@gmail.com) (@martinTayx)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            23-Dec-17
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

using System.Collections.Generic;
using System.Linq;

#if GRAPHY_BUILTIN_AUDIO
using Tayx.Graphy.Audio;
#endif // GRAPHY_BUILTIN_AUDIO

#if GRAPHY_FMOD
using Tayx.Graphy.Fmod;
#endif // GRAPHY_FMOD

using Tayx.Graphy.Fps;
using Tayx.Graphy.Ram;
using Tayx.Graphy.Utils;

namespace Tayx.Graphy
{
    /// <summary>
    /// Main class to access the Graphy Debugger API.
    /// </summary>
    public class GraphyDebugger : G_Singleton<GraphyDebugger>
    {
        protected GraphyDebugger()
        {
        }

        #region Enums -> Public

        public enum DebugVariable
        {
            Fps,
            Fps_Min,
            Fps_Max,
            Fps_Avg,
            Cpu,
            Cpu_Min,
            Cpu_Max,
            Cpu_Avg,
            Gpu,
            Gpu_Min,
            Gpu_Max,
            Gpu_Avg,
            Ram_Allocated,
            Ram_Reserved,
            Ram_Mono,
#if GRAPHY_BUILTIN_AUDIO
            Audio_DB,
#endif // GRAPHY_BUILTIN_AUDIO
#if GRAPHY_FMOD
            Fmod_Cpu,
            Fmod_Cpu_Avg,
            Fmod_Cpu_Peak,
            Fmod_Memory,
            Fmod_Memory_Avg,
            Fmod_Memory_Peak,
            Fmod_Channels,
            Fmod_Channels_Avg,
            Fmod_Channels_Peak,
            Fmod_FileIO,
            Fmod_FileIO_Avg,
            Fmod_FileIO_Peak,
            Fmod_Audio_RMS_Left,
            Fmod_Audio_RMS_Right,
            Fmod_Audio_Peak_Left,
            Fmod_Audio_Peak_Right
#endif // GRAPHY_FMOD
        }

        public enum DebugComparer
        {
            Less_than,
            Equals_or_less_than,
            Equals,
            Equals_or_greater_than,
            Greater_than
        }

        public enum ConditionEvaluation
        {
            All_conditions_must_be_met,
            Only_one_condition_has_to_be_met,
        }

        public enum MessageType
        {
            Log,
            Warning,
            Error
        }

        #endregion

        #region Structs -> Public

        [System.Serializable]
        public struct DebugCondition
        {
            [Tooltip( "Variable to compare against" )]
            public DebugVariable Variable;

            [Tooltip( "Comparer operator to use" )]
            public DebugComparer Comparer;

            [Tooltip( "Value to compare against the chosen variable" )]
            public float Value;
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        public class DebugPacket
        {
            [Tooltip( "If false, it won't be checked" )]
            public bool Active = true;

            [Tooltip( "Optional Id. It's used to get or remove DebugPackets in runtime" )]
            public int Id;

            [Tooltip( "If true, once the actions are executed, this DebugPacket will delete itself" )]
            public bool ExecuteOnce = true;

            [Tooltip( "Time to wait before checking if conditions are met (use this to avoid low fps drops triggering the conditions when loading the game)" )]
            public float InitSleepTime = 2;

            [Tooltip( "Time to wait before checking if conditions are met again (once they have already been met and if ExecuteOnce is false)" )]
            public float ExecuteSleepTime = 2;

            public ConditionEvaluation ConditionEvaluation = ConditionEvaluation.All_conditions_must_be_met;

            [Tooltip( "List of conditions that will be checked each frame" )]
            public List<DebugCondition> DebugConditions = new List<DebugCondition>();

            // Actions on conditions met

            public MessageType MessageType;
            [Multiline] public string Message = string.Empty;
            public bool TakeScreenshot = false;
            public string ScreenshotFileName = "Graphy_Screenshot";

            [Tooltip( "If true, it pauses the editor" )]
            public bool DebugBreak = false;

            public UnityEvent UnityEvents;
            public List<System.Action> Callbacks = new List<System.Action>();


            private bool canBeChecked = false;
            private bool executed = false;

            private float timePassed = 0;

            public bool Check => canBeChecked;

            public void Update()
            {
                if( !canBeChecked )
                {
                    timePassed += Time.deltaTime;

                    if( (executed && timePassed >= ExecuteSleepTime)
                        || (!executed && timePassed >= InitSleepTime) )
                    {
                        canBeChecked = true;

                        timePassed = 0;
                    }
                }
            }

            public void Executed()
            {
                canBeChecked = false;
                executed = true;
            }
        }

        #endregion

        #region Variables -> Serialized Private

        [SerializeField] private List<DebugPacket> m_debugPackets = new List<DebugPacket>();

        #endregion

        #region Variables -> Private

        private G_FpsMonitor m_fpsMonitor = null;
        private G_RamMonitor m_ramMonitor = null;

#if GRAPHY_BUILTIN_AUDIO
        private G_AudioMonitor m_audioMonitor = null;
#endif // GRAPHY_BUILTIN_AUDIO

#if GRAPHY_FMOD
        private G_FmodMonitor m_fmodMonitor = null;
#endif // GRAPHY_FMOD

        #endregion

        #region Methods -> Unity Callbacks

        private void Start()
        {
            m_fpsMonitor = GetComponentInChildren<G_FpsMonitor>();
            m_ramMonitor = GetComponentInChildren<G_RamMonitor>();

#if GRAPHY_BUILTIN_AUDIO
            m_audioMonitor = GetComponentInChildren<G_AudioMonitor>();
#endif // GRAPHY_BUILTIN_AUDIO

#if GRAPHY_FMOD
            m_fmodMonitor = GetComponentInChildren<G_FmodMonitor>();
#endif // GRAPHY_FMOD
        }

        private void Update()
        {
            CheckDebugPackets();
        }

        #endregion

        #region Methods -> Public 

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket( DebugPacket newDebugPacket )
        {
            m_debugPackets?.Add( newDebugPacket );
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            DebugCondition newDebugCondition,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            System.Action newCallback
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions.Add( newDebugCondition );
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks.Add( newCallback );

            AddNewDebugPacket( newDebugPacket );
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            List<DebugCondition> newDebugConditions,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            System.Action newCallback
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions = newDebugConditions;
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks.Add( newCallback );

            AddNewDebugPacket( newDebugPacket );
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            DebugCondition newDebugCondition,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            List<System.Action> newCallbacks
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions.Add( newDebugCondition );
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks = newCallbacks;

            AddNewDebugPacket( newDebugPacket );
        }

        /// <summary>
        /// Add a new DebugPacket.
        /// </summary>
        public void AddNewDebugPacket
        (
            int newId,
            List<DebugCondition> newDebugConditions,
            MessageType newMessageType,
            string newMessage,
            bool newDebugBreak,
            List<System.Action> newCallbacks
        )
        {
            DebugPacket newDebugPacket = new DebugPacket();

            newDebugPacket.Id = newId;
            newDebugPacket.DebugConditions = newDebugConditions;
            newDebugPacket.MessageType = newMessageType;
            newDebugPacket.Message = newMessage;
            newDebugPacket.DebugBreak = newDebugBreak;
            newDebugPacket.Callbacks = newCallbacks;

            AddNewDebugPacket( newDebugPacket );
        }

        /// <summary>
        /// Returns the first Packet with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public DebugPacket GetFirstDebugPacketWithId( int packetId )
        {
            return m_debugPackets.First( x => x.Id == packetId );
        }

        /// <summary>
        /// Returns a list with all the Packets with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public List<DebugPacket> GetAllDebugPacketsWithId( int packetId )
        {
            return m_debugPackets.FindAll( x => x.Id == packetId );
        }

        /// <summary>
        /// Removes the first Packet with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public void RemoveFirstDebugPacketWithId( int packetId )
        {
            if( m_debugPackets != null && GetFirstDebugPacketWithId( packetId ) != null )
            {
                m_debugPackets.Remove( GetFirstDebugPacketWithId( packetId ) );
            }
        }

        /// <summary>
        /// Removes all the Packets with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="packetId"></param>
        /// <returns></returns>
        public void RemoveAllDebugPacketsWithId( int packetId )
        {
            if( m_debugPackets != null )
            {
                m_debugPackets.RemoveAll( x => x.Id == packetId );
            }
        }

        /// <summary>
        /// Add an Action callback to the first Packet with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="id"></param>
        public void AddCallbackToFirstDebugPacketWithId( System.Action callback, int id )
        {
            if( GetFirstDebugPacketWithId( id ) != null )
            {
                GetFirstDebugPacketWithId( id ).Callbacks.Add( callback );
            }
        }

        /// <summary>
        /// Add an Action callback to all the Packets with the specified ID in the DebugPacket list.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="id"></param>
        public void AddCallbackToAllDebugPacketWithId( System.Action callback, int id )
        {
            if( GetAllDebugPacketsWithId( id ) != null )
            {
                foreach( var debugPacket in GetAllDebugPacketsWithId( id ) )
                {
                    if( callback != null )
                    {
                        debugPacket.Callbacks.Add( callback );
                    }
                }
            }
        }

        #endregion

        #region Methods -> Private

        /// <summary>
        /// Checks all the Debug Packets to see if they have to be executed.
        /// </summary>
        private void CheckDebugPackets()
        {
            if( m_debugPackets == null )
            {
                return;
            }

            for( int i = 0; i < m_debugPackets.Count; i++ )
            {
                DebugPacket packet = m_debugPackets[ i ];

                if( packet != null && packet.Active )
                {
                    packet.Update();

                    if( packet.Check )
                    {
                        switch( packet.ConditionEvaluation )
                        {
                            case ConditionEvaluation.All_conditions_must_be_met:
                                int count = 0;

                                foreach( var packetDebugCondition in packet.DebugConditions )
                                {
                                    if( CheckIfConditionIsMet( packetDebugCondition ) )
                                    {
                                        count++;
                                    }
                                }

                                if( count >= packet.DebugConditions.Count )
                                {
                                    ExecuteOperationsInDebugPacket( packet );

                                    if( packet.ExecuteOnce )
                                    {
                                        m_debugPackets[ i ] = null;
                                    }
                                }

                                break;

                            case ConditionEvaluation.Only_one_condition_has_to_be_met:
                                foreach( var packetDebugCondition in packet.DebugConditions )
                                {
                                    if( CheckIfConditionIsMet( packetDebugCondition ) )
                                    {
                                        ExecuteOperationsInDebugPacket( packet );

                                        if( packet.ExecuteOnce )
                                        {
                                            m_debugPackets[ i ] = null;
                                        }

                                        break;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

            m_debugPackets.RemoveAll( ( packet ) => packet == null );
        }

        /// <summary>
        /// Returns true if a condition is met.
        /// </summary>
        /// <param name="debugCondition"></param>
        /// <returns></returns>
        private bool CheckIfConditionIsMet( DebugCondition debugCondition )
        {
            switch( debugCondition.Comparer )
            {
                case DebugComparer.Less_than:
                    return GetRequestedValueFromDebugVariable( debugCondition.Variable ) < debugCondition.Value;
                case DebugComparer.Equals_or_less_than:
                    return GetRequestedValueFromDebugVariable( debugCondition.Variable ) <= debugCondition.Value;
                case DebugComparer.Equals:
                    return Mathf.Approximately( GetRequestedValueFromDebugVariable( debugCondition.Variable ),
                        debugCondition.Value );
                case DebugComparer.Equals_or_greater_than:
                    return GetRequestedValueFromDebugVariable( debugCondition.Variable ) >= debugCondition.Value;
                case DebugComparer.Greater_than:
                    return GetRequestedValueFromDebugVariable( debugCondition.Variable ) > debugCondition.Value;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Obtains the requested value from the specified variable.
        /// </summary>
        /// <param name="debugVariable"></param>
        /// <returns></returns>
        private float GetRequestedValueFromDebugVariable( DebugVariable debugVariable )
        {
            switch( debugVariable )
            {
                case DebugVariable.Fps:
                    return m_fpsMonitor != null ? m_fpsMonitor.CurrentFPS : 0;
                case DebugVariable.Fps_Min:
                    return m_fpsMonitor != null ? m_fpsMonitor.OnePercentFPS : 0;
                case DebugVariable.Fps_Max:
                    return m_fpsMonitor != null ? m_fpsMonitor.Zero1PercentFps : 0;
                case DebugVariable.Fps_Avg:
                    return m_fpsMonitor != null ? m_fpsMonitor.AverageFPS : 0;
                    
                case DebugVariable.Cpu:
                    return m_fpsMonitor != null ? m_fpsMonitor.CurrentCPU : 0;
                case DebugVariable.Cpu_Min:
                    return m_fpsMonitor != null ? m_fpsMonitor.OnePercentCPU : 0;
                case DebugVariable.Cpu_Max:
                    return m_fpsMonitor != null ? m_fpsMonitor.Zero1PercentCpu : 0;
                case DebugVariable.Cpu_Avg:
                    return m_fpsMonitor != null ? m_fpsMonitor.AverageCPU : 0;
                    
                case DebugVariable.Gpu:
                    return m_fpsMonitor != null ? m_fpsMonitor.CurrentGPU : 0;
                case DebugVariable.Gpu_Min:
                    return m_fpsMonitor != null ? m_fpsMonitor.OnePercentGPU : 0;
                case DebugVariable.Gpu_Max:
                    return m_fpsMonitor != null ? m_fpsMonitor.Zero1PercentGpu : 0;
                case DebugVariable.Gpu_Avg:
                    return m_fpsMonitor != null ? m_fpsMonitor.AverageGPU : 0;

                case DebugVariable.Ram_Allocated:
                    return m_ramMonitor != null ? m_ramMonitor.AllocatedRam : 0;
                case DebugVariable.Ram_Reserved:
                    return m_ramMonitor != null ? m_ramMonitor.ReservedRam : 0;
                case DebugVariable.Ram_Mono:
                    return m_ramMonitor != null ? m_ramMonitor.MonoRam : 0;

#if GRAPHY_BUILTIN_AUDIO
                case DebugVariable.Audio_DB:
                    return m_audioMonitor != null ? m_audioMonitor.MaxDB : 0;
#endif // GRAPHY_BUILTIN_AUDIO

#if GRAPHY_FMOD
                case DebugVariable.Fmod_Cpu:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentFmodCpu : 0;
                case DebugVariable.Fmod_Cpu_Avg:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.AverageFmodCpu : 0;
                case DebugVariable.Fmod_Cpu_Peak:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.PeakFmodCpu : 0;
                    
                case DebugVariable.Fmod_Memory:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentFmodMemoryMB : 0;
                case DebugVariable.Fmod_Memory_Avg:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.AverageFmodMemoryMB : 0;
                case DebugVariable.Fmod_Memory_Peak:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.PeakFmodMemoryMB : 0;
                    
                case DebugVariable.Fmod_Channels:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentChannelsPlaying : 0;
                case DebugVariable.Fmod_Channels_Avg:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.AverageChannelsPlaying : 0;
                case DebugVariable.Fmod_Channels_Peak:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.PeakChannelsPlaying : 0;
                    
                case DebugVariable.Fmod_FileIO:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentFileUsageKBps : 0;
                case DebugVariable.Fmod_FileIO_Avg:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.AverageFileUsageKBps : 0;
                case DebugVariable.Fmod_FileIO_Peak:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.PeakFileUsageKBps : 0;
                    
                case DebugVariable.Fmod_Audio_RMS_Left:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentLeftRMS : 0;
                case DebugVariable.Fmod_Audio_RMS_Right:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentRightRMS : 0;
                case DebugVariable.Fmod_Audio_Peak_Left:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentLeftPeak : 0;
                case DebugVariable.Fmod_Audio_Peak_Right:
                    return m_fmodMonitor != null && m_fmodMonitor.IsAvailable ? m_fmodMonitor.CurrentRightPeak : 0;
#endif // GRAPHY_FMOD

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Executes the operations in the DebugPacket specified.
        /// </summary>
        /// <param name="debugPacket"></param>
        private void ExecuteOperationsInDebugPacket( DebugPacket debugPacket )
        {
            if( debugPacket != null )
            {
                if( debugPacket.DebugBreak )
                {
                    Debug.Break();
                }

                if( debugPacket.Message != "" )
                {
                    string message = "[Graphy] (" + System.DateTime.Now + "): " + debugPacket.Message;

                    switch( debugPacket.MessageType )
                    {
                        case MessageType.Log:
                            Debug.Log( message );
                            break;
                        case MessageType.Warning:
                            Debug.LogWarning( message );
                            break;
                        case MessageType.Error:
                            Debug.LogError( message );
                            break;
                    }
                }

                if( debugPacket.TakeScreenshot )
                {
                    string path = debugPacket.ScreenshotFileName + "_" + System.DateTime.Now + ".png";
                    path = path.Replace( "/", "-" ).Replace( " ", "_" ).Replace( ":", "-" );

                    ScreenCapture.CaptureScreenshot( path );
                }

                debugPacket.UnityEvents?.Invoke();

                foreach( Action callback in debugPacket.Callbacks )
                {
                    callback?.Invoke();
                }

                debugPacket.Executed();
            }
        }

        #endregion
    }
}