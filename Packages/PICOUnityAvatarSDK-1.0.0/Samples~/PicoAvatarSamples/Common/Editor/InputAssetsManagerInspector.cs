//#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            [CustomEditor(typeof(InputAssetsManager))]
            public class InputAssetsManagerInspector : Editor
            {

                public override void OnInspectorGUI()
                {
                    InputAssetsManager demoComp = (InputAssetsManager)target;
                    if (CanShowButton(demoComp))
                    {
                        if (GUILayout.Button("Init Load Action"))
                        {
                            LoadActions(demoComp);
                        }
                        if (GUILayout.Button("Init Set Action"))
                        {
                            SetActions(demoComp);
                        }
                    }

                    GUILayout.Space(2);

                    base.OnInspectorGUI();
                  
                }

                bool CanShowButton(InputAssetsManager manager)
                {
                    if (manager.inputAsset == null)
                        return false;
                    return true;
                }
                void LoadActions(InputAssetsManager manager)
                {
                    InputActionReference[] data = GetAllAssetReferencesFromAssetDatabase(manager.inputAsset);
                    manager.Clear();
                    foreach (var item in data)
                    {
                        var path = AssetDatabase.GetAssetPath(item);
                        if (item != null)
                        {
                            manager.AddInputActionReference(item);
                        }
                    }
                }
                void SetActions(InputAssetsManager manager)
                {
                    GameObject Origin = GameObject.Find("XR Origin");
                    if (Origin == null)
                    {
                        return;
                    }
                    var XRgo = Origin.GetComponent<Unity.XR.CoreUtils.XROrigin>();
                    var inputMgr = XRgo.gameObject.GetComponent<InputActionManager>();
                    if (inputMgr == null)
                    {
                        inputMgr = XRgo.gameObject.AddComponent<InputActionManager>();
                        inputMgr.actionAssets = new List<InputActionAsset>();
                        inputMgr.actionAssets.Add(manager.inputAsset);
                    }

                    GameObject cameraTrack = GameObject.Find("XR Origin/Camera Offset/Main Camera");
                    var com = cameraTrack.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                    com.positionInput = new InputActionProperty(manager.GetInputActionReference("XRI Head/Position"));
                    com.rotationInput = new InputActionProperty(manager.GetInputActionReference("XRI Head/Rotation"));


                    GameObject leftHand = GameObject.Find("XR Origin/Camera Offset/LeftHand Controller");
                    GameObject rightHand = GameObject.Find("XR Origin/Camera Offset/RightHand Controller");
                    if (leftHand == null || rightHand == null)
                    {
                        return;
                    }

                    var leftController = leftHand.GetComponent<ActionBasedController>();
                    var rightController = rightHand.GetComponent<ActionBasedController>();

                    List<string> applyActionName = new List<string>()
                    {

                    };
                    string RootName = "XRI LeftHand";

                    leftController.enableInputTracking = true;
                    leftController.positionAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Position"));
                    leftController.rotationAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Rotation"));
                    leftController.trackingStateAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Tracking State"));

                    leftController.enableInputActions = true;
                    leftController.selectAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Select"));
                    leftController.selectActionValue = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Select Value"));
                    leftController.activateAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Activate"));
                    leftController.activateActionValue = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Activate Value"));

                    leftController.uiPressAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/UI Press"));
                    leftController.uiPressActionValue = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/UI Press Value"));
                    leftController.hapticDeviceAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Haptic Device"));
                    leftController.rotateAnchorAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Rotate Anchor"));
                    leftController.translateAnchorAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Translate Anchor"));


                    RootName = "XRI RightHand";
                    rightController.enableInputTracking = true;
                    rightController.positionAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Position"));
                    rightController.rotationAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Rotation"));
                    rightController.trackingStateAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Tracking State"));

                    rightController.enableInputActions = true;
                    rightController.selectAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Select"));
                    rightController.selectActionValue = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Select Value"));
                    rightController.activateAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Activate"));
                    rightController.activateActionValue = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Activate Value"));

                    rightController.uiPressAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/UI Press"));
                    rightController.uiPressActionValue = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/UI Press Value"));
                    rightController.hapticDeviceAction = new InputActionProperty(manager.GetInputActionReference(RootName + "/Haptic Device"));
                    rightController.rotateAnchorAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Rotate Anchor"));
                    rightController.translateAnchorAction = new InputActionProperty(manager.GetInputActionReference(RootName + " Interaction/Translate Anchor"));

                }

                private static InputActionReference[] GetAllAssetReferencesFromAssetDatabase(InputActionAsset actions)
                {
                    if (actions == null)
                        return null;

                    var path = AssetDatabase.GetAssetPath(actions);
                    var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                    List<InputActionReference> result = new List<InputActionReference>();
                    foreach (var item in assets)
                    {
                        if (item is InputActionReference)
                            result.Add(item as InputActionReference);
                    }
                    return result.ToArray();
                }
            }

        }
    }
}
//#endif

