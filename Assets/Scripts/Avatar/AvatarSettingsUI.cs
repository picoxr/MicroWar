using MicroWar.Avatar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSettingsUI : MonoBehaviour
{

    public void OnOpenAvatarEditorClick()
    {
        AvatarManager.Instance.OpenAvatarEditor();
    }
    
}
