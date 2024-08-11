using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PostProcessingValues
{
    public float BloomIntensity;
    public bool DepthOfField;
    public float DepthOfFieldDistance;
    public float AmbientOculsionIntensity;
    public float AmbientOculsionRadius;
}

public class CameraUiSettings : MonoBehaviour
{
    public Camera uiCamera;
    public Light cameraLight;
    public TMP_Dropdown projection;
    public Slider fovSlider;
    public Slider cameraLightSlider;
    [Space(20)]
    public VolumeProfile ppVolume;
    public Slider bloomSlider;
    public Toggle depthOfFieldOnOff;
    public Slider depthOfFieldFocusDistance;
    public Slider vignetteSlider;

    private void Start()
    {
        fovSlider.SetValueWithoutNotify(uiCamera.fieldOfView);
        projection.ClearOptions();
        projection.AddOptions(new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Perspective"),
            new TMP_Dropdown.OptionData("Orthographic")
        });
        cameraLightSlider.SetValueWithoutNotify(cameraLight.intensity);

        ppVolume.TryGet(out Bloom bloom);
        bloomSlider.SetValueWithoutNotify(bloom == null ? 0 : bloom.intensity.value);

        ppVolume.TryGet(out DepthOfField dof);
        depthOfFieldOnOff.SetIsOnWithoutNotify(dof.active);
        depthOfFieldFocusDistance.SetValueWithoutNotify(dof.focusDistance.value);

        ppVolume.TryGet(out Vignette vignette);
        vignetteSlider.SetValueWithoutNotify(vignette == null ? 0 : vignette.intensity.value);
    }

    public void OnFOVChange()
    {
        if (uiCamera.orthographic == false)
            uiCamera.fieldOfView = fovSlider.value;
        else
            uiCamera.orthographicSize = fovSlider.value;
    }

    public void OnProjectionChange()
    {
        if(projection.value == 1)
        {
            uiCamera.orthographic = true;
            fovSlider.SetValueWithoutNotify(20);
            fovSlider.minValue = 1.5f;
            fovSlider.maxValue = 40f;
            fovSlider.SetValueWithoutNotify(uiCamera.orthographicSize);
        }
        else
        {
            uiCamera.orthographic = false;
            fovSlider.SetValueWithoutNotify(20);
            fovSlider.minValue = 15f;
            fovSlider.maxValue = 150f;
            fovSlider.SetValueWithoutNotify(uiCamera.fieldOfView);
        }
    }

    public void OnCameraLightChange()
    {
        cameraLight.intensity = cameraLightSlider.value;
    }

    public void OnBloomChange()
    {
        ppVolume.TryGet(out Bloom bloom);
        if(bloom != null)
        {
            bloom.intensity.value = bloomSlider.value;
            if (bloomSlider.value <= 0.05f)
                bloom.active = false;
            else bloom.active = true;
        }
    }

    public void OnDepthOfFieldToggle()
    {
        ppVolume.TryGet(out DepthOfField dof);
        dof.active = depthOfFieldOnOff.isOn;
    }

    public void OnDepthOfFieldDistance()
    {
        ppVolume.TryGet(out DepthOfField dof);
        dof.focusDistance.value = depthOfFieldFocusDistance.value;
    }

    public void OnVignetteChange()
    {
        ppVolume.TryGet(out Vignette vignette);
        if (vignette != null)
        {
            vignette.intensity.value = vignetteSlider.value;
            if (vignetteSlider.value <= 0.05f)
                vignette.active = false;
            else vignette.active = true;
        }
    }
}
