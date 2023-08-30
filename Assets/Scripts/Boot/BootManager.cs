using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MicroWar.Platform;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
public class BootManager : MonoSingleton<BootManager>
{
    public bool ErrorNotificationFlag { get; private set; }

    private int RetryGameInitTimes = 3; //Retry 3 times if game not properly initialized. TODO: Game service might need re-initialize if the app is interrupted.
    private int retryTimes = 0;

    private float startTime;
    private float MinLoadingTime = 3f;
    private float maxNetRequestTime = 9f; //Give up waitting for response after 5 seconds.
    private bool IsInitializeFinished = false;

    private bool IsFirstLaunch = true;

    private AsyncOperation asyncLoadSceneOp;
    private void Awake()
    {
        // Allow the application to keep running when the headset is removed. 
        // This prevents various hangs & issues that could occur by interupting the game at inopportune moments.
        Application.runInBackground = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Bind notification
        startTime = Time.realtimeSinceStartup;
#if !UNITY_EDITOR
        InputDevices.GetDeviceAtXRNode(XRNode.Head).subsystem.TryRecenter();
#endif
        StartCoroutine(Load());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Load()
    {
        //On start load. Try initialize platform services.
        PlatformServiceManager.Instance.OnPlatformInitializeStatusChange = OnPlatformInitializeStatusChange;
        PlatformServiceManager.Instance.InitPlatformServices();
        //Start loading loading scene asynchronously
        IsFirstLaunch = true;
        asyncLoadSceneOp = LoadSceneAsync("Main", LoadSceneMode.Single);
        asyncLoadSceneOp.allowSceneActivation = false; //Do not switch scene immidiatly
        //Platform Init timeout
        yield return new WaitUntil(CheckPlatformInitTimeout);
        if (!IsInitializeFinished)//Give up
        {
            Debug.LogWarning("Give up initialize platform service!");
            ErrorNotificationFlag = true;
        }
        //Check for loading conditions.
        yield return new WaitUntil(CheckLoadingConditions); 
        asyncLoadSceneOp.allowSceneActivation = true;
        //On finish load
        IsFirstLaunch = false;
    }
    private void OnPlatformInitializeStatusChange(ServiceInitializeStatus status)
    {
        if (!IsFirstLaunch)
            return; //Return if it's not first launch

        switch (status)
        {
            case ServiceInitializeStatus.Unknown:
            case ServiceInitializeStatus.Initializing:
                IsInitializeFinished = false;
                break;
            case ServiceInitializeStatus.initialized:
                ErrorNotificationFlag = false;
                IsInitializeFinished = true;
                break;
            case ServiceInitializeStatus.Failed:
                var shouldRetry = RetryInitGameServiceInBoot(); //Retry
                if (!shouldRetry) //Give up initialize game service.
                {
                    ErrorNotificationFlag = true;
                    IsInitializeFinished = true;
                }
                break;
            default:
                break;
        }
    }

    private bool CheckPlatformInitTimeout()
    {
        if (Time.realtimeSinceStartup - startTime <= maxNetRequestTime)
        {
            if (IsInitializeFinished)
            {
                Debug.Log("Finished Initialize!");
                return true;
            }
            return false;
        }
        else
        {
            return true;
        }
    }
    private bool CheckLoadingConditions()
    {
        //Check for min time out
        if (Time.realtimeSinceStartup-startTime >= MinLoadingTime && asyncLoadSceneOp.progress>=0.9f)
            return true;
        else
            return false;
    }

    private bool RetryInitGameServiceInBoot()
    {
        retryTimes++;
        if(retryTimes<=RetryGameInitTimes)
        {
            Debug.Log($"Retry game init Times: {retryTimes}");
            PlatformServiceManager.Instance.InitPlatformServices();
            return true;
        }
        return false;
    }

    private AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
    {
        AsyncOperation SceneLoadOp = SceneManager.LoadSceneAsync(sceneName, mode);
        return SceneLoadOp;
    }

}
