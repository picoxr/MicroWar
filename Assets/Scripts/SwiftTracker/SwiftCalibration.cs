using MicroWar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwiftCalibration : MonoBehaviour
{
    public Button swiftCalibrationButton;

    private void Start()
    {
        // Add a listener to the button click event
        swiftCalibrationButton.onClick.AddListener(CallSwiftTrackerFunction);
    }

    private void CallSwiftTrackerFunction()
    {
        // Call the function through the singleton instance
        SwiftTrackerManager.Instance.OpenSwiftCalibrationApp();
    }
}
