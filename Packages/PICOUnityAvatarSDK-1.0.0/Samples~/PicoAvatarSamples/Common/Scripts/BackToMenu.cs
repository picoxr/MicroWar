using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pico.Avatar.Sample
{
    public class BackToMenu : MonoBehaviour
    {
        public void OnClickBackToMenu()
        {
            var count = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                var sceneName = Path.GetFileNameWithoutExtension(path);
                if (sceneName.Contains("MenuEntry"))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                    found = true;
                }
            }

            if (!found)
            {
                Debug.LogError("Please add MenuEntry.unity into BuildSettings!");
            }
        }
    }

}
