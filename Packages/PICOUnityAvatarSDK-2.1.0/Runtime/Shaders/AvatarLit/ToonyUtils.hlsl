half3 CalculateRamp(half halfLambert, half bands, half threshold, half smooth, half bandsSmoothing)
{
    half x = smoothstep(threshold - smooth, threshold + smooth, halfLambert);
    half bandsSmooth = bandsSmoothing * 0.5;
    half3 ramp = saturate((smoothstep(0.5 - bandsSmooth, 0.5 + bandsSmooth, frac(x * bands)) + floor(x * bands)) / bands).xxx;
    return ramp;
}

half3 RimLighting(half NdotV, half3 rimColor, half rimPower, half rimMin, half rimMax)
{
    half rim = 1.0f - saturate(NdotV);
    rim = pow(rim, rimPower);
    rim = smoothstep(rimMin, rimMax, rim);
    return rim * rimColor;
}

half3 SampleMatCap(TEXTURE2D_PARAM(matCapMap, sampler_matCapMap), half3 matCapColor, half3 normalWS)
{
    half3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
    half2 matCapUV = normalVS.xy * 0.5 + 0.5;
    half3 matCap = SAMPLE_TEXTURE2D(matCapMap, sampler_matCapMap, matCapUV).rgb;
    return matCapColor * matCap;
}