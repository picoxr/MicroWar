using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Pico.Avatar.Sample
{
    public class XRAnimationController : MonoBehaviour
    {
        private PxrControllerTracking pxrControllerTrackingLeft = new PxrControllerTracking();
        private PxrControllerTracking pxrControllerTrackingRight = new PxrControllerTracking();
        private float[] headData = new float[7] { 0, 0, 0, 0, 0, 0, 0 };

        public float speed = 0.01f;
        private PicoAvatar targetAvatar;
        private AnimationDecompressor groupAssets;

        //move
        private CharacterController moveControl;
        //lerp
        private string[] lerpAnimationTest = new string[]{ "walking", "walkingBack", "walkingRight" , "walkingLeft" };
        private int curPlayIndex = 0;
        private HashSet<string> _loadedAnimationNames = new HashSet<string>();

        // Start is called before the first frame update
        void Start()
        {
            moveControl = gameObject.AddComponent<CharacterController>();
            moveControl.center = new Vector3(0, 1f, 0);
        }
        public void InitAnimationAssets(AnimationDecompressor asset)
        {
            groupAssets = asset;
        }
        public void LinkAvatar(PicoAvatar avatar)
        {
            if (avatar == null || avatar.entity == null)
                return;
            var animController = avatar.entity.bodyAnimController;
            if (animController == null)
            {
                Debug.LogError("animController is null!");
                return;
            }
            targetAvatar = avatar;
            init2DTreeStateTest(targetAvatar);
            initHandStateTest(targetAvatar);
            initHandPoseTest(targetAvatar);
            initLerpTest(targetAvatar);

            StartLoadAnimationData();
        }

        void StartLoadAnimationData()
        {
            string groupData = groupAssets.GetGroupPath("test1");
            if (string.IsNullOrEmpty(groupData))
                return;
            targetAvatar.OnLoadAnimationsExternComplete += OnLoadAnimationsExternComplete;
            targetAvatar.LoadAllAnimationsExtern(groupData);
        }
        void OnLoadAnimationsExternComplete(string assetBundlePath, string animationNamesJson)
        {
            var animController = targetAvatar.entity.bodyAnimController;
     
            var anims = LitJson.JsonMapper.ToObject(animationNamesJson);
            if (anims.IsArray)
            {
                for (int i = 0; i < anims.Count; ++i)
                {
                    string animName = anims[i].ToString();
                    if(!_loadedAnimationNames.Contains(animName))
                    {
                        if (animName == "walk_left.Take 001")
                        {
                            //add animation to state when load complete
                            //walkLeftState.AddAnimationClip(animName);
                        }
                        if (animName == "Female_AnTest.Take 001")
                        {
                            AvatarAnimationLayer animationLayer = animController.GetAnimationLayerByName("HandAction");
                            AvatarAnimationState raiseHandState = animationLayer.GetAnimationStateByName("raiseHandState");
                            //add animation to state when load complete
                            raiseHandState.AddAnimationClip(animName);
                        }
                        _loadedAnimationNames.Add(animName);
                    }
                }
            }
            string animationNames = targetAvatar.GetAnimationNames();
            LogTools("animations list: " + animationNames);
        }
        #region sample layer case
        private void initLerpTest(PicoAvatar avatar)
        {
            var animController = avatar.entity.bodyAnimController;
            //Create an animation Layer
            AvatarAnimationLayer testLayer1 = animController.CreateAnimationLayerByName("AnimationLerp");
            //Only enable rotation of the animations in this layer
            testLayer1.SetSRTEnable(false, true, false);
            testLayer1.SetLayerBlendMode(AnimLayerBlendMode.Lerp);
            for (int i = 0; i < lerpAnimationTest.Length; i++)
            {
                string animName = lerpAnimationTest[i];
                AvatarAnimationState walkFrontState = testLayer1.CreateAnimationStateByName(animName);
                walkFrontState.AddAnimationClip(animName);
                walkFrontState.SetWrapMode(0);
            }
        }
        private void initHandPoseTest(PicoAvatar avatar)
        {
            var animController = avatar.entity.bodyAnimController;
            //create an override clip layer with avatar mask for hand pose
            AvatarAnimationLayer testLayer1 = animController.CreateAnimationLayerByName("HandPose");
            testLayer1.SetSRTEnable(false, true, false);

            //set blend mode to override
            testLayer1.SetLayerBlendMode(AnimLayerBlendMode.Additive);

            AvatarAnimationState tempState = testLayer1.CreateAnimationStateByName("HandPoseState");
            tempState.AddStateStatusCallBack(AnimStateStatus.StateEnd, (animState) =>
            {
                OnStateEnd(animState);
            });
            tempState.AddStateStatusCallBack(AnimStateStatus.StateEnter, (animState) =>
            {
                OnStateEnter(animState);
            });
            tempState.AddStateStatusCallBack(AnimStateStatus.StateLeave, (animState) =>
            {
                OnStateLeave(animState);
            });
            //set wrapMode: only play animation once
            tempState.SetWrapMode(1);
            tempState.AddAnimationClip("rHandFist");

            //create a new avatar mask and mask out all other joints except hand pose
            Pico.Avatar.AvatarMask mask = new Pico.Avatar.AvatarMask(animController);
            for (uint i = 0; i < (uint)JointType.LeftHandThumbTrapezium; ++i)
            {
                mask.SetJointRotationEnable((JointType)i, false);
            }
            testLayer1.SetAvatarMask(mask);

        }
        private void initHandStateTest(PicoAvatar avatar)
        {
            var animController = avatar.entity.bodyAnimController;
            //Create an animation Layer
            AvatarAnimationLayer topActionLayer = animController.CreateAnimationLayerByName("HandAction");
            topActionLayer.SetSRTEnable(false, true, false);
            topActionLayer.SetLayerBlendMode(AnimLayerBlendMode.Additive);
            AvatarAnimationState raiseHandState = topActionLayer.CreateAnimationStateByName("raiseHandState");
       
            raiseHandState.SetWrapMode(0);

            Pico.Avatar.AvatarMask mask3 = new AvatarMask(animController);
            for (uint i = 0; i < (uint)JointType.RightShoulder; ++i)
            {
                mask3.SetJointRotationEnable((JointType)i, false);
            }

            for (uint i = (uint)JointType.BasicJointCount; i < (uint)JointType.Count; ++i)
            {
                mask3.SetJointRotationEnable((JointType)i, false);
            }
            topActionLayer.SetAvatarMask(mask3);

            //AvatarAnimationLayer testLayer4 = _bodyAnimController.CreateAnimationLayerByName("AnimationTestLayer4");
            //testLayer4.SetSRTEnable(false, true, false);
            //testLayer4.SetLayerBlendMode(AnimLayerBlendMode.Additive);
            //AvatarAnimationState fistState = testLayer4.CreateAnimationStateByName("fistState");
            //fistState.SetWrapMode(0);
            //fistState.AddAnimationClip("rHandFist");

            //Pico.Avatar.AvatarMask mask4 = new Pico.Avatar.AvatarMask();
            //for (uint i = 0; i < (uint)JointType.RightHandThumbRoot; ++i)
            //{
            //    mask4.AddJointWithType((JointType)i);
            //}
            //testLayer4.SetAvatarMask(mask4);
        }
        //2dBlendTree layer test
        private void init2DTreeStateTest(PicoAvatar avatar)
        {
            var animController = avatar.entity.bodyAnimController;
            //Create an animation Layer
            AvatarAnimationLayer testLayer = animController.CreateAnimationLayerByName("BottomAction");
            testLayer.SetSRTEnable(false, true, false);
            //Only enable rotation of the animations in this layer

            //Create an animation state for blendTree2D
            var animationState = testLayer.CreateAnimationStateByName("2DTreeState");
            //Create a blend tree for anim state
            animationState.CreateBlendTree(AnimBlendTreeType.BlendTree2D);
            //Register animation parameters in bodyAnimController
            animController.SetAnimationParameterFloatByName("joystickX", 0.0f);
            animController.SetAnimationParameterFloatByName("joystickY", 0.0f);
            //Set parameters to blendTree
            animationState.blendTree.AddParameterByName("joystickX");
            animationState.blendTree.AddParameterByName("joystickY");
            //Add animation clips to blendTree
            animationState.blendTree.AddAnimationClip("idle");
            animationState.blendTree.AddAnimationClip("walking");
            animationState.blendTree.AddAnimationClip("walkingBack");
            animationState.blendTree.AddAnimationClip("walkingLeft");
            animationState.blendTree.AddAnimationClip("walkingRight");
            //Set thresholds for each clip in blend tree (clip index is the order when you add clips to the blend tree)
            animationState.blendTree.SetThreshold2D(0, 0, 0);
            animationState.blendTree.SetThreshold2D(1, 0, -1);
            animationState.blendTree.SetThreshold2D(2, 0, 1);
            animationState.blendTree.SetThreshold2D(3, -1, 0);
            animationState.blendTree.SetThreshold2D(4, 1, 0);
        }
        
        #endregion
        #region move control
        void SetJoystickData(Vector2 touchPosition)
        {
            var bodyAnimController = targetAvatar.entity.bodyAnimController;
            //set animator parameter value
            bodyAnimController.SetAnimationParameterFloatByName("joystickX", touchPosition.x);
            bodyAnimController.SetAnimationParameterFloatByName("joystickY", touchPosition.y);

            MoveToDir(touchPosition);
        }
        void MoveToDir(Vector2 touchPosition)
        {
            if (moveControl == null)
                return;
            if (touchPosition.Equals(Vector2.zero))
                return;
            moveControl.Move(new Vector3(touchPosition.x * speed, 0, touchPosition.y  * speed));
        }
        #endregion
        // Update is called once per frame
        void Update()
        {
            if (targetAvatar == null)
                return;
#if UNITY_EDITOR
            Vector2 touchNormal = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                touchNormal.y = 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                touchNormal.y = -1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                touchNormal.x = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                touchNormal.x = 1;
            }

            SetJoystickData(touchNormal);
#endif


            //  PXR_Plugin.Controller.UPxr_GetControllerTrackingState((uint)PXR_Input.Controller.LeftController, 0, headData, ref pxrControllerTrackingLeft);
            PXR_Plugin.Controller.UPxr_GetControllerTrackingState((uint)PXR_Input.Controller.RightController, 0, headData, ref pxrControllerTrackingRight);
            uint rightControllerStatus = (uint)pxrControllerTrackingRight.localControllerPose.status;

            //判断控制器是否连接可以使用
            if (rightControllerStatus > 0)
            {
                Vector2 touchPosition;
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out touchPosition);

                SetJoystickData(touchPosition);

            }
            else
            {
                return;
            }
        }

        #region action call Function

        public void StartPlay2DBlendTree(bool isStop)
        {
            var _bodyAnimController = targetAvatar.entity.bodyAnimController;
            if (_bodyAnimController == null)
                return;
            LogTools("StartPlay2DBlendTree:"+isStop.ToString());
            AvatarAnimationLayer animationLayer = _bodyAnimController.GetAnimationLayerByName("BottomAction");
            if (animationLayer != null)
            {
                if (isStop)
                {
                    animationLayer.StopAnimation(0);
                    return;
                }
                //Play anim state, no fade time
                animationLayer.PlayAnimationState(animationLayer.GetAnimationStateByName("2DTreeState"), 0);
            }
        }
        public void StartPlayMaskHandLoop(bool isStop)
        {
            var _bodyAnimController = targetAvatar.entity.bodyAnimController;
            if (_bodyAnimController == null)
                return;
            LogTools("StartPlayMaskHand:" + isStop.ToString());
            AvatarAnimationLayer animationLayer = _bodyAnimController.GetAnimationLayerByName("HandAction");
            if (animationLayer != null)
            {
                if (isStop)
                {
                    animationLayer.StopAnimation(0);
                    _bodyAnimController.bipedIKController?.SetIKEnable(IKEffectorType.RightHand, true);
                    return;
                }
                _bodyAnimController.bipedIKController?.SetIKEnable(IKEffectorType.RightHand, false);
                //Play anim state, no fade time
                animationLayer.PlayAnimationState(animationLayer.GetAnimationStateByName("raiseHandState"), 0);
            }
        }
        public void StartPlayHandPoseOnce(bool isStop)
        {
            var _bodyAnimController = targetAvatar.entity.bodyAnimController;
            if (_bodyAnimController == null)
                return;
            LogTools("StartPlayHandPose:" + isStop.ToString());
            AvatarAnimationLayer animationLayer = _bodyAnimController.GetAnimationLayerByName("HandPose");
            if (animationLayer != null)
            {
                if (isStop)
                {
                    animationLayer.StopAnimation(0);
                    return;
                }
                var state = animationLayer.GetAnimationStateByName("HandPoseState");
                state.SetSpeed(0.05f);
                //Play anim state, 0.1f fade time
                animationLayer.PlayAnimationState(animationLayer.GetAnimationStateByName("HandPoseState"), 1);
            }
        }
        public bool StartPlayLerpAnimation(bool isStop)
        {
            var _bodyAnimController = targetAvatar.entity.bodyAnimController;
            if (_bodyAnimController == null)
                return false;
            LogTools("StartPlayLerpAnimation:" + curPlayIndex.ToString());
            AvatarAnimationLayer animationLayer = _bodyAnimController.GetAnimationLayerByName("AnimationLerp");
            if (animationLayer != null)
            {
                if (curPlayIndex >= lerpAnimationTest.Length)
                    isStop = true;
                else
                    isStop = false;
               
                if (isStop)
                {
                    curPlayIndex = 0;
                    animationLayer.StopAnimation(0.1f);
                    return false;
                }
                string animName = lerpAnimationTest[curPlayIndex];
                var state = animationLayer.GetAnimationStateByName(animName);
                //Play anim state, 0.1f fade time
                animationLayer.PlayAnimationState(state, 0.5f);
                curPlayIndex += 1;
                return true;
            }
            return false;
            
        }
        #endregion
        void LogTools(string content)
        {
            Debug.LogFormat("SampleXRAnimation {0} : {1}",Time.time,content);
        }
            void OnStateEnter(AvatarAnimationState animationState)
        {
            LogTools("stateEnter " + animationState.name);
        }

        void OnStateLeave(AvatarAnimationState animationState)
        {
            LogTools("stateLeave " + animationState.name);
        }

        void OnStateEnd(AvatarAnimationState animationState)
        {
            LogTools("stateEnd " + animationState.name);
            var _bodyAnimController = targetAvatar.entity.bodyAnimController;
            if (_bodyAnimController == null)
                return;
            if (animationState.name == "HandPoseState")
            {
                AvatarAnimationLayer animationLayer = _bodyAnimController.GetAnimationLayerByName("HandPose");
                if (animationLayer != null)
                {
                    //stop anim state, fade time 0.2s
                    animationLayer.StopAnimation(1);
                }
            }
          
        }

    }


}
