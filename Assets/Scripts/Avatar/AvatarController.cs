using Pico.Avatar;
using System.Collections;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace MicroWar.Avatar
{
    public class AvatarController : MonoBehaviour
    {
        private static XRInteractionManager interactionManagerCache = null;
        //private const float userHeightToCameraOffset = 0.14f;

        private const float headHeightOffset = 0.2f;
        private const float avatarColliderRadius = 0.3f;


        [SerializeField]
        private Transform loadingIndicator;
        [SerializeField]
        private Transform avatarRoot;

        public bool ShowLoadingIfEmpty { get; set; } = true;
        public bool DontShowLoading { get; set; } = false;
        public bool IsAnchored { get; private set; } = false;
        public bool IsUserClone { get; set; } = false;
        public float CalibratedHeight { get; private set; }

        private PicoAvatar avatar;
        private IXRInteractable simpleInteractable;
        private CapsuleCollider avatarCollider;

        private Vector3 anchoredPosition = Vector3.zero;
        private bool isAvatarBindingDone = false;

        public PicoAvatar Avatar
        {
            get => avatar;
            set
            {

                avatar = value;
                avatar.transform.parent = avatarRoot;
                avatar.transform.localPosition = Vector3.zero;
                avatar.transform.localRotation = Quaternion.identity;
                if (!avatar.entity.isAnyLodReady)
                {
                    avatar.entity.OnAvatarLodReady.AddListener(OnAvatarReady);
                    ShowLoadingIndicator();
                }
                else //Avatar is ready
                {
                    StartCoroutine(CompleteAvatarBinding());
                }
            }
        }


        private void Start()
        {
            if (interactionManagerCache == null)
            {
                interactionManagerCache = Object.FindObjectOfType<XRInteractionManager>();
            }

            simpleInteractable = GetComponent<XRSimpleInteractable>();
            if (simpleInteractable == null)
            {
                DebugUtils.LogError(nameof(AvatarController), "Attach a XRSimpleInteractable component to the Avatar Controller.");
            }

            Initialise();

            if ((avatar == null || !avatar.isAnyEntityReady) && ShowLoadingIfEmpty)
            {
                ShowLoadingIndicator();
            }
        }

        private void Update()
        {
            if (IsAnchored)
            {
                avatarRoot.position = anchoredPosition;
            }
        }

        private void FixedUpdate()
        {
            if (isAvatarBindingDone && avatar != null && avatar.isAnyEntityReady)
            {
                UpdateAvatarCollider();
            }
        }

        private void LateUpdate()
        {
            if (IsAnchored)
            {
                GameObject head = avatar.GetJointObject(JointType.Head);
                if (head == null) return;

                Vector3 headPos = head.transform.position;
                float newX = anchoredPosition.x + (anchoredPosition.x - headPos.x);
                float newZ = anchoredPosition.z + (anchoredPosition.z - headPos.z);
                avatarRoot.position = new Vector3(newX, anchoredPosition.y, newZ);
            }
        }

        private void OnDestroy()
        {
            if (avatar != null && avatar.entity != null)
            {
                avatar.entity.OnAvatarLodReady.RemoveListener(OnAvatarReady);
            }
        }

        private void Initialise()
        {
            HideLoadingIndicator();
        }

        private void OnAvatarReady()
        {
            HideLoadingIndicator();
            StartCoroutine(CompleteAvatarBinding());
        }

        private IEnumerator CompleteAvatarBinding()
        {

            if (avatar.capabilities.controlSourceType == ControlSourceType.MainPlayer)
            {
#if !UNITY_EDITOR
                CalibrateAvatarHeight();
#endif
                yield break;
            }

            //Wait for a frame so that the critical joints gets ready.
            yield return null;


            SetupAvatarCollider();
            isAvatarBindingDone = true;
        }

        private void SetupAvatarCollider()
        {
            if (avatar == null || avatar.entity == null || simpleInteractable == null) return;

            //DebugAvatarHeadPosition();

            avatarCollider = avatar.entity.gameObject.AddComponent<CapsuleCollider>();
            avatar.entity.gameObject.layer = AvatarManager.Instance.AvatarContextInteractionLayer;
            simpleInteractable.colliders.Add(avatarCollider);

            //Dynamically adding colliders to a XRSimpleInteractable object doesn't work. We do a simple hack to get the colliders working.
            interactionManagerCache.UnregisterInteractable(simpleInteractable);
            interactionManagerCache.RegisterInteractable(simpleInteractable);

            UpdateAvatarCollider();
        }

        private void UpdateAvatarCollider()
        {
            if (avatarCollider == null || avatarCollider.IsDestroyed()) return;

            Vector3 avatarLocalPos = GetAvatarHeadPositionLocal();

            float approxHeight = GetAvatarHeight();
            Vector3 center = new Vector3(avatarLocalPos.x, approxHeight / 2f, avatarLocalPos.z);
            ResizeAvatarCollider(approxHeight, avatarColliderRadius, center);
        }

        private void ShowLoadingIndicator()
        {
            if (DontShowLoading) return;

            GameObject loadingObject = loadingIndicator.gameObject;

            //Hide loading indicator from the main camera.
            if (avatar != null)
            {
                if (avatar.capabilities.controlSourceType == ControlSourceType.MainPlayer && loadingObject.layer != AvatarManager.Instance.UserAvatarCullingLayer)
                {
                    loadingObject.SetLayerRecursively(AvatarManager.Instance.UserAvatarCullingLayer);
                }
            }

            if (!loadingObject.activeInHierarchy)
            {
                loadingObject.SetActive(true);
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator.gameObject.activeInHierarchy)
            {
                loadingIndicator.gameObject.SetActive(false);
            }
        }

        public void LoadLod(AvatarLodLevel lodLevel)
        {
            avatar.entity.ForceLod(lodLevel);
        }

        private Vector3 GetAvatarHeadPositionLocal()
        {
            GameObject headJoint = avatar.GetJointObject(JointType.Head);
            if (headJoint == null)
            {
                DebugUtils.LogError(nameof(AvatarController),"Unable to get head joint. Set head joint as a critical joint.");
                return Vector3.zero;
            }

            return headJoint.transform.localPosition;
        }

        public float GetAvatarHeight()
        {
            Vector3 avatarLocalPos = GetAvatarHeadPositionLocal();
            float approxHeight = avatarLocalPos.y + headHeightOffset;

            return approxHeight;
        }

        public void ResizeAvatarCollider(float height, float radius, Vector3 center)
        {
            avatarCollider.height = height;
            avatarCollider.radius = radius;
            avatarCollider.center = center;
        }

        public void AnchorAvatar()
        {
            anchoredPosition = avatarRoot.transform.position;
            IsAnchored = true;
        }

        public void EnableArmStretch()
        {
            avatar.entity.bodyAnimController.bipedIKController.SetStretchEnable(IKEffectorType.LeftHand, true);
            avatar.entity.bodyAnimController.bipedIKController.SetStretchEnable(IKEffectorType.RightHand, true);
            avatar.entity.bodyAnimController.bipedIKController.SetMaxStretchLength(IKEffectorType.LeftHand, 3f);
            avatar.entity.bodyAnimController.bipedIKController.SetMaxStretchLength(IKEffectorType.RightHand, 3f);
            avatar.entity.bodyAnimController.bipedIKController.SetRotationLimitEnable(JointType.LeftHandWrist, false);
            avatar.entity.bodyAnimController.bipedIKController.SetRotationLimitEnable(JointType.RightHandWrist, false);
        }

        public void CalibrateAvatarHeight()
        {
            CalibratedHeight = Camera.main.transform.position.y;// + userHeightToCameraOffset;
            avatar.entity.bodyAnimController.SetAvatarHeight(CalibratedHeight, true);
            EnableArmStretch();
        }

        public void SetAvatarHeight(float height)
        {
            if (avatar != null)
            {
                avatar.entity.bodyAnimController.SetAvatarHeight(height, true);
            }
        }

        private void DebugAvatarHeadPosition()
        {
            GameObject head = avatar.GetJointObject(JointType.Head);
            GameObject headCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headCube.transform.localScale = Vector3.one / 5f;
            headCube.transform.parent = head.transform;
            headCube.transform.localPosition = Vector3.zero;
            headCube.transform.localRotation = Quaternion.identity;
        }

        public void EnableOutline()
        {
            avatar.SetAvatarEffectKind(AvatarEffectKind.Black);
        }

        public void DisableOutline()
        {
            avatar.SetAvatarEffectKind(AvatarEffectKind.None);
        }

    }
}
