using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pico.Avatar.Sample
{
    public class JumpSceneDialog : MonoBehaviour
    {
        public Text Content;

        private string ContentTemplate;
        private string SceneName;
        private Action<string> continueCallback;
    
        public void OnClickCallBack(string sceneName, Action<string> callback)
        {
            SceneName = sceneName;
            if (ContentTemplate == null)
            {
                ContentTemplate = Content.text;
            }
       
            continueCallback = callback;
            Content.text = string.Format(ContentTemplate, SceneName);
        }

        public void OnContinue()
        {
            continueCallback?.Invoke(SceneName);
        }
    
        public void OnCancel()
        {
            gameObject.SetActive(false);
        }
    }
}


