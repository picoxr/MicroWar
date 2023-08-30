using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MicroWar.Platform
{
    public abstract class ControllerBase : MonoBehaviour
    {
        protected PlatformServiceManager platformServiceManager 
        { 
            get {return PlatformServiceManager.Instance;}
        }
        protected virtual void Awake()
        {
            RegisterController();
        }
        internal abstract void DelayInit();
        internal abstract void RegisterController();
    }

}
