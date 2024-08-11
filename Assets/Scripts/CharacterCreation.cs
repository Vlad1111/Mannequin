using RuntimeHandle;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterCreation : MonoBehaviour
{
    [System.Serializable]
    public class BoneData
    {
        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public Vector3 Scale = Vector3.zero;

        public BoneData() { }
        public BoneData(Transform obj)
        {
            Position = obj.localPosition;
            Rotation = obj.localEulerAngles;
            Scale = obj.localScale;
        }

        public override string ToString()
        {
            string s = "P: " + Position.ToString() + "\n";
            s += "R: " + Rotation.ToString() + "\n";
            s += "S: " + Scale.ToString();
            return s;
        }
    }

    [System.Serializable]
    public class SliderData
    {
        public enum SliderType
        {
            Normal,
            Light,
            Sprite
        }
        [System.Serializable]
        public class SliderBlendShapeData
        {
            public string shapeName;
            public float weight = 1;
        }
        [System.Serializable]
        public class SliderBoneData
        {
            public Transform origin;
            public float weight = 1;
            public Vector3 positionOffset;
            public Vector3 rotationOffset;
            public Vector3 scaleOffset;
        }
        public string name = "";
        public Vector2 marginValues = new Vector2(0, 1);
        public SliderBlendShapeData[] negativeShapes = new SliderBlendShapeData[0];
        public SliderBoneData[] negativeBones = new SliderBoneData[0];
        public SliderBlendShapeData[] positiveShapes = new SliderBlendShapeData[0];
        public SliderBoneData[] positiveBones = new SliderBoneData[0];
        public float overOneValue = 0;
        public SliderBoneData[] possitiveOverOneBones = new SliderBoneData[0];
        public SliderType type = SliderType.Normal;
    }

    [System.Serializable]
    public class CharacterData
    {
        [System.Serializable]
        public class BoneDataSavable
        {
            public MyVector3 Position = Vector3.zero;
            public MyVector3 Rotation = Vector3.zero;
            public MyVector3 Scale = Vector3.zero;
            public override string ToString()
            {
                string s = "P: " + Position.ToString() + "\n";
                s += "R: " + Rotation.ToString() + "\n";
                s += "S: " + Scale.ToString();
                return s;
            }
        }
        public DictionaryKeyValue<string, float>[] sliderValues;
        public DictionaryKeyValue<string, BoneDataSavable>[] boneOffets;
    }

    [System.Serializable]
    public class MirroredBones
    {
        public Transform bone1;
        public Transform bone2;
        public Vector3 positionMultiplier = Vector3.one;
        public Vector3 rotationMultiplyer = Vector3.one;
        public Vector3 rotationOffset = Vector3.zero;
        public Vector3 scaleMultiplier = Vector3.one;
    }

    [System.Serializable]
    public class BlendShapeMinMaxValue
    {
        public string blendName;
        public Vector2 values;
    }
    [System.Serializable]
    public class BlendShapeMinMaxValues
    {
        public BlendShapeMinMaxValue[] perMesh;
    }

    public SliderData[] sliderDatas = new SliderData[0];
    public SliderData[] poseSliderDatas = new SliderData[0];
    public SkinnedMeshRenderer[] meshRenderers = new SkinnedMeshRenderer[0];
    public BlendShapeMinMaxValues[] blendShapeMinMaxValues = new BlendShapeMinMaxValues[0];
    public DictionaryKeyValue<string, DictionaryKeyValue<string, float>[]>[] defaultPrefabs = 
                                                new DictionaryKeyValue<string, DictionaryKeyValue<string, float>[]>[0];
    public MirroredBones[] mirroredBones = new MirroredBones[0];
    public List<Transform> possibleParentBones = new List<Transform>();
    public Transform rigParent;
    public Transform bonesParent;
    private List<Dictionary<string, int>> blendShapesIndex = new List<Dictionary<string, int>>();
    private List<float[]> blendShapesOriginalValues = new List<float[]>();

    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;
    //private RaycastHit raycastHitHandle;
    //private GameObject runtimeTransformGameObj;
    public RuntimeTransformHandle runtimeTransformHandle;
    private static int runtimeTransformLayer = 6;
    private static int runtimeTransformLayerMask;

    public List<Transform> movableParts = new List<Transform>();

    private Dictionary<Transform, BoneData> originalBones = new Dictionary<Transform, BoneData>();
    private Dictionary<string, float> _sliderValues = new Dictionary<string, float>();

    public Material[] characterMaterials = new Material[0]; 

    public Dictionary<string, float> sliderValues
    {
        get => _sliderValues;
        set
        {
            sliderOffsets.Clear();
            finaleShapes.Clear();
            foreach (var part in originalBones.Keys)
            { 
                UpdateBone(part); 
            }
            _sliderValues = value;
            ApplySliders();
        }
    }
    private Dictionary<string, float> finaleShapes = new Dictionary<string, float>();
    Dictionary<Transform, BoneData> sliderOffsets = new Dictionary<Transform, BoneData>();
    Dictionary<Transform, BoneData> manualOffsets = new Dictionary<Transform, BoneData>();

    private List<Collider> colliders = new List<Collider>();

    private bool wasPressed = false;

    private void Awake()
    {
        if (possibleParentBones != null && !possibleParentBones.Contains(transform))
        {
            possibleParentBones.Add(transform);
            for (int i = possibleParentBones.Count - 1; i > 0; i--)
            {
                possibleParentBones[i] = possibleParentBones[i - 1];
            }
            possibleParentBones[0] = transform;
        }
        SaveOriginalData(transform);

        if (meshRenderers != null)
            foreach (var renderer in meshRenderers)
            {
                var dic = new Dictionary<string, int>();
                var dicOr = new float[renderer.sharedMesh.blendShapeCount];
                for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                {
                    dic.Add(renderer.sharedMesh.GetBlendShapeName(i), i);
                    dicOr[i] = renderer.GetBlendShapeWeight(i);
                }
                blendShapesIndex.Add(dic);
                blendShapesOriginalValues.Add(dicOr);
            }
    }

    void Start()
    {
        if (runtimeTransformHandle == null)
            runtimeTransformHandle = RuntimeTransformHandle.Instance;
        SetHandleLayer(runtimeTransformHandle);
        runtimeTransformHandle.type = HandleType.POSITION;
        //runtimeTransformHandle.autoScale = true;
        //runtimeTransformHandle.autoScaleFactor = 1.0f;
        runtimeTransformHandle.gameObject.SetActive(false);

        RuntimeTransformHandle.ApplyLayerToChildren();

        if (defaultPrefabs != null && _sliderValues != null)
            if (defaultPrefabs.Length > 0 && _sliderValues.Count <= 0)
            {
                foreach (var val in defaultPrefabs[0].value)
                {
                    _sliderValues.Add(val.key, val.value);
                }
                ApplySliders();
                EditMenuBehaviour.Instance.ResetSliders();
            }
    }

    private void SaveOriginalData(Transform parent)
    {
        var restriction = parent.GetComponent<HandleTypeRestiction>();
        if(restriction)
        {
            movableParts.Add(parent);
            if(parent == transform && !manualOffsets.ContainsKey(parent))
                manualOffsets.Add(parent, new BoneData());
        }
        if((parent.tag == "Bone" || parent == transform) && !originalBones.ContainsKey(parent))
        {
            originalBones.Add(parent, new BoneData()
            {
                Position = parent.localPosition,
                Rotation = parent.localEulerAngles,
                Scale = parent.localScale,
            });
        }
        var collider = parent.GetComponents<Collider>();
        if(collider != null && collider.Length > 0)
            colliders.AddRange(collider);
        foreach(Transform child in parent)
            SaveOriginalData(child);
    }

    public static bool IsRayOverTransformHandle(Ray ray)
    {
        SetHandleLayer();
        var ret = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, runtimeTransformLayerMask);
        return ret;
    }

    public static void SetHandleLayer(RuntimeTransformHandle runtimeTransformHandle = null)
    {
        if (runtimeTransformHandle == null)
            runtimeTransformHandle = RuntimeTransformHandle.Instance;
        runtimeTransformHandle.gameObject.layer = runtimeTransformLayer;
        runtimeTransformLayerMask = 1 << runtimeTransformLayer; //Layer number represented by a single bit in the 32-bit integer using bit shift
    }

    public void ToggleCreator(bool? value)
    {
        if (value == null)
            value = !enabled;
        enabled = (bool)value;
        if (rigParent != null)
            rigParent.gameObject.SetActive(enabled);
        //if (bonesParent != null)
        //    bonesParent.gameObject.SetActive(enabled);

        foreach (var col in colliders)
            col.enabled = enabled;

        var meshes = GetComponents<MeshRenderer>();
        if (meshes != null)
            foreach (var mesh in meshes)
                mesh.enabled = enabled;
        var skinMeshes = GetComponents<SkinnedMeshRenderer>();
        if (skinMeshes != null)
            foreach (var mesh in skinMeshes)
                mesh.enabled = enabled;
    }

    public void SelectBoneToTranslate(Transform highlight)
    {
        if(runtimeTransformHandle == null)
            runtimeTransformHandle = RuntimeTransformHandle.Instance;
        //Debug.Log(runtimeTransformHandle);
        //Debug.Log(highlight);
        //if (highlight != runtimeTransformHandle.target)
        {
            //debug.text = "Show gizmo";
            selection = highlight;
            runtimeTransformHandle.target = selection;
            runtimeTransformHandle.gameObject.SetActive(true);
            runtimeTransformHandle.endedDraggingHandle.RemoveAllListeners();
            runtimeTransformHandle.endedDraggingHandle.AddListener(() =>
            {
                UpdateBoneOffset(runtimeTransformHandle.target);
            });
        }
    }

    private void VerifyTouchPoint()
    {
        RuntimeTransformHandle.ApplyLayerToChildren();
        var position = Input.mousePosition;
        if (Application.platform == RuntimePlatform.Android)
            position = Input.touches[0].position;
        //debug.text = Input.GetAxis("Fire1") + " " + Input.mousePosition + " " + position;
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out raycastHit)) //Make sure you have EventSystem in the hierarchy before using EventSystem
        {
            highlight = raycastHit.transform;
            //debug.text = "Hit " + highlight.name;
            if (IsRayOverTransformHandle(ray))
            {
            }
            else if (highlight)
            {
                SelectBoneToTranslate(highlight);
                EditMenuBehaviour.Instance.CharacterBoneSeleted = highlight;
            }
            else
            {
                if (selection)
                {
                    selection = null;
                    runtimeTransformHandle.gameObject.SetActive(false);
                    EditMenuBehaviour.Instance.CharacterBoneSeleted = null;
                }
            }
        }
        else
        {
            if (selection)
            {
                selection = null;
                runtimeTransformHandle.gameObject.SetActive(false);
                EditMenuBehaviour.Instance.CharacterBoneSeleted = null;
            }
        }
    }

    private float lastTime = -1;
    void Update()
    {
        // Selection
        if (Input.GetAxis("Fire1") > 0 && !EventSystem.current.IsPointerOverUIObject() && Input.touchCount <= 1)
        {
            if (wasPressed == false)
            {
                lastTime = Time.time;
            }
            wasPressed = true;
        }
        else
        {
            wasPressed = false;
            //debug.text = "Not pressed";
        }

        if(lastTime > 0 && wasPressed == false)
        {
            var delta = Time.time - lastTime;
            if(delta < 0.2f)
            {
                VerifyTouchPoint();
            }
            lastTime = -1;
        }
    }

    public void ChangeSlidersValue(DictionaryKeyValue<string, float>[] values)
    {
        foreach(var val in values)
        {
            if (_sliderValues.ContainsKey(val.key))
                _sliderValues[val.key] = val.value;
            else _sliderValues.Add(val.key, val.value);
        }
        ApplySliders();
    }
    public void ChangeSliderValue(string valueName, float value)
    {
        if(_sliderValues.ContainsKey(valueName))
            _sliderValues[valueName] = value;
        else _sliderValues.Add(valueName, value);

        ApplySliders();
    }

    public void UpdateBoneOffset(Transform bone)
    {
        if(movableParts.Contains(bone))
        {
            var original = originalBones[bone];
            var slider = new BoneData();
            if (sliderOffsets.ContainsKey(bone))
                slider = sliderOffsets[bone];
            var manualOffset = new BoneData();

            //Debug.Log(bone.localPosition + " " + original.Position + " " + slider.Position);
            manualOffset.Position = bone.localPosition - original.Position - slider.Position;
            //manualOffset.Rotation = bone.localEulerAngles - original.Rotation - slider.Rotation;
            manualOffset.Rotation = (
                Quaternion.Inverse(Quaternion.Euler(slider.Rotation)) *
                Quaternion.Inverse(Quaternion.Euler(original.Rotation)) *
                bone.localRotation
                ).eulerAngles;
            manualOffset.Scale = bone.localScale - original.Scale - slider.Scale;
            //Debug.Log(manualOffset.Position);

            if (manualOffsets.ContainsKey(bone))
                manualOffsets[bone] = manualOffset;
            else manualOffsets.Add(bone, manualOffset);

            UpdateBone(bone, slider, manualOffset);
        }
    }

    public void ApplySliders()
    {
        sliderOffsets.Clear();
        finaleShapes.Clear();

        var allSliderData = new List<SliderData>();
        allSliderData.AddRange(sliderDatas);
        allSliderData.AddRange(poseSliderDatas);

        foreach (var keyVal in _sliderValues)
        {
            var slider = allSliderData.FirstOrDefault((x) => x.name == keyVal.Key);
            if (slider == null)
                continue;
            float val = keyVal.Value;
            SliderData.SliderBlendShapeData[] shapes = slider.positiveShapes;
            SliderData.SliderBoneData[] bones = slider.positiveBones;
            float overOneValue = slider.overOneValue;
            SliderData.SliderBoneData[] overOnebones = slider.possitiveOverOneBones;
            if (val < 0)
            {
                val *= -1;
                shapes = slider.negativeShapes;
                bones = slider.negativeBones;
                overOnebones = new SliderData.SliderBoneData[] { };
                overOneValue = 1;
            }

            if (val <= 1)
            {
                foreach (var shape in shapes)
                {
                    if (finaleShapes.ContainsKey(shape.shapeName))
                    {
                        finaleShapes[shape.shapeName] += val * shape.weight;
                    }
                    else finaleShapes.Add(shape.shapeName, val * shape.weight);
                }
                foreach (var bone in bones)
                {
                    if (sliderOffsets.ContainsKey(bone.origin))
                    {
                        var newOff = sliderOffsets[bone.origin];
                        newOff.Position += bone.positionOffset * bone.weight * val;
                        newOff.Rotation += bone.rotationOffset * bone.weight * val;
                        newOff.Scale += bone.scaleOffset * bone.weight * val;
                    }
                    else sliderOffsets.Add(bone.origin, new BoneData()
                    {
                        Position = bone.positionOffset * bone.weight * val,
                        Rotation = bone.rotationOffset * bone.weight * val,
                        Scale = bone.scaleOffset * bone.weight * val,
                    });
                }
            }
            else
            {
                foreach (var bone in overOnebones)
                {
                    var prevBone = bones.FirstOrDefault(x => x.origin == bone.origin);
                    if (prevBone == null)
                        continue;

                    var boneData = new BoneData();
                    boneData.Position = Vector3.Lerp(prevBone.positionOffset, bone.positionOffset, bone.weight * (val - 1) / overOneValue);
                    boneData.Rotation = Vector3.Lerp(prevBone.rotationOffset, bone.rotationOffset, bone.weight * (val - 1) / overOneValue);
                    boneData.Scale = Vector3.Lerp(prevBone.scaleOffset, bone.scaleOffset, bone.weight * (val - 1) / overOneValue);

                    if (sliderOffsets.ContainsKey(bone.origin))
                    {
                        var newOff = sliderOffsets[bone.origin];
                        newOff.Position += boneData.Position;
                        newOff.Rotation += boneData.Rotation;
                        newOff.Scale += boneData.Scale;
                    }
                    else sliderOffsets.Add(bone.origin, boneData);
                }
            }
            if (slider.type == SliderData.SliderType.Light)
            {
                var light = slider.positiveBones[0].origin.GetComponent<Light>();
                val = keyVal.Value;
                switch (slider.name.ToLower())
                {
                    case "angle":
                        light.spotAngle = val * 180 + 40;
                        break;
                    case "range":
                        light.range = val + 10;
                        break;
                    case "intensity":
                        light.intensity = val + 1;
                        break;
                    case "color":
                        var color = Color.white;
                        if (val > 0.01)
                            color = Color.HSVToRGB(val, 1, 1);
                        light.color = color;
                        break;
                    default:
                        break;
                }
            }
            else if(slider.type == SliderData.SliderType.Sprite)
            {
                val = keyVal.Value;
                switch (slider.name.ToLower())
                {
                    case "alpha":
                        val += 1;
                        foreach(var mat in characterMaterials)
                        {
                            var col = mat.GetColor("_BaseColor");
                            col.a = val;
                            mat.SetColor("_BaseColor", col);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        if (blendShapesIndex.Count != meshRenderers.Length)
            Awake();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            var render = meshRenderers[i];
            var indexes = blendShapesIndex[i];
            foreach (var index in indexes.Values)
            {
                render.SetBlendShapeWeight(index, blendShapesOriginalValues[i][index]);
            }
            //render.sharedMesh.RecalculateNormals();
        }
        foreach (var shape in finaleShapes.Keys.ToArray())
        {
            float value = finaleShapes[shape];
            //if (value > 1)
            //{
            //    value = 1;
            //    finaleShapes[shape] = value;
            //}
            //else if (value < 0)
            //{ 
            //    value = 0;
            //    finaleShapes[shape] = value;
            //}
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (blendShapesIndex[i].ContainsKey(shape))
                {
                    //Debug.Log(meshRenderers[i] + " " + shape + " " + blendShapesIndex[i].ContainsKey(shape));
                    var index = blendShapesIndex[i][shape];
                    var newVal = value * 100 + blendShapesOriginalValues[i][index];
                    //Debug.Log(shape + " " + value + " " + blendShapesOriginalValues[i][index]);
                    BlendShapeMinMaxValue minMacValuesData = null;
                    if(i < blendShapeMinMaxValues.Length)
                        minMacValuesData = blendShapeMinMaxValues[i].perMesh.FirstOrDefault(x => x.blendName == shape);
                    if (minMacValuesData == null)
                        minMacValuesData = new BlendShapeMinMaxValue() { values = new Vector2(0, 1) };
                    var minMaxValue = minMacValuesData.values * 100;
                    if (newVal > minMaxValue.y)
                        newVal = minMaxValue.y;
                    else if (newVal < minMaxValue.x)
                        newVal = minMaxValue.x;
                    meshRenderers[i].SetBlendShapeWeight(index, newVal);
                }
            }
        }

        foreach (var bone in sliderOffsets)
        {
            UpdateBone(bone.Key);
        }
    }

    public void AddValueToManualOffset(Transform bone, Vector3 value, HandleType type)
    {
        if(originalBones.ContainsKey(bone))
        {
            var manual = new BoneData();
            if(manualOffsets.ContainsKey(bone))
                manual = manualOffsets[bone];
            else manualOffsets.Add(bone, manual);

            if (type == HandleType.POSITION)
                manual.Position += value / 60f;
            else if (type == HandleType.ROTATION)
            {
                var newRotation = Quaternion.Euler(manual.Rotation) * Quaternion.Euler(value);

                manual.Rotation = newRotation.eulerAngles;//+= value;
            }
            else manual.Scale += value / 200;

            UpdateBone(bone, manualOffset: manual);
        }
    }

    public void ResetMoveOffset(Transform bone, HandleType type)
    {
        if (manualOffsets.ContainsKey(bone))
        {
            var manual = manualOffsets[bone];
            if (type == HandleType.POSITION)
                manual.Position = Vector3.zero;
            else if (type == HandleType.ROTATION)
                manual.Rotation = Vector3.zero;
            else manual.Scale = Vector3.zero;

            UpdateBone(bone, manualOffset: manual);
        }
    }

    public void MirrorBoneSetting(Transform bone, string axis, HandleType type)
    {
        if (manualOffsets.ContainsKey(bone))
        {
            var manual = manualOffsets[bone];
            axis = axis.Trim().ToLower();
            if (type == HandleType.POSITION)
            {
                if (axis[0] == 'x')
                    manual.Position.x = -manual.Position.x;
                else if (axis[0] == 'y')
                    manual.Position.y = -manual.Position.y;
                else
                    manual.Position.z = -manual.Position.z;
            }
            else if (type == HandleType.ROTATION)
            {
                //var angle = Quaternion.Inverse(Quaternion.Euler(manual.Rotation)).eulerAngles;
                if (axis[0] == 'x')
                    manual.Rotation.x = -manual.Rotation.x;
                else if (axis[0] == 'y')
                {
                    //manual.Rotation.y = angle.y;
                    //manual.Rotation.z = 180 - manual.Rotation.z;
                    manual.Rotation.y = -manual.Rotation.y;
                    manual.Rotation.z = 180 - manual.Rotation.z;
                }
                else
                    manual.Rotation.z = -manual.Rotation.z;
            }
            else
            {
                if (axis[0] == 'x')
                    manual.Scale.x = -manual.Scale.x;
                else if (axis[0] == 'y')
                    manual.Scale.y = -manual.Scale.y;
                else
                    manual.Scale.z = -manual.Scale.z;
            }
            UpdateBone(bone, manualOffset: manual);
        }
    }

    private void UpdateBone(Transform bone, BoneData sliderOffset = null, BoneData manualOffset = null)
    {
        BoneData original = null;
        if(originalBones.ContainsKey(bone))
            original = originalBones[bone];
        else
        {
            original = new BoneData(bone);
            originalBones.Add(bone, original);
        }
        if(sliderOffset == null)
        {
            if (sliderOffsets.ContainsKey(bone))
                sliderOffset = sliderOffsets[bone];
            else sliderOffset = new BoneData();
        }
        if (manualOffset == null)
        {
            manualOffset = new BoneData();
            if (manualOffsets.ContainsKey(bone))
                manualOffset = manualOffsets[bone];
        }
        //Debug.Log(bone);
        //Debug.Log("O: " + original.Position);
        //Debug.Log("S: " + sliderOffset.Position);
        //Debug.Log("M: " + manualOffset.Position);
        bone.localPosition = original.Position + sliderOffset.Position + manualOffset.Position;
        //bone.localEulerAngles = original.Rotation + sliderOffset.Rotation + manualOffset.Rotation;
        bone.localEulerAngles = original.Rotation;
        bone.Rotate(sliderOffset.Rotation, Space.Self);
        bone.Rotate(manualOffset.Rotation, Space.Self);
        bone.localScale = original.Scale + sliderOffset.Scale + manualOffset.Scale;
        //Debug.Log("F: " + bone.localPosition);
    }

    public CharacterData GetCharacterData()
    {
        var data = new CharacterData()
        {
            sliderValues = GENERAL.GetObjectSliderValues(this),
        };

        List<DictionaryKeyValue<string, CharacterData.BoneDataSavable>> list = 
            new List<DictionaryKeyValue<string, CharacterData.BoneDataSavable>>();
        foreach(var bone in movableParts)
        {
            if(manualOffsets.ContainsKey(bone))
            {
                var offset = manualOffsets[bone];
                list.Add(new DictionaryKeyValue<string, CharacterData.BoneDataSavable>(
                        bone.name,
                        new CharacterData.BoneDataSavable()
                        {
                            Position = offset.Position,
                            Rotation = offset.Rotation,
                            Scale = offset.Scale,
                        }   
                    ));
            }
        }
        data.boneOffets = list.ToArray();

        return data;
    }

    public void ApplyData(CharacterData data)
    {
        _sliderValues.Clear();
        foreach(var slider in data.sliderValues)
            _sliderValues.Add(slider.key, slider.value);
        ApplySliders();

        manualOffsets.Clear();
        if (data.boneOffets.Length > 0)
        {
            var mainBoneOffset = new BoneData()
            {
                Position = data.boneOffets[0].value.Position,
                Rotation = data.boneOffets[0].value.Rotation,
                Scale = data.boneOffets[0].value.Scale,
            };
            if(movableParts.Count <= 0)
            {
                movableParts.Add(transform);
            }
            manualOffsets.Add(movableParts[0], mainBoneOffset);
            //SaveMenusBehaviour.Instance.debug.text = movableParts[0] + " " + data.boneOffets[0].value;
            //Debug.Log(movableParts[0]);
            //Debug.Log(mainBoneOffset);
            UpdateBone(movableParts[0], manualOffset: mainBoneOffset);
        }
        foreach (var offset in data.boneOffets)
        {
            var bone = movableParts.FirstOrDefault(x => x.name == offset.key);
            if (bone == default(Transform))
                continue;

            if (!manualOffsets.ContainsKey(bone))
            {
                var boneData = new BoneData()
                {
                    Position = offset.value.Position,
                    Rotation = offset.value.Rotation,
                    Scale = offset.value.Scale,
                };
                //Debug.Log(bone);
                //Debug.Log(boneData);
                manualOffsets.Add(bone, boneData);
                UpdateBone(bone, manualOffset: boneData);
            }
        }

        if (rigParent != null)
        {
            var rigs = rigParent.GetComponentsInChildren<InverseKinematics>(true);
            foreach (var rig in rigs)
                rig.UpdateOnce();
            var rests = rigParent.GetComponentsInChildren<MyParentRestriction>(true);
            foreach (var res in rests)
                res.UpdateOnce();
        }
    }

    public bool HasMirror(Transform bone)
    {
        foreach (var mirr in mirroredBones)
            if (bone == mirr.bone1 || bone == mirr.bone2)
                return true;
        return false;
    }

    public void MirrorBones(Transform bone)
    {
        if (!manualOffsets.ContainsKey(bone)) return;
        foreach (var mb in mirroredBones)
        {
            var rotOffset = mb.positionMultiplier;
            var target = mb.bone2;

            if (mb.bone2 == bone)
            {
                rotOffset *= -1;
                target = mb.bone1;
            }
            else if (mb.bone1 != bone) continue;

            var boneOffset = manualOffsets[bone];
            var targetOffset = new BoneData();
            if(manualOffsets.ContainsKey(target))
                targetOffset = manualOffsets[target];
            else manualOffsets.Add(target, targetOffset);

            targetOffset.Position = new Vector3(
                                        boneOffset.Position.x * mb.positionMultiplier.x,
                                        boneOffset.Position.y * mb.positionMultiplier.y,
                                        boneOffset.Position.z * mb.positionMultiplier.z
                                    );
            targetOffset.Rotation = new Vector3(
                                        boneOffset.Rotation.x * mb.rotationMultiplyer.x,
                                        boneOffset.Rotation.y * mb.rotationMultiplyer.y,
                                        boneOffset.Rotation.z * mb.rotationMultiplyer.z
                                    ) + rotOffset;
            targetOffset.Scale = new Vector3(
                                        boneOffset.Scale.x * mb.scaleMultiplier.x,
                                        boneOffset.Scale.y * mb.scaleMultiplier.y,
                                        boneOffset.Scale.z * mb.scaleMultiplier.z
                                    );
            UpdateBone(target, manualOffset: targetOffset);

            break;
        }
    }
}
