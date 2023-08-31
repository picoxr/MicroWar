# Eye Tracking

Eye Tracking, a cutting-edge feature exclusive to PICO 4 PRO. This innovative input method opens the door for developers to explore new dimensions within the realm of VR experiences.

## MicroWar Hand Tracking Integration
In MicroWar, we use Eye Tracking to interact with the vehicle status UI. 

## Project Setup

To unlock the potential of Eye Tracking, follow these simple steps:

1. Ensure Eye Tracking is activated on PXR_Manager.\
![PXR_Manager](https://github.com/picoxr/MicroWar/blob/e4694b1f2fe5028c673b007c6731fa5b9e5a80aa/Documentation/Files/PXR_Manager_EyeTracking.png)  
2. Attach the `EyeTrackingManager` component within your scene.\
![EyeTrackingManager](https://github.com/picoxr/MicroWar/blob/e4694b1f2fe5028c673b007c6731fa5b9e5a80aa/Documentation/Files/EyeTrackingManager.png)
3. To enable UI interaction with Eye Tracking, attach the `EyeTrackingArea` to the tank holders within the `TankSelector` prefab.\
![EyeTrackingAera](https://github.com/picoxr/MicroWar/blob/e4694b1f2fe5028c673b007c6731fa5b9e5a80aa/Documentation/Files/TankSelectorPrefab.png)

If you wish to craft custom Eye Tracking interactions, create a new class inheriting from `EyeTrackingObject`.
## Key Scripts and Logic

### EyeTrackingManager.cs

At the heart of Eye Tracking lies the `EyeTrackingManager.cs` script. Here's a glimpse of its core functionality:

1. Initialization: In the `Start()` method, the script checks if Eye Tracking is supported on the current device.
2. Data Retrieval: Crucial Eye Tracking data such as head pose matrix, combined eye gaze vector, and combined eye gaze origin are extracted.
3. World Space Transformation: The retrieved eye gaze point and vector are translated to world space for accurate interactions.
4. Interaction Detection: The script employs SphereCast to determine if the player's gaze is fixed upon a game object.
      ```csharp
                PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);

            bool isSuccess = PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
            if (!isSuccess) return;

            PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);

            //Translate Eye Gaze point and vector to world space
            combineEyeGazeOrigin += combineEyeGazeOriginOffset;
            combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
            combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

            if (SpotLight != null)
            {
                SpotLight.transform.position = combineEyeGazeOriginInWorldSpace;
                SpotLight.transform.rotation = Quaternion.LookRotation(combineEyeGazeVectorInWorldSpace, Vector3.up);
            }

            GazeTargetControl(combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);
   ```
