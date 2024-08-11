using RuntimeHandle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditMenuBehaviour : MonoBehaviour
{
    private static EditMenuBehaviour _instance = null;
    public static EditMenuBehaviour Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<EditMenuBehaviour>(true);
            return _instance;
        }
        set => _instance = value;
    }
    private void Awake()
    {
        Instance = this;
    }
    private CharacterCreation characterCreation;
    public CharacterCreation CharacterCreation
    {
        get => characterCreation;
        set
        {
            characterCreation = value;
            ResetSliders();
        }
    }

    private Transform characterBoneSeleted;
    public Transform CharacterBoneSeleted
    {
        get => characterBoneSeleted;
        set
        {
            characterBoneSeleted = value;
            if (characterBoneSeleted != null)
            {
                boneSettings.gameObject.SetActive(true);
                boneNameText.text = characterBoneSeleted.name;
                selectionRestriction = characterBoneSeleted.GetComponent<HandleTypeRestiction>();
            }
            else
                boneSettings.gameObject.SetActive(false);

            if (curentType == HandleType.SCALE)
                linkScaleBoneSetting.gameObject.SetActive(true);
            else linkScaleBoneSetting.gameObject.SetActive(false);

            if (curentType == HandleType.ROTATION)
                rotateAngleBoneSetting.gameObject.SetActive(true);
            else rotateAngleBoneSetting.gameObject.SetActive(false);

            if (characterCreation != null && characterCreation.HasMirror(characterBoneSeleted))
                mirrorBoneSetting.gameObject.SetActive(true);
            else
                mirrorBoneSetting.gameObject.SetActive(false);
        }
    }
    private HandleTypeRestiction selectionRestriction = null;
    public RuntimeTransformHandle runtimeTransformHandle;
    public TextMeshProUGUI gizmoTypeText;
    public TextMeshProUGUI gizmoSpaceText;
    public Transform slidersParent;
    public Transform sliderPrefab;
    public TextMeshProUGUI boneNameText;
    public Transform boneSettings;
    public Toggle linkScaleBoneSetting;
    public Transform rotateAngleBoneSetting;
    public Button mirrorBoneSetting;
    private Slider boneSettingSlider;
    public Transform poseSpecificValuesParent;
    public HandleType curentType { get; private set; } = HandleType.POSITION;
    public HandleSpace curentSpace { get; private set; } = HandleSpace.LOCAL;

    private void Start()
    {
        ResetSliders();
        ChangeGizmosType(HandleType.POSITION);
    }

    private void FixedUpdate()
    {
        if (boneSettingSlider)
        {
            if (boneSettings.gameObject.activeSelf)
                OnBoneSettingChange(boneSettingSlider);
            else
            {
                boneSettingSlider.SetValueWithoutNotify(0);
                boneSettingSlider = null;
            }
        }
    }

    private void Update()
    {
        //Debug.Log(selectionRestriction + " " + selectionRestriction?.transform.name);
        if (selectionRestriction && selectionRestriction.types.Length > 0)
        {
            if (!selectionRestriction.types.Contains(runtimeTransformHandle.type))
                ChangeGizmosType(selectionRestriction.types[0]);
        }

        if(runtimeTransformHandle.type != curentType)
        {
            ChangeGizmosType(curentType);
        }

        //CharacterCreation.SetHandleLayer();
        //CharacterCreation.ApplyLayerToChildren();
    }

    public void SwitchGizmosType()
    {
        if (curentType == HandleType.POSITION)
        {
            curentType = HandleType.ROTATION;
            gizmoTypeText.text = "Rot.";
        }
        else if (curentType == HandleType.ROTATION)
        {
            curentType = HandleType.SCALE;
            gizmoTypeText.text = "Scale";
        }
        else
        {
            curentType = HandleType.POSITION;
            gizmoTypeText.text = "Move";
        }
        runtimeTransformHandle.type = curentType; 

        if (curentType == HandleType.SCALE)
            linkScaleBoneSetting.gameObject.SetActive(true);
        else linkScaleBoneSetting.gameObject.SetActive(false);

        if (curentType == HandleType.ROTATION)
            rotateAngleBoneSetting.gameObject.SetActive(true);
        else rotateAngleBoneSetting.gameObject.SetActive(false);
    }

    public void ChangeGizmosType(HandleType type)
    {
        curentType = type;
        if (curentType == HandleType.POSITION)
        {
            gizmoTypeText.text = "Move";
        }
        else if (curentType == HandleType.ROTATION)
        {
            gizmoTypeText.text = "Rot.";
        }
        else
        {
            gizmoTypeText.text = "Scale";
        }
        runtimeTransformHandle.type = curentType;

        if (curentType == HandleType.SCALE)
            linkScaleBoneSetting.gameObject.SetActive(true);
        else linkScaleBoneSetting.gameObject.SetActive(false);

        if (curentType == HandleType.ROTATION)
            rotateAngleBoneSetting.gameObject.SetActive(true);
        else rotateAngleBoneSetting.gameObject.SetActive(false);
    }

    public void SwitchGizmosSpace()
    {
        if (curentSpace == HandleSpace.LOCAL)
        {
            curentSpace = HandleSpace.WORLD;
            gizmoSpaceText.text = "World";
        }
        else
        {
            curentSpace = HandleSpace.LOCAL;
            gizmoSpaceText.text = "Local";
        }
        runtimeTransformHandle.space = curentSpace;
    }

    public void ResetSliders()
    {
        foreach (Transform child in slidersParent)
            Destroy(child.gameObject);
        if (characterCreation == null)
            return;

        Dictionary<string, float> setValues = characterCreation.sliderValues;

        if(characterCreation.sliderDatas != null)
        foreach (var slide in characterCreation.sliderDatas)
        {
            var sl = Instantiate(sliderPrefab, slidersParent);

            var text = sl.GetComponentInChildren<TextMeshProUGUI>();
            text.text = slide.name;

            var slider = sl.GetComponentInChildren<Slider>();
            slider.minValue = slide.marginValues.x;
            slider.maxValue = slide.marginValues.y;
            if(setValues.ContainsKey(slide.name))
                slider.SetValueWithoutNotify(setValues[slide.name]);
            slider.onValueChanged.AddListener((x) =>
            {
                ChangeSliderValue(slide.name, x);
            });

            var button = sl.GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                ChangeSliderValue(slide.name, 0);
                slider.SetValueWithoutNotify(0);
            });
        }

        _ = SetPoseSliders(poseSpecificValuesParent);
    }

    private bool SetPoseSliders(Transform parent)
    {
        if(parent.name == "Lable")
        {
            parent.gameObject.SetActive(true);
            return false;
        }
        bool isStillActive = false;

        foreach(Transform child in parent)
        {
            var slider = child.GetComponent<Slider>();
            if(slider != null && characterCreation?.poseSliderDatas != null)
            {
                var sliderName = slider.name;
                var data = characterCreation.poseSliderDatas.FirstOrDefault(x => x.name == sliderName);
                if (data == null)
                    child.gameObject.SetActive(false);
                else
                {
                    child.gameObject.SetActive(true);
                    if (characterCreation.sliderValues.ContainsKey(sliderName))
                        slider.SetValueWithoutNotify(characterCreation.sliderValues[sliderName]);
                    else slider.SetValueWithoutNotify(0);
                    slider.minValue = data.marginValues.x;
                    slider.maxValue = data.marginValues.y;
                }
                isStillActive |= child.gameObject.activeSelf;
            }
            else if (child.name == "Look")
            {
                var slider2D = child.GetComponent<MySlider2D>();
                if (slider2D != null && characterCreation?.poseSliderDatas != null)
                {
                    var dataX = characterCreation.poseSliderDatas.FirstOrDefault(x => x.name == "Look X");
                    var dataY = characterCreation.poseSliderDatas.FirstOrDefault(x => x.name == "Look Y");
                    if (dataX == null || dataY == null)
                        child.gameObject.SetActive(false);
                    else
                    {
                        child.gameObject.SetActive(true);
                        slider2D.MinValues = new Vector2(dataX.marginValues.x, dataY.marginValues.x);
                        slider2D.MaxValues = new Vector2(dataX.marginValues.y, dataY.marginValues.y);

                        var valX = 0f;
                        var valY = 0f;
                        if (characterCreation.sliderValues.ContainsKey("Look X"))
                            valX = characterCreation.sliderValues["Look X"];
                        if (characterCreation.sliderValues.ContainsKey("Look Y"))
                            valY = characterCreation.sliderValues["Look Y"];

                        slider2D.Value = new Vector2(valX, valY);
                    }

                    isStillActive |= child.gameObject.activeSelf;
                }
            }
            else
            {
                isStillActive |= SetPoseSliders(child);
            }
        }

        if(parent != poseSpecificValuesParent)
            parent.gameObject.SetActive(isStillActive);

        return isStillActive;
    }

    public void OnPoseSliderChange(Slider slider)
    {
        ChangeSliderValue(slider.name, slider.value);
    }

    public void OnPoseEyeSliderChange(MySlider2D slider)
    {
        //ChangeSliderValue("Look X", slider.Value.x);
        //ChangeSliderValue("Look Y", slider.Value.y);
        ChangeSliderValue(new DictionaryKeyValue<string, float>[] 
        {
            new DictionaryKeyValue<string, float>("Look X", slider.Value.x),
            new DictionaryKeyValue<string, float>("Look Y", slider.Value.y)
        });

    }

    public void ChangeSliderValue(string name, float value)
    {
        if(characterCreation != null)
            characterCreation.ChangeSliderValue(name, value);
    }

    public void ChangeSliderValue(DictionaryKeyValue<string, float>[] values)
    {
        if (characterCreation != null)
            characterCreation.ChangeSlidersValue(values);
    }
    public void OnBoneSettingClickDown(Slider value)
    {
        boneSettingSlider = value;
    }

    public void OnBoneSettingChange(Slider value)
    {
        if(characterBoneSeleted != null)
        {
            //value.SetValueWithoutNotify(0);
            var val = Vector3.zero;

            if(curentType == HandleType.SCALE && linkScaleBoneSetting.isOn)
            {
                val = new Vector3(value.value, value.value, value.value);
            }
            else
            {
                if (value.name == "X")
                    val.x = value.value;
                else if (value.name == "Y")
                    val.y = value.value;
                else val.z = value.value;
            }

            if (characterCreation != null)
                characterCreation.AddValueToManualOffset(characterBoneSeleted, val, curentType);
            else if(characterBoneSeleted == WorldMenuBehaviour.Instance.sun)
            {
                characterBoneSeleted.Rotate(val, Space.Self);
            }
        }    
    }

    public void OnBoneSettingClickUp(Slider value)
    {
        value.SetValueWithoutNotify(0);
        boneSettingSlider = null;
    }

    public void ResetBoneSetting()
    {
        if (characterCreation != null)
            characterCreation.ResetMoveOffset(characterBoneSeleted, curentType);
    }

    public void MirrorBone()
    {
        if (characterCreation != null)
            characterCreation.MirrorBones(characterBoneSeleted);
    }

    public void MirrorBoneSetting(Button buttonAxis)
    {
        if (characterCreation != null)
            characterCreation.MirrorBoneSetting(characterBoneSeleted, buttonAxis.name, curentType);
    }

    public void RotateBoneSetting(Button buttonAxis)
    {
        if (characterCreation != null)
        {
            //characterCreation.RotateBoneSetting(characterBoneSeleted, buttonAxis.name);
            var axis = buttonAxis.name.Trim().ToLower()[0];

            var val = Vector3.zero;
            if (axis == 'x')
                val.x = 45;
            else if (axis == 'y')
                val.y = 45;
            else val.z = 45;

            characterCreation.AddValueToManualOffset(characterBoneSeleted, val, HandleType.ROTATION);
        }
    }
}
