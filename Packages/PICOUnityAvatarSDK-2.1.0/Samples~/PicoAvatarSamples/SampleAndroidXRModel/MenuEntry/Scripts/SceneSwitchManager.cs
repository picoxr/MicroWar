using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pico.Avatar.Sample
{
    public class SceneSwitchManager : MonoBehaviour
    {
        public GameObject buttonItem;
        public Transform menuRoot;
        public SceneDescription sceneDescConfigs;
        
        public Text sceneTitle;
        public Text sceneDesc;
        public JumpSceneDialog jumpDialog;
        
        public List<string> includeList = new List<string>();

        private Button _currSelectButton;
        private string _currSelectSceneName;
        // Start is called before the first frame update
        void Start()
        {
            int n = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < n; ++i)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                var sceneName = Path.GetFileNameWithoutExtension(path);
                if (!includeList.Contains(sceneName))
                    continue;
                
                var sceneIndex = i;
                var go = Instantiate(buttonItem, menuRoot);
                var sceneBtn = go.GetComponent<Button>();
                if (sceneBtn != null)
                {
                    sceneBtn.onClick.AddListener(() =>
                    {
                        OnChangeUI(sceneBtn, sceneName);
                        //LoadScene(sceneName);
                    });
                    var text = sceneBtn.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text =  sceneName;
                    }
                }
            }
        }

        void OnChangeUI(Button btn, string sceneName)
        {
            if (_currSelectButton != null && btn.transform.GetInstanceID() 
                == _currSelectButton.transform.GetInstanceID())
                return;
            var color = btn.colors;
            
            if (_currSelectButton != null)
            {
                _currSelectButton.colors = color;
            }
            
            _currSelectButton = btn;
            color.colorMultiplier = 5;
            btn.colors = color;

            _currSelectSceneName = sceneName;
            sceneTitle.text = sceneName;
            sceneDesc.text = GetDescription(sceneName).Replace("\\n", "\n");
        }

        private string GetDescription(string sceneName)
        {
            foreach (var config in sceneDescConfigs.sceneDescGroup)
            {
                if (config.sceneName.Equals(sceneName))
                {
                    return config.sceneDesc;
                }
            }
            return string.Empty;
        }
        public void LoadScene()
        {
            if (string.IsNullOrEmpty(_currSelectSceneName))
                return;
            jumpDialog.gameObject.SetActive(true);
            jumpDialog.OnClickCallBack(_currSelectSceneName, desc =>
            {
                Debug.Log("Load Scene: " + desc);
                UnityEngine.SceneManagement.SceneManager.LoadScene(desc, LoadSceneMode.Single);
            });
        }
    }

}

