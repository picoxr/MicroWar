using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackingArea : ETObject
{
    public GameObject infoCanvas;
    public override void IsFocused()
    {
        base.IsFocused();
        
        if (infoCanvas != null)
            infoCanvas.SetActive(isFocused);
    }

    public override void UnFocused()
    {
        base.UnFocused();
        
        if (infoCanvas != null)
            infoCanvas.SetActive(isFocused);
    }
}
