using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            /// <summary>
            /// Used to control body tracking scene UI logic
            /// </summary>
            public class BodyTrackingDemoUIManager : MonoBehaviour
            {
                // singleton instance.
                public static BodyTrackingDemoUIManager Instance
                {
                    get
                    {
                        if (_instance == null)
                        {
                            _instance = GameObject.Find("BodyTrackingDemoUIManager").GetComponent<BodyTrackingDemoUIManager>();
                            _instance.InitializeUI();
                        }
                        return _instance;
                    }
                }

                public GameObject startMenu;
                public Text showMode;
                
                private Button _fitGroundButton;
                private Button _cancelButton;
                private Button _fullBodyModeButton;
                private Button _lowerBodyModeButton;

                private BodyTrackingDeviceInputReader _bodyTrackingDeviceInputReader;
                private bool _canCalibrate = true;

                private static BodyTrackingDemoUIManager _instance;

                private bool _fullBodyMode = true;
                public void ResetBodyTrackingDeviceInputReader(BodyTrackingDeviceInputReader bodyTrackingDeviceInputReader)
                {
                    _bodyTrackingDeviceInputReader = bodyTrackingDeviceInputReader;
                    _bodyTrackingDeviceInputReader.SetSwiftMode(_fullBodyMode ? 1 : 0);

                    OpenCalibrationUI();
                }

                /// <summary>
                /// On click swift mode buttons, control the logic of FullBodyMode and LowerBodyMode buttons.
                /// </summary>
                /// <param name="swiftMode"> 0: lower body(only legs), 1: full body </param>
                public void OnSwiftModeButtons(int swiftMode)
                {
                    _fullBodyMode = !_fullBodyMode;
                    showMode.text = _fullBodyMode ? "FullBody" : "LowerBody";
                    _bodyTrackingDeviceInputReader.SetSwiftMode(_fullBodyMode?1:0);
                }

                /// <summary>
                /// On click Start button
                /// </summary>
                public void OnStartButton()
                {
                    startMenu.SetActive(false);

                    _bodyTrackingDeviceInputReader.SetSwiftMode(_fullBodyMode?1:0);
                    _bodyTrackingDeviceInputReader.CalibrateSwiftTracker();
                }

                /// <summary>
                /// On click Fit Ground button
                /// </summary>
                public void OnFitGroundButton()
                {
                    startMenu.SetActive(false);

                    _bodyTrackingDeviceInputReader.FitGround();
                }

                /// <summary>
                /// On click Cancel button
                /// </summary>
                public void OnCancelButton()
                {
                    startMenu.SetActive(false);
                }

                // Update is called once per frame
                void Update()
                {
                    if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bool right_B) && right_B)
                    {
                        // Press "B" button to open the calibration UI
                        if (_canCalibrate)
                        {
                            _canCalibrate = false;
                            OpenCalibrationUI();
                        }
                    }
                    else
                    {
                        _canCalibrate = true;
                    }
                }

                private BodyTrackingDemoUIManager()
                {
                }

                private void InitializeUI()
                {
                    if (startMenu == null)
                    {
                        Debug.LogError("[BodyTrackingInput] Please check the body tracking menu reference!");
                        return;
                    }

                    _fitGroundButton = startMenu.transform.Find("FitGroundButton").GetComponent<Button>();
                    _cancelButton = startMenu.transform.Find("CancelButton").GetComponent<Button>();
                    _fullBodyModeButton = startMenu.transform.Find("FullBodyModeButton").GetComponent<Button>();
                    _lowerBodyModeButton = startMenu.transform.Find("LowerBodyModeButton").GetComponent<Button>();

                    if (_fitGroundButton == null || _cancelButton == null || _fullBodyModeButton == null || _lowerBodyModeButton == null)
                    {
                        Debug.LogError("[BodyTrackingInput] Please check the 'BodyTrackingDemoUIManager' hierarchy!");
                    }
                }

                private void OpenCalibrationUI()
                {
                    startMenu.SetActive(true);

                    // if the swift is not calibrated, the player cannot continue to play, must restart to calibrate
                    if (_bodyTrackingDeviceInputReader.IsCalibrated)
                    {
                        _fitGroundButton.interactable = true;
                        _cancelButton.interactable = true;
                    }
                    else
                    {
                        _fitGroundButton.interactable = false;
                        _cancelButton.interactable = false;
                    }
                }
            }
        }
    }
}