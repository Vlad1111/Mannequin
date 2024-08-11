using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

class SsaoConfigurator
{
    private readonly object _ssaoSettings;
    private readonly FieldInfo _fIntensity;
    private readonly FieldInfo _fRadius;

    public SsaoConfigurator(ScriptableRendererFeature ssaoFeature)
    {
        //static ScriptableRendererFeature findRenderFeature(System.Type type)
        //{
        //    FieldInfo field = reflectField(typeof(ScriptableRenderer), "m_RendererFeatures");
        //    ScriptableRenderer renderer = UniversalRenderPipeline.asset.scriptableRenderer;
        //    var list = (List<ScriptableRendererFeature>)field.GetValue(renderer);
        //    foreach (ScriptableRendererFeature feature in list)
        //        if (feature.GetType() == type)
        //            return feature;
        //    throw new System.Exception($"Could not find instance of {type.AssemblyQualifiedName} in the renderer features list");
        //}

        static FieldInfo reflectField(System.Type type, string name) =>
            type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new System.Exception($"Could not reflect field [{type.AssemblyQualifiedName}].{name}");

        System.Type tSsaoFeature = System.Type.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion, Unity.RenderPipelines.Universal.Runtime", true);
        FieldInfo fSettings = reflectField(tSsaoFeature, "m_Settings");
        //ScriptableRendererFeature ssaoFeature = findRenderFeature(tSsaoFeature);
        _ssaoSettings = fSettings.GetValue(ssaoFeature) ?? throw new System.Exception("ssaoFeature.m_Settings was null");

        _fIntensity = reflectField(_ssaoSettings.GetType(), "Intensity");
        _fRadius = reflectField(_ssaoSettings.GetType(), "Radius");
    }

    public float Intensity
    {
        get => (float)_fIntensity.GetValue(_ssaoSettings);
        set => _fIntensity.SetValue(_ssaoSettings, value);
    }

    public float Radius
    {
        get => (float)_fRadius.GetValue(_ssaoSettings);
        set => _fRadius.SetValue(_ssaoSettings, value);
    }
}

public class SettingsMenuBehaviour : MonoBehaviour
{
    private const string RENDER_SCALE = "__RENDER_SCALE__";
    private const string AMBIENT_OCLUSION_INTENSITY = "__AMBIENT_OCLUSION_INTENSITY__";
    private const string AMBIENT_OCLUSION_RADIUS = "__AMBIENT_OCLUSION_RADIUS__";

    public UniversalRenderPipelineAsset URP_asset;
    public ScriptableRendererFeature ambientOcculsion;
    public Slider renderScale;
    private SsaoConfigurator ambientOcculsionSettings;
    public Slider ambientOcculsionIntensity;
    public Slider ambientOcculsionRadius;

    public void Start()
    {
        ambientOcculsionSettings = new SsaoConfigurator(ambientOcculsion);

        if (PlayerPrefs.HasKey(RENDER_SCALE))
            renderScale.value = PlayerPrefs.GetFloat(RENDER_SCALE);
        else
            renderScale.value = 1;

        if (PlayerPrefs.HasKey(AMBIENT_OCLUSION_INTENSITY))
            ambientOcculsionIntensity.value = PlayerPrefs.GetFloat(AMBIENT_OCLUSION_INTENSITY);
        else
            ambientOcculsionIntensity.value = ambientOcculsionSettings.Intensity;

        if (PlayerPrefs.HasKey(AMBIENT_OCLUSION_RADIUS))
            ambientOcculsionRadius.value = PlayerPrefs.GetFloat(AMBIENT_OCLUSION_RADIUS);
        else
            ambientOcculsionRadius.value = ambientOcculsionSettings.Radius;
    }

    public void OnRenderScaleChange()
    {
        URP_asset.renderScale = renderScale.value;

        PlayerPrefs.SetFloat(RENDER_SCALE, renderScale.value);
        Debug.Log(PlayerPrefs.GetFloat(RENDER_SCALE));
    }

    public void OnAmbinetOcclusionChange()
    {
        ambientOcculsionSettings.Intensity = ambientOcculsionIntensity.value;
        if (ambientOcculsionIntensity.value <= 0.1f)
            ambientOcculsion.SetActive(false);
        else ambientOcculsion.SetActive(true);

        PlayerPrefs.SetFloat(AMBIENT_OCLUSION_INTENSITY, ambientOcculsionIntensity.value);
    }

    public void OnAmbinetOcclusionRadiusChange()
    {
        ambientOcculsionSettings.Radius = ambientOcculsionRadius.value;

        PlayerPrefs.SetFloat(AMBIENT_OCLUSION_RADIUS, ambientOcculsionRadius.value);
    }
}
