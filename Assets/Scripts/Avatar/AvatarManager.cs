using MicroWar.Platform;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Pico.Avatar;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace MicroWar.Avatar
{
    public class AvatarManager : AvatarAppLauncher
    {

        //DEBUG
        public string debugUserID = string.Empty;
        //[END] DEBUG

        private const string AVATAR_CONTEXT_INTERACTION_LAYER = "AvatarContext";
        private const string USER_AVATAR_LAYER = "UserAvatar";
        private const string USER_AVATAR_CULLING_LAYER = "UserAvatarCulling";

        public override string PlatformAppID => Pico.Platform.CoreService.GetAppID();//"9f7e83c0dacdd38eb9f7167258610888";

        [SerializeField]
        private GameObject avatarPrefab;

        public int UserAvatarLayer { get; private set; }
        public int UserAvatarCullingLayer { get; private set; }
        public int AvatarContextInteractionLayer { get; private set; }
        public AvatarController MainUserAvatar { get => userAvatarController; }
        public static AvatarManager Instance { get; private set; }

        [Header("Controller Refs")]
        public ActionBasedController leftController;
        public ActionBasedController rightController;

        [Header("Custom Hand Pose - Main User")]
        public GameObject LeftHandPoseSkeleton;
        public GameObject RightHandPoseSkeleton;
        public GameObject LeftHandPose;
        public GameObject RightHandPose;

        [Header("Height Calibration - Main User")]
        public InputActionReference CalibrateAvatarHeightAction;

        private AvatarController userAvatarController;

#if !UNITY_EDITOR
        private bool isPlatformReady = false;
#endif

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one instance of AvatarManager exists in the scene!");
                return;
            }

            Instance = this;

            //Code block below is for upcoming changes. It doesn't have a function atm.
            UserAvatarCullingLayer = LayerMask.NameToLayer(USER_AVATAR_CULLING_LAYER);
            UserAvatarLayer = LayerMask.NameToLayer(USER_AVATAR_LAYER);
            AvatarContextInteractionLayer = LayerMask.NameToLayer(AVATAR_CONTEXT_INTERACTION_LAYER);
            //[END]
        }

        private void Update()
        {
            if (MainUserAvatar != null && CalibrateAvatarHeightAction.action.IsPressed())
            {
                CalibrateAvatarHeight();
            }
        }

        private void OnDestroy()
        {
            if (PicoAvatarManager.instance != null)
            {
                PicoAvatarManager.instance.OnAvatarSpecificationUpdated -= OnAvatarUpdated;
                PicoAvatarManager.instance.RemoveAvatarChangeListener(OnAvatarChanged);
            }
        }

        private void OnAvatarUpdated(PicoAvatar avatar, int errorCode, string message)
        {
            Debug.Log($"OnAvatarSpecificationUpdated: Avatar Updated. message:{message} - errorCode:{errorCode}");
        }

        private IEnumerator Start()
        {
            string mainUserId = debugUserID;

            //Making sure that PICO Platform Services are ready
            while (!Pico.Platform.CoreService.IsInitialized())
            {
                yield return null;
            }           

            //Making sure that PicoAvatarApp is ready to use
            while (!PicoAvatarApp.isWorking)
            {
                yield return null;
            }

            //Providing required user info to launch PicoAvatarApp
            StartCoroutine(LaunchPicoAvatarApp());

            while (!PicoAvatarManager.canLoadAvatar)
            { 
                yield return null;
            }

            PicoAvatarManager.instance.OnAvatarSpecificationUpdated += OnAvatarUpdated;
            PicoAvatarManager.instance.AddAvatarChangeListener(OnAvatarChanged);

            while (PlatformServiceManager.Instance.Me == null)
            {
                yield return null;
            }

            mainUserId = PlatformServiceManager.Instance.Me.ID;


            LoadUserAvatar(mainUserId);
        }

        private void OnAvatarChanged(string data)
        {
            if (data == "0")
            {
                // Open Avatar Hub (suggest hiding the app's own panel at this time to avoid overlap)
                Debug.Log("<color=cyan>Open Avatar Hub</color>");

            }
            else if (data == "1")
            {
                // Avatar changed. Need to reload the avatar
                Debug.Log("<color=cyan>Avatar changed. Need to reload the avatar</color>");
                UnloadAvatar(MainUserAvatar.Avatar);
                MainUserAvatar.gameObject.SetActive(false);
                Destroy(MainUserAvatar, 1f);

                LoadUserAvatar(PlatformServiceManager.Instance.Me.ID);
            }
            else if (data == "2")
            {
                // Avatar updated. Need to reload the avatar
                Debug.Log("<color=cyan>Avatar updated. Need to reload the avatar</color>");

            }
            else if (data == "3")
            {
                // Directly exit Avatar Hub
                Debug.Log("<color=cyan>Directly exit Avatar Hub</color>");

            }
            else if (data == "4")
            {
                // Save edit to the avatar and exit Avatar Hub. Need to reload the avatar
                Debug.Log("<color=cyan>Save edit to the avatar and exit Avatar Hub. Need to reload the avatar</color>");

            }
            else if (data == "5")
            {
                // Click the Exit button to exit Avatar Hub without saving any edits to the avatar
                Debug.Log("<color=cyan>Click the Exit button to exit Avatar Hub without saving any edits to the avatar</color>");

            }
            else if (data == "6")
            {
                // Minimize Avatar Hub as a background app after clicking on any area outside the UI of Avatar Hub
                Debug.Log("<color=cyan>Minimize Avatar Hub as a background app after clicking on any area outside the UI of Avatar Hub</color>");

            }
        }

        //Load non-user avatar. In our case, we load remote player avatars by using this method.
        public AvatarController LoadAvatar(string userId, Transform spawnPoint, string jsonConfig = null)
        {
            AvatarCapabilities capabilities = new AvatarCapabilities();
            capabilities.manifestationType = AvatarManifestationType.Full;
            capabilities.controlSourceType = ControlSourceType.OtherPlayer;
            capabilities.inputSourceType = DeviceInputReaderBuilderInputType.RemotePackage;
            capabilities.recordBodyAnimLevel = RecordBodyAnimLevel.FullBone;
            capabilities.autoStopIK = true;
            capabilities.bodyCulling = true;

            PicoAvatar avatar = CreateAvatar(capabilities, userId, jsonConfig);

            avatar.criticalJoints = new JointType[] { JointType.Head };
            AvatarController avatarController = Instantiate(avatarPrefab, spawnPoint).GetComponent<AvatarController>();
            avatarController.ShowLoadingIfEmpty = true;
            avatarController.Avatar = avatar;


            return avatarController;
        }

        //Load main user avatar. Use LoadAvatar method for loading remote avatars.
        public void LoadUserAvatar(string mainUserId)
        {
            // Set Avatar Capabilities
            AvatarCapabilities capabilities = new AvatarCapabilities();
            capabilities.manifestationType = AvatarManifestationType.Full;
            capabilities.controlSourceType = ControlSourceType.MainPlayer;
            capabilities.inputSourceType = DeviceInputReaderBuilderInputType.PicoXR;
            capabilities.recordBodyAnimLevel = RecordBodyAnimLevel.FullBone;
            
            capabilities.bodyCulling = true;
            capabilities.headShowType = AvatarHeadShowType.Hide;

            //Create Avatar
            PicoAvatar userAvatar = CreateAvatar(capabilities, mainUserId, null);
            //Setting Right Hand and Left Hand Wrists as critical joints so that we can attach objects to hands.
            userAvatar.criticalJoints = new JointType[] { JointType.RightHandWrist, JointType.LeftHandWrist };

            //Wait for avatar entity to be ready
            userAvatar.AddFirstEntityReadyCallback((picoAvatar, avatarEntity) =>
            {
                if (avatarEntity == null) return;

                //Initialise Avatar Controller and bind avatar
                AvatarController avatarController = Instantiate(avatarPrefab, Camera.main.gameObject.transform.parent).GetComponent<AvatarController>();
                avatarController.Avatar = userAvatar;
                this.userAvatarController = avatarController;

                OnUserAvatarEntityReady(userAvatar);
            });
        }

        public bool UnloadAvatar(PicoAvatar avatar)
        {
            return PicoAvatarManager.instance.UnloadAvatar(avatar);
        }

        public void OpenAvatarEditor()
        {
            PicoAvatarManager.instance.StartAvatarEditor(null,(result) => 
            {
                Debug.Log("start avatar editor = " + result);
            });
        }

        //Not utilized in the project. This method retrieves custom avatars defined for the app.
        public void RequestUserAvatars()
        {
            RequestCharacterListRequest.DoRequest((NativeResult errorCode, string message) =>
            {
                var characterList = JsonConvert.DeserializeObject<JArray>(message);
                Debug.Log("LoadAvatar RequestCharacterList, errorCode= " + errorCode + ", message= " + message);
                for (int i = 0; i < characterList.Count; i++)
                {
                    var data = characterList[i];
                    // Output avatar IDs
                    var characterID = data.Value<string>("character_id");
                    Debug.Log("characterID  = " + characterID);
                    // Output avatar types
                    // 1：PICO's official avatars
                    // 2：Custom avatars uploaded to the Avatar Asset Platform
                    // 3：Avatars provided by the Creator Studio
                    var characterType = data.Value<string>("character_type");
                    Debug.Log("characterType = " + characterType);
                    // Output avatar versions
                    var characterVersion = data.Value<string>("item_online_version");
                    Debug.Log("characterVersion= " + characterVersion);
                }
            });
        }

        //Calls the SDK method to Load an avatar by using the given AvatarCapabilities and the userId. 
        //jsonConfig parameter is currently not utilized.
        private PicoAvatar CreateAvatar(AvatarCapabilities capabilities, string userId, string jsonConfig)
        {
            AvatarLoadContext loadContext;

            if (!string.IsNullOrEmpty(jsonConfig))
            {
                loadContext = new AvatarLoadContext(userId, string.Empty, jsonConfig, capabilities);
            }
            else
            {
                loadContext = new AvatarLoadContext(userId, string.Empty, string.Empty, capabilities);
            }

            return PicoAvatarManager.instance.LoadAvatar(
                loadContext,
                (picoAvatar, avatarEntity) =>
                {
                    DebugUtils.Log(nameof(AvatarManager), $"Avatar loaded. avatarEntity:{avatarEntity} userID:{userId} - Name:{picoAvatar?.name} - AvatarId:{picoAvatar?.avatarId}");
                });
        }

        private void OnUserAvatarEntityReady(PicoAvatar userAvatar)
        {
            //In case We want to render the head when we look at the mirrors so we set a custom layer for the user avatar. See the "Culling Mask" of mirror cameras.
            userAvatar.gameObject.SetLayerRecursively(this.UserAvatarLayer); //Doesn't have a function currently. Will be used when we add a mirror object in the game.

            //Enable FaceTracking and/or LipSync
            userAvatar.StartFaceTrack(true, true);//Should be (true (FaceTracking),true (LipSync)) for PICO 4 Pro
            AssignCustomHandPose(userAvatar, LeftHandPoseSkeleton, RightHandPoseSkeleton, LeftHandPose, RightHandPose);
        }

        private void AssignCustomHandPose(PicoAvatar avatar, GameObject leftHandPoseSkeleton, GameObject rightHandPoseSkeleton, GameObject leftHandPose, GameObject rightHandPose)
        {
            // Right
            Vector3 rightUp = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 rightForward = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightOffset = Vector3.zero;

            // Left
            Vector3 leftUp = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 leftForward = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 leftOffset = Vector3.zero;

            bool state = avatar.entity.SetCustomHand(HandSide.Right, rightHandPoseSkeleton, rightHandPose, rightUp, rightForward, rightOffset);
            state = state & avatar.entity.SetCustomHand(HandSide.Left, leftHandPoseSkeleton, leftHandPose, leftUp, leftForward, leftOffset);

            //We don't want the avatar to use the transform of the custom hand pose root so we disable sync with Wrist bone.
            avatar.entity.rightCustomHandPose.syncWristTransform = false;
            avatar.entity.leftCustomHandPose.syncWristTransform = false;

            if (!state)
            {
                Debug.Log("Failed! - SetCustomHand");
            }
        }

        private void CalibrateAvatarHeight()
        {
            MainUserAvatar.CalibrateAvatarHeight();
        }

        //private IEnumerator AttachControllers()
        //{
        //    yield return null;//Wait for a frame to let the SDK assign critical joints

        //    GameObject leftWrist = MainUserAvatar.Avatar.GetJointObject(JointType.LeftHandWrist);
        //    GameObject rightWrist = MainUserAvatar.Avatar.GetJointObject(JointType.RightHandWrist);
        //    if (leftController.model != null) leftController.model.transform.SetParent(leftWrist.transform);
        //    if (rightController.model != null) rightController.model.transform.SetParent(rightWrist.transform);
        //}

        private void OnApplicationQuit()
        {
            PicoAvatarApp.instance.StopApp();
        }

    }
}
