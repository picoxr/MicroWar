using MicroWar.Platform;
using Pico.Avatar;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

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

        public override string PlatformAppID => "9f7e83c0dacdd38eb9f7167258610888";

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
            }
        }

        private void OnAvatarUpdated(PicoAvatar avatar, long requestId, int errorCode, string message)
        {
            Debug.Log($"Avatar Updated. message:{message} - errorCode:{errorCode}");
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

            while (PlatformServiceManager.Instance.Me == null)
            {
                yield return null;
            }

            mainUserId = PlatformServiceManager.Instance.Me.ID;


            LoadUserAvatar(mainUserId);
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

        public void UnloadAvatar(PicoAvatar avatar)
        {
            PicoAvatarManager.instance.UnloadAvatar(avatar);
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
