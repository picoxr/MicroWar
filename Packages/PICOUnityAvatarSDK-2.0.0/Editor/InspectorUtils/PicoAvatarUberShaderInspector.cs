using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class PicoAvatarUberShaderInspector : BaseShaderGUI
    {
        // Properties
        private LitGUI.LitProperties litProperties;
        private MaterialProperty ShaderModeProperties;
        private static string ShaderModeKey = "PAV_LITMODE";
        static class ShaderModeKeyWord{
            public static string PAV_LITMODE_URP_STANDARD = ShaderModeKey + "_URP_STANDARD";
            public static string PAV_LITMODE_UBERSTANDARD = ShaderModeKey + "_UBERSTANDARD";
            public static string PAV_LITMODE_UBERSKIN = ShaderModeKey + "_UBERSKIN";

            public static string PAV_LITMODE_UBERHAIR = ShaderModeKey + "_UBERHAIR";

            public static string PAV_LITMODE_UBERFABRIC = ShaderModeKey + "_UBERFABRIC";

            public static string PAV_LITMODE_UBEREYE = ShaderModeKey + "_UBEREYE";
            public static string PAV_LITMODE_NPRSTANDARD = ShaderModeKey + "_NPRSTANDARD";
            public static string PAV_LITMODE_NPRSKIN = ShaderModeKey + "_NPRSKIN";
            public static string PAV_LITMODE_NPRHAIR = ShaderModeKey + "_NPRHAIR";
            public static string PAV_LITMODE_NPRFABRIC = ShaderModeKey + "_NPRFABRIC";
            public static string PAV_LITMODE_NPREYE = ShaderModeKey + "_NPREYE";
        }
        public enum ShaderMode
        {
            URP_STANDARD = 0, UBERSTANDARD, UBERSKIN, UBERHAIR, UBERFABRIC, UBEREYE, NPRSTANDARD, NPRSKIN, NPRHAIR, NPRFABRIC, NPREYE
        }

        

        public static GUIContent shaderModeText = new GUIContent("Shader Mode",
                "Select a ShaderMode");

        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            ShaderModeProperties = BaseShaderGUI.FindProperty("PAV_LITMODE", properties, false);
        }

        static string PAV_LIT_FULL_PBR = "PAV_LIT_FULL_PBR";
        static string PAV_VERTEX_FROM_BUFFER = "PAV_VERTEX_FROM_BUFFER";
        static string PAV_MERGED_TEXTURE = "PAV_MERGED_TEXTURE";

        public static void SetPavKeyWorld(Material material, string keyW, bool pIsEnable)
        {
            if(pIsEnable)
            {
                material.EnableKeyword(keyW);
            }
            else
            {
                 material.DisableKeyword(keyW);
            }
        }
        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
            bool isPAV_LIT_FULL_PBR = material.IsKeywordEnabled(PAV_LIT_FULL_PBR);
            bool isPAV_VERTEX_FROM_BUFFER = material.IsKeywordEnabled(PAV_VERTEX_FROM_BUFFER);
            bool isPAV_MERGED_TEXTURE = material.IsKeywordEnabled(PAV_MERGED_TEXTURE);
            // if(!isPAV_MERGED_TEXTURE || !Application.isPlaying)
            if(!isPAV_MERGED_TEXTURE)
            {
                 BaseShaderGUI.SetMaterialKeywords(material, LitGUI.SetMaterialKeywords);
            }
            SetPavKeyWorld(material, PAV_LIT_FULL_PBR, isPAV_LIT_FULL_PBR);
            SetPavKeyWorld(material, PAV_VERTEX_FROM_BUFFER, isPAV_VERTEX_FROM_BUFFER);
            SetPavKeyWorld(material, PAV_MERGED_TEXTURE, isPAV_MERGED_TEXTURE);
            // // Setup blending - consistent across all Universal RP shaders
            // SetupMaterialBlendMode(material);
            // // Receive Shadows
            // if(material.HasProperty("_ReceiveShadows"))
            //     CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);
            // // Emission
            // if (material.HasProperty("_EmissionColor"))
            //     MaterialEditor.FixupEmissiveFlag(material);
            // bool shouldEmissionBeEnabled =
            //     (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            // if (material.HasProperty("_EmissionEnabled") && !shouldEmissionBeEnabled)
            //     shouldEmissionBeEnabled = material.GetFloat("_EmissionEnabled") >= 0.5f;
            // CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
            // // Normal Map
            // if(material.HasProperty("_BumpMap"))
            //     CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            // LitGUI.SetMaterialKeywords(material);
            ShaderMode mode = (ShaderMode)material.GetFloat(ShaderModeKey);
            // material.SetFloat(ShaderModeKey, (float)mode);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_URP_STANDARD ,mode == ShaderMode.URP_STANDARD);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_UBERSTANDARD ,mode == ShaderMode.UBERSTANDARD);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_UBERSKIN ,mode == ShaderMode.UBERSKIN);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_UBERHAIR ,mode == ShaderMode.UBERHAIR);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_UBERFABRIC ,mode == ShaderMode.UBERFABRIC);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_UBEREYE ,mode == ShaderMode.UBEREYE);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_NPRSTANDARD ,mode == ShaderMode.NPRSTANDARD);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_NPRSKIN ,mode == ShaderMode.NPRSKIN);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_NPRHAIR ,mode == ShaderMode.NPRHAIR);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_NPRFABRIC ,mode == ShaderMode.NPRFABRIC);
            CoreUtils.SetKeyword(material, ShaderModeKeyWord.PAV_LITMODE_NPREYE ,mode == ShaderMode.NPREYE);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
             EditorGUI.BeginChangeCheck();
            if (ShaderModeProperties != null)
            {
                // DoPopup(shaderModeText, ShaderModeProperties, Enum.GetNames(typeof(ShaderMode)));
                materialEditor.ShaderProperty(ShaderModeProperties, shaderModeText);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // MaterialChanged(material);
                foreach (var obj in ShaderModeProperties.targets)
                    MaterialChanged((Material)obj);
            }

            EditorGUI.BeginChangeCheck();
            if (litProperties.workflowMode != null)
            {
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, Enum.GetNames(typeof(LitGUI.WorkflowMode)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
                if(EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }

            MaterialChanged(material);
        }
    }
}
