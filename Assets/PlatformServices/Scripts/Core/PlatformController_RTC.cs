using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pico.Platform;
using Pico.Platform.Models;
namespace MicroWar.Platform
{
    public class PlatformController_RTC : ControllerBase
    {
        private const int tokenTTL = 1800;
        private string rtcRoomID = null;
        private bool isInitialized = false;//Is RTC service initialized
        private bool isInRtcRoom = false;
        private string rtcToken = null;
        private Dictionary<RtcPrivilege, int> privilege = new Dictionary<RtcPrivilege, int>();

        //internal static SystemVoipStatus systemVoipStatus = SystemVoipStatus.Unknown;
        internal override void DelayInit()
        {
            var initState = RtcService.InitRtcEngine();
            if (initState.Equals(RtcEngineInitResult.AlreadyInitialized) || initState.Equals(RtcEngineInitResult.Success))
            {
                isInitialized = true;
                privilege.Add(RtcPrivilege.PublishStream, 3600 * 2);
                privilege.Add(RtcPrivilege.SubscribeStream, 3600 * 2);
                RtcService.SetAudioScenario(RtcAudioScenarioType.GameStreaming);
                RtcService.EnableAudioPropertiesReport(300);
                RtcService.SetCaptureVolume(400); //Could be adjusted.
                RtcService.SetPlaybackVolume(400); //Could be adjusted.
                BindCallbacks();
                Debug.Log($"[PVRRTC]: RTC Init success!.");
            }
            else
            {
                isInitialized = false;
                Debug.LogError($"RTC Initialize failed. Result:  {initState}");
            }
        }

        private void BindCallbacks()
        {
            //Bind RTC Callbacks
            //RtcService.SetOnRemoteAudioPropertiesReport(OnRemoteAudioPropertiesReport);
            //RtcService.SetOnUserMessageSendResult(OnUserMessageSendResult);
            PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(RoomUpdateEventHandler);
            RtcService.SetOnTokenWillExpire(OnTokenWillExpire);
            RtcService.SetOnJoinRoomResultCallback(OnJoinRoomResultCallback);
            RtcService.SetOnUserJoinRoomResultCallback(UserJoinRoomResultCallback);
            RtcService.SetOnConnectionStateChangeCallback(OnConnectionStateChangeCallback);
            RtcService.SetOnLeaveRoomResultCallback(OnLeaveRoomResultCallback);
            RtcService.SetOnWarnCallback(message =>
            {
                if (message.IsError)
                {
                    Debug.LogError($"[PVRRTC]: {message.Error} message: {message.Error.Message}");
                }
                Debug.LogError($"[PVRRTC] warn code: {message.Data}");
            });
            RtcService.SetOnRoomStatsCallback(msg => 
            {
                if (msg.IsError)
                {
                    var err = msg.GetError();
                    Debug.LogError($"[PVRRTC]: Roomstats Error {err.Code} {err.Message}");
                    return;
                }
                var res = msg.Data;
               // Debug.Log($"[PVRRTC] Roomstats: RoomId={res.RoomId} UserCount={res.UserCount} Duration={res.TotalDuration}");
            });
        }

        private void RoomUpdateEventHandler(EventWrapper<RoomUpdateEvent> msg)
        {
            var roomEvent = msg.Data;
            //Room status update
            if(msg.NotificationType == NotificationType.RoomServiceStatus)
            {
                if(roomEvent.RoomServiceStatus == RoomServiceStatus.InRoom)
                {
                    TryJoinRTCRoom(roomEvent.CurrentRoom.RoomId.ToString());
                }
                else
                {
                    LeaveRTCRoom(rtcRoomID);
                }
            }
        }

        #region Callbacks
        private void OnTokenWillExpire(Pico.Platform.Message<string> msg)
        {
            if (msg.IsError)
                return;

            RtcService.UpdateToken(rtcRoomID, rtcToken);
        }
        //userID/Is Send message
        private  Dictionary<string, bool> remoteUsers = new Dictionary<string, bool>();
        private  void OnRemoteAudioPropertiesReport(Pico.Platform.Message<RtcRemoteAudioPropertiesReport> msg)
        {
            if (msg.IsError)
            {
                Debug.LogError($"Receive remote audio report info error: error code:{msg.Error.Code}| msg:{msg.Error.Message}");
                return;
            }

            var infos = msg.Data.AudioPropertiesInfos;
            remoteUsers.Clear();
            foreach (RtcRemoteAudioPropertiesInfo info in infos)
            {
                string remoteUserID = info.StreamKey.UserId;
                if (!remoteUsers.ContainsKey(remoteUserID))
                {
                    remoteUsers.Add(remoteUserID, false);
                }
                //Debug.Log($"Remote User: {remoteUserID}");
            }
        }

