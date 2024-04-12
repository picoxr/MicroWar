using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pico
{
    namespace Avatar
    {
        public class URPShaderInspector : ShaderGUI
        {
            static Dictionary<int, float> _colorRegion1MinXValues = new Dictionary<int, float>();

            public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
            {
                var mat = materialEditor.target as Material;
                int instID = mat.GetInstanceID();
                bool colorRegionBaked = mat.IsKeywordEnabled("PAV_COLOR_REGION_BAKED");

                bool usingAlbedoHue = false;
                for (int i = 0; i < properties.Length; ++i)
                {
                    var prop = properties[i];
                    if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) == 0)
                    {
                        const float MaxSVA = 5.0f;

                        if (prop.displayName == "UsingAlbedoHue")
                        {
                            if (colorRegionBaked)
                            {
                                EditorGUILayout.Toggle("ColorRegionBaked", true);
                            }
                            else
                            {
                                EditorGUI.BeginChangeCheck();
                                bool enable = EditorGUILayout.Toggle(prop.displayName, prop.floatValue > 0);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    prop.floatValue = enable ? 1.0f : 0.0f;
                                }
                                usingAlbedoHue = prop.floatValue > 0;
                            }
                        }
                        else if (prop.displayName.StartsWith("ColorRegion") && prop.type == MaterialProperty.PropType.Vector)
                        {
                            if (!colorRegionBaked)
                            {
                                Vector4 vec = prop.vectorValue;
                                EditorGUILayout.LabelField(prop.displayName);
                                EditorGUI.indentLevel += 1;
                                vec.x = EditorGUILayout.Slider("H", vec.x, 0, 1);
                                vec.y = EditorGUILayout.Slider("S", vec.y, 0, MaxSVA);
                                vec.z = EditorGUILayout.Slider("V", vec.z, 0, MaxSVA);
                                vec.w = EditorGUILayout.Slider("A", vec.w, 0, MaxSVA);
                                EditorGUI.indentLevel -= 1;
                                prop.vectorValue = vec;
                            }
                        }
                        else if (prop.displayName == "ColorRegionMap")
                        {
                            if (!colorRegionBaked)
                            {
                                materialEditor.ShaderProperty(prop, prop.displayName);
                            }
                        }
                        else
                        {
                            materialEditor.ShaderProperty(prop, prop.displayName);
                        }

                        if (prop.displayName == "ColorRegion1")
                        {
                            if (!colorRegionBaked && usingAlbedoHue)
                            {
                                float colorRegion1MinX = 0;
                                if (!_colorRegion1MinXValues.TryGetValue(instID, out colorRegion1MinX))
                                {
                                    _colorRegion1MinXValues.Add(instID, colorRegion1MinX);
                                }
                                colorRegion1MinX = EditorGUILayout.FloatField("ColorRegion1MinX", colorRegion1MinX);
                                _colorRegion1MinXValues[instID] = colorRegion1MinX;
                            }
                        }

                        if (prop.displayName.StartsWith("ColorRegion") && prop.type == MaterialProperty.PropType.Vector)
                        {
                            if (!colorRegionBaked)
                            {
                                Vector4 vec = prop.vectorValue;
                                if (usingAlbedoHue)
                                {
                                    vec.x -= _colorRegion1MinXValues[instID];
                                }
                                vec.x = vec.x % 1.0f;
                                if (vec.x < 0.0f)
                                {
                                    vec.x += 1.0f;
                                }
                                Vector4 hsv = new Vector4(
                                    vec.x,
                                    Mathf.Clamp01(vec.y / MaxSVA),
                                    Mathf.Clamp01(vec.z / MaxSVA),
                                    Mathf.Clamp01(vec.w / MaxSVA));
                                Vector4 rgb = HsvToRGB(hsv);
                                EditorGUILayout.BeginHorizontal();
                                string rgbText = RGBToStr(rgb);
                                EditorGUI.BeginChangeCheck();
                                rgbText = EditorGUILayout.TextField(prop.displayName + "_RGBA", rgbText);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    if (StrToRGB(rgbText, ref rgb))
                                    {
                                        ApplyRGB(prop, rgb, MaxSVA, usingAlbedoHue, usingAlbedoHue ? _colorRegion1MinXValues[instID] : 0);
                                    }
                                }
                                EditorGUI.BeginChangeCheck();
                                rgb = EditorGUILayout.ColorField(rgb, GUILayout.Width(50));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    ApplyRGB(prop, rgb, MaxSVA, usingAlbedoHue, usingAlbedoHue ? _colorRegion1MinXValues[instID] : 0);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
                materialEditor.RenderQueueField();
            }

            void ApplyRGB(MaterialProperty prop, Vector4 rgbNew, float MaxSVA, bool usingAlbedoHue, float colorRegion1MinX)
            {
                Vector4 hsvNew = RGBToHsv(rgbNew);
                hsvNew.y *= MaxSVA;
                hsvNew.z *= MaxSVA;
                hsvNew.w *= MaxSVA;
                if (usingAlbedoHue)
                {
                    hsvNew.x += colorRegion1MinX;
                }
                hsvNew.x = hsvNew.x % 1.0f;
                prop.vectorValue = hsvNew;
            }

            Vector4 RGBToHsv(Vector4 rgb)
            {
                float r = rgb.x;
                float g = rgb.y;
                float b = rgb.z;
                float a = rgb.w;
                float maxc = Mathf.Max(r, g, b);
                float minc = Mathf.Min(r, g, b);
                float rangec = (maxc - minc);
                float v = maxc;
                if (minc == maxc)
                    return new Vector4(0.0f, 0.0f, v, a);
                float s = rangec / maxc;
                float rc = (maxc - r) / rangec;
                float gc = (maxc - g) / rangec;
                float bc = (maxc - b) / rangec;
                float h = 0;
                if (r == maxc)
                    h = bc - gc;
                else if (g == maxc)
                    h = 2.0f + rc - bc;
                else
                    h = 4.0f + gc - rc;
                h = (h / 6.0f) % 1.0f;
                if (h < 0.0f) h += 1.0f;
                return new Vector4(h, s, v, a);
            }

            bool StrToRGB(string str, ref Vector4 rgb)
            {
                str = str.ToUpper();
                //
                var outRGBA = new int[] { 0, 0, 0, 0 };
                int curPos = 0;
                int curChannel = 0;
                try
                {
                    while (curPos < str.Length && curChannel < 4)
                    {
                        // skip leading #
                        if (curPos == 0 && str[curPos] == '#')
                        {
                            ++curPos;
                            continue;
                        }

                        int remainLen = str.Length - curPos;
                        if (remainLen == 1)
                        {
                            outRGBA[curChannel++] = System.Convert.ToInt32(str.Substring(curPos, 1), 16);
                            curPos += 1;
                            break;
                        }
                        else
                        {
                            outRGBA[curChannel++] = System.Convert.ToInt32(str.Substring(curPos, 2), 16);
                            curPos += 2;
                        }
                    }
                    if (curPos < str.Length)
                    {
                        str = str.Substring(0, curPos);
                    }
                }
                catch (System.Exception e)
                {
                    // not valid color.
                    return false;
                }

                rgb.x = outRGBA[0] / 255.0f;
                rgb.y = outRGBA[1] / 255.0f;
                rgb.z = outRGBA[2] / 255.0f;
                rgb.w = outRGBA[3] / 255.0f;
                return true;

                if (str.Length == 9 && str.StartsWith("#"))
                {
                    bool validChar = true;
                    for (int i = 1; i < 9; ++i)
                    {
                        if (!((str[i] >= '0' && str[i] <= '9') ||
                            (str[i] >= 'A' && str[i] <= 'F')))
                        {
                            validChar = false;
                            break;
                        }
                    }
                    if (validChar)
                    {
                        int r = System.Convert.ToInt32(str.Substring(1, 2), 16);
                        int g = System.Convert.ToInt32(str.Substring(3, 2), 16);
                        int b = System.Convert.ToInt32(str.Substring(5, 2), 16);
                        int a = System.Convert.ToInt32(str.Substring(7, 2), 16);
                        rgb = new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
                        return true;
                    }
                }
                return false;
            }

            string RGBToStr(Vector4 rgb)
            {
                int r = Mathf.CeilToInt(rgb.x * 255);
                int g = Mathf.CeilToInt(rgb.y * 255);
                int b = Mathf.CeilToInt(rgb.z * 255);
                int a = Mathf.CeilToInt(rgb.w * 255);
                return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
            }

            Vector4 HsvToRGB(Vector4 hsv)
            {
                float h = hsv.x;
                float s = hsv.y;
                float v = hsv.z;
                float a = hsv.w;
                if (s == 0)
                    return new Vector4(v, v, v, a);
                int i = (int) (h * 6.0f);
                float f = (h * 6.0f) - i;
                float p = v * (1.0f - s);
                float q = v * (1.0f - s * f);
                float t = v * (1.0f - s * (1.0f - f));
                i = i % 6;
                if (i == 0)
                    return new Vector4(v, t, p, a);
                if (i == 1)
                    return new Vector4(q, v, p, a);
                if (i == 2)
                    return new Vector4(p, v, t, a);
                if (i == 3)
                    return new Vector4(p, q, v, a);
                if (i == 4)
                    return new Vector4(t, p, v, a);
                if (i == 5)
                    return new Vector4(v, p, q, a);
                return Vector4.zero;
            }
        }
    }
}
