/*******************************************************************************
Copyright © 2015-2022 PICO Technology Co., Ltd.All rights reserved.  

NOTICE：All information contained herein is, and remains the property of 
PICO Technology Co., Ltd. The intellectual and technical concepts 
contained hererin are proprietary to PICO Technology Co., Ltd. and may be 
covered by patents, patents in process, and are protected by trade secret or 
copyright law. Dissemination of this information or reproduction of this 
material is strictly forbidden unless prior written permission is obtained from
PICO Technology Co., Ltd. 
*******************************************************************************/

using System.Runtime.InteropServices;

namespace Unity.XR.PICO.LivePreview
{
    public static class PXR_Plugin
    {
        private const string PXR_PLATFORM_DLL = "PxrLivePreview";

        [DllImport(PXR_PLATFORM_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pxr_SetSRPState(bool value);

        public static class System
        {
            public static void UPxr_PTSetSRPState(bool value)
            {
#if UNITY_EDITOR
                Pxr_SetSRPState(value);
#endif
            }
            
        }
        
    }
}