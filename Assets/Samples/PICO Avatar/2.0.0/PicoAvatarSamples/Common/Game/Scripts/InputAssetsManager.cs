using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class InputAssetsManager : MonoBehaviour
            {
                [SerializeField]
                public InputActionAsset inputAsset;
                [SerializeField]
                public List<string> actionNameList = new List<string>();
                [SerializeField]
                public List<InputActionReference> actionsRefList = new List<InputActionReference>();

                public void AddInputActionReference(InputActionReference data)
                {
                    if (data == null)
                        return;
                    var org = GetInputActionReference(data.name);
                    if (org == data)
                        return;

                    actionNameList.Add(data.name);
                    actionsRefList.Add(data);
                }
                public InputActionReference GetInputActionReference(string actionName)
                {
                    if (actionNameList.Count != actionsRefList.Count)
                        return null;
                    int index = actionNameList.IndexOf(actionName);
                    if (index >= 0)
                        return actionsRefList[index];
                    return null;
                }
                public int Count
                {
                    get
                    {
                        return actionsRefList.Count;
                    }
                }
                public void Clear()
                {
                    actionNameList.Clear();
                    actionsRefList.Clear();
                }
            }
        }
    }
}