        private void UserJoinRoomResultCallback(Pico.Platform.Message<RtcUserJoinInfo> message)
        {
            if (message.IsError)
            {
                var err = message.Error;
                return;
            }

            var res = message.Data;
            if (!remoteUsers.ContainsKey(res.UserId))
            {
                remoteUsers.Add(res.UserId, true);
            }

            //Handle new joined user.
           Debug.Log($"[PVRRTC] User join room callback: user={res.UserId} join room={res.RoomId},UserExtra={res.UserExtra},TimeElapsed{res.Elapsed}");

        }

        private void OnJoinRoomResultCallback(Pico.Platform.Message<RtcJoinRoomResult> message)
        {
            if (message.IsError)
            {
                Debug.LogError($"[PVRRTC]: Join RTC Room failed! Error:{message.GetError()}");
                return;
            }
            string _roomID = message.Data.RoomId;
            //Join Room Success.
            Debug.Log($"[PVRRTC]: Join RTC Room {_roomID} Success!");
            RtcService.PublishRoom(_roomID);
            RtcService.StartAudioCapture();

            //Init.
            isInRtcRoom = true;
            rtcRoomID = _roomID;
        }

        private void OnLeaveRoomResultCallback(Pico.Platform.Message<RtcLeaveRoomResult> message)
        {
            if (message.IsError)
            {
                Debug.LogError($"[PVRRTC]: Leave RTC Room failed! Error:{message.GetError()}");
                return;
            }
            isInRtcRoom = false;
            remoteUsers.Clear();
            Debug.Log("[PVRRTC]: Leave RTC Room success!");
        }
        private void OnConnectionStateChangeCallback(Pico.Platform.Message<RtcConnectionState> message)
        {
            if (message.IsError)
            {

                Debug.LogError($"[PVRRTC]: Connection state change error: {message.GetError()}");
                return;
            }
            RtcConnectionState rtcConnectionState = message.Data;

            if (rtcConnectionState != RtcConnectionState.Connected && rtcConnectionState != RtcConnectionState.Reconnected)
            {
                //Handle the offline situation.
                isInRtcRoom = false;
                rtcRoomID = null;
                remoteUsers.Clear();
                }
            Debug.Log($"[PVRRTC]: Connection state change: {message.Data}");
        }
        #endregion

        private void TryJoinRTCRoom(string roomID)
        {
            if (!isInitialized)
            {
                Debug.LogError("[PVRRTC]:RTC is not Initialized!");
                return;
            }

            if (isInRtcRoom)
            {
                Debug.Log("[PVRRTC]:Already in a RTC Room, Try leave room first");
                RtcService.LeaveRoom(roomID);
            }

            rtcRoomID = null;
            var currentUserId = PlatformServiceManager.Instance.Me.ID;
            if (string.IsNullOrEmpty(currentUserId))
            {
                UserService.GetLoggedInUser().OnComplete(message =>
                {
                    if (message.IsError)
                    {
                        Debug.Log($"[PVRRTC]: Unable to fetch user information. ");
                        currentUserId = null;
                        return;
                    }
                    currentUserId = message.Data.ID;
                    JoinRTCRoom(roomID,currentUserId);
                });
                return;
            }
            JoinRTCRoom(roomID,currentUserId);
        }


        private void JoinRTCRoom(string roomId, string currentUserId)
        {
            if (roomId == string.Empty)
                return;
            
            RtcService.GetToken(roomId, currentUserId, tokenTTL, privilege).OnComplete(result =>
            {
                if (result.IsError)
                {
                    Debug.LogWarning($"[PVRRTC]: PICO RTC Service get token failed. Result:  {result.Error.Code} | {result.Error.Message} ");
                    return;
                }
                rtcToken = result.Data;
                RtcRoomOptions rtcRoomOp = new RtcRoomOptions();
                rtcRoomOp.SetRoomId(roomId);
                rtcRoomOp.SetUserId(currentUserId);
                rtcRoomOp.SetToken(rtcToken);
                rtcRoomOp.SetRoomProfileType(RtcRoomProfileType.Game);
                rtcRoomOp.SetIsAutoSubscribeAudio(true);
                RtcService.JoinRoom2(rtcRoomOp, true);
            });
        }
        private void LeaveRTCRoom(string rtcRoomId)
        {
            if (rtcRoomId == null)
                return;

            Debug.Log("[PVRRTC]: Leave Room");
            string strRoomID = rtcRoomId;

            RtcService.StopAudioCapture();
            RtcService.UnPublishRoom(strRoomID);
            RtcService.LeaveRoom(strRoomID);

            //Init.
            rtcRoomID = null;
            rtcToken = null;
        }
        internal override void RegisterController()
        {
            PlatformServiceManager.Instance.AddController(this);
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        public void CheckRTCEnable(bool isUseRTC)
        {
            if (!isInRtcRoom) return;

            if (isUseRTC)
            {
                RtcService.PublishRoom(rtcRoomID);
                RtcService.RoomResumeAllSubscribedStream(rtcRoomID);
            }
            else
            {
                RtcService.UnPublishRoom(rtcRoomID);
                RtcService.RoomPauseAllSubscribedStream(rtcRoomID);
            }
        }

    }


}

