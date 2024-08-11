using RuntimeHandle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldMenuBehaviour : MonoBehaviour
{
    private static WorldMenuBehaviour _instance = null;
    public static WorldMenuBehaviour Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<WorldMenuBehaviour>(true);
            return _instance;
        }
        set => _instance = value;
    }
    private void Awake()
    {
        Instance = this;
    }

    [System.Serializable]
    public class ObjectsData
    {
        public string name;
        public int id;
        public bool isShown = true;
        [System.NonSerialized]
        public Transform worldObject;
        [System.NonSerialized]
        public TextButton UILable;
        public string prefabName;
        public string parentBoneName;
        [System.NonSerialized]
        public CharacterCreation creator;
        public List<ObjectsData> childs = new List<ObjectsData>();

        public ObjectsData CreateCopy()
        {
            var ret = new ObjectsData()
            {
                name = name,
                prefabName = prefabName,
                parentBoneName = parentBoneName,
            };
            foreach(var c in childs)
                ret.childs.Add(c.CreateCopy());

            return ret;
        }
    }

    [System.Serializable]
    public class WorldData
    {
        public ObjectsData[] objects = new ObjectsData[] { };
        public Vector3 sunRotation;
        public float sunIntensity = 1;
    }

    [System.Serializable]
    public class PrefabObjectsFolders
    {
        public static string defaultCharacter = ">";
        public string name;
        public string characterFile = defaultCharacter;
        public bool isToggled = false;
        public PrefabObjectsFolders[] childrens;
    }

    public PrefabObjectsFolders[] prefabs;

    [Space(20)]
    public CharacterCreation sun;
    public Slider sunIntensity;

    [Space(20)]
    public Transform worldParent;
    public Transform objectUiParent;
    public Transform objectUiPrefab;

    [Space(20)]
    public Transform parentDeleteWorning;
    public Transform prefabsView;
    public Transform prefabsParent;
    public TMP_Dropdown parentDropdown;
    public TMP_InputField seletedNameInput;
    public List<ObjectsData> objects = new List<ObjectsData>();
    public ObjectsData lastObjectSelected = null;
    private TextButton lastObjectSelectedText = null;
    public Transform objectMenu;
    public Button bodyObjectButton;
    public Button poseObjectButton;
    public Toggle selectedObjectToggle;

    private void Start()
    {
        sunIntensity.SetValueWithoutNotify(sun.GetComponent<Light>().intensity);
        UpdateMenuList();
        foreach (Transform child in prefabsParent)
            Destroy(child.gameObject);
        UpdatePrefabListUI();
    }

    private void UpdatePrefabListUI()
    {
        foreach (Transform child in prefabsParent)
            Destroy(child.gameObject);
        for (int i = 0; i < prefabs.Length; i++)
        {
            var prefab = prefabs[i];
            UpdatePrefabListUI(prefab, isTheLastInFolder: i == prefabs.Length - 1);
        }

        var line = Instantiate(objectUiPrefab, prefabsParent).GetComponent<TextButton>();
        line.SetText(" ~ Import object ~ ");
        line.onClick.AddListener(() => ImportedObjectsBehaviour.Instance.TurnOnOff(true));
        
        line = Instantiate(objectUiPrefab, prefabsParent).GetComponent<TextButton>();
        line.text.text = "";
        line.enabled = false;
    }
    private void UpdatePrefabListUI(PrefabObjectsFolders curent, int depth = 0, bool isTheLastInFolder = false, string folderPath = "")
    {
        var line = Instantiate(objectUiPrefab, prefabsParent);
        var text = line.GetComponent<TextButton>();
        string lineName = " ";
        if (depth > 0)
        {
            for (int i = 0; i < depth - 1; i++) lineName += "\U00002502";
            lineName = lineName + "\U00002502\n" + lineName;
            if(isTheLastInFolder)
                lineName += "\U00002514";
            else lineName += "\U0000251C";
        }
        else
        {
            lineName = "\n";
        }
        //for (int i = 0; i < depth -1; i++) lineName += "| ";
        if (curent.childrens.Length > 0)
        {
            if (curent.isToggled) lineName += "\U0001F5C1:";
            else lineName += "\U0001F5C0:";
        }
        else
        {
            if (curent.characterFile == "" || curent.characterFile == null)
                curent.characterFile = PrefabObjectsFolders.defaultCharacter;
            lineName += curent.characterFile + " ";
        }
        lineName += curent.name;
        text.text.text = lineName;
        text.text.horizontalAlignment = HorizontalAlignmentOptions.Left;
        text.text.lineSpacing = -23f;
        string fullPath = "";
        if (folderPath == "")
            fullPath = curent.name;
        else fullPath = folderPath + "/" + curent.name;
        if (curent.childrens.Length > 0)
        {
            if (curent.isToggled)
                for (int i = 0; i < curent.childrens.Length; i++)
                {
                    var child = curent.childrens[i];
                    if(child.characterFile == PrefabObjectsFolders.defaultCharacter ||
                        child.characterFile == "" || child.characterFile == null)
                        child.characterFile = curent.characterFile;
                    UpdatePrefabListUI(child, depth + 1, i == curent.childrens.Length - 1, fullPath);
                }

            text.onClick.AddListener(() =>
            {
                curent.isToggled = !curent.isToggled;
                UpdatePrefabListUI();
                text.DeSelect();
            });
        }
        else text.onClick.AddListener(() =>
        {
            prefabsView.gameObject.SetActive(false);
            AddNewObject(fullPath);
            text.DeSelect();
        });
    }

    private ObjectsData FindObjectById(List<ObjectsData> parent, int id)
    {
        foreach (ObjectsData obj in parent)
        {
            if(obj.id == id)
                return obj;
            var rez = FindObjectById(obj.childs, id);
            if (rez != null)
                return rez;
        }
        return null;
    }

    private ObjectsData FindObjectParentById(int id, ObjectsData currentObject = null)
    {
        if(currentObject == null)
        {
            foreach(var obj in objects)
            {
                if (obj.id == id)
                    return null;
                var rez = FindObjectParentById(id, obj);
                if (rez != null)
                    return rez;
            }
            return null;
        }
        foreach (ObjectsData obj in currentObject.childs)
        {
            if (obj.id == id)
                return currentObject;
            var rez = FindObjectParentById(id, obj);
            if (rez != null)
                return rez;
        }
        return null;
    }

    public void SelectSun()
    {
        if (lastObjectSelected != null)
        {
            if (lastObjectSelected.creator != null)
                lastObjectSelected.creator.ToggleCreator(false);
            if (lastObjectSelectedText != null)
                lastObjectSelectedText.DeSelect();
            lastObjectSelectedText = null;
        }
        RuntimeTransformHandle.Instance.target = sun.transform;
        RuntimeTransformHandle.Instance.gameObject.SetActive(true);
        EditMenuBehaviour.Instance.ChangeGizmosType(HandleType.ROTATION);
        EditMenuBehaviour.Instance.CharacterBoneSeleted = sun.transform;
        sun.ToggleCreator(true);
        sun.enabled = false;
        lastObjectSelected = new ObjectsData()
        {
            name = "sun",
            creator = sun,
            worldObject = sun.transform,
        };
    }

    public void OnSunIntensityChanged()
    {
        sun.transform.GetComponent<Light>().intensity = sunIntensity.value;
    }

    public void AddNewObject()
    {
        prefabsView.gameObject.SetActive(true);
    }

    public void AddNewObjectFromImported(int id)
    {
        AddNewObject("[import]" + id, instantiateNew: true);
    }

    public void HideNewObject()
    {
        prefabsView.gameObject.SetActive(false);
    }

    public string FindTheNewPrefabPath(string prefabName, PrefabObjectsFolders[] curents = null, string folderPath = "")
    {
        if (curents == null)
            curents = prefabs;
        foreach(var curent in curents)
        {
            var path = curent.name;
            if(folderPath != "")
                path = folderPath + "/" + curent.name;
            if(curent.childrens.Length > 0)
            {
                var ret = FindTheNewPrefabPath(prefabName, curent.childrens, path);
                if (ret != null)
                    return ret;
            }
            else if (curent.name == prefabName)
                return path;
        }
        return null;
    }

    private Transform AddNewObject(string prefabName, bool instantiateNew = true, Transform parentObject = null)
    {
        if(!prefabName.Contains("/"))
        {
            var newPath = FindTheNewPrefabPath(prefabName, prefabs);
            if (newPath != null)
                prefabName = newPath;
        }
        Transform parent = worldParent;
        if (parentObject == null)
        {
            if (parentDropdown.value > 0)
            {
                var parentName = parentDropdown.options[parentDropdown.value].text;
                foreach (var bone in lastObjectSelected.creator.possibleParentBones)
                    if (bone.name == parentName)
                    {
                        parent = bone;
                        break;
                    }
            }
        }
        else parent = parentObject;

        Transform objTransform = null;
        if(prefabName.StartsWith("[import]"))
        {
            var objId = prefabName.Substring(8);
            objTransform = ImportedObjectsBehaviour.InstantiateObject(int.Parse(objId));
            if (objTransform != null)
                objTransform.parent = parent;
        }
        else
        {
            var prefab = Resources.Load<Transform>("Prefab/" + prefabName);
            if (prefab == null)
            {
                var simplePrefabName = prefabName.Split('/').Last();
                var newPath = FindTheNewPrefabPath(simplePrefabName, prefabs);
                if (newPath != null)
                {
                    return AddNewObject(newPath, instantiateNew, parentObject);
                }
                return null;
            }
            objTransform = Instantiate(prefab, parent);
            objTransform.name = prefabName.Split("/").Last();
        }

        if(instantiateNew && objTransform != null)
        {
            int id = (new System.Random()).Next();
            while (FindObjectById(objects, id) != null)
                id = (new System.Random()).Next();

            var newData = new ObjectsData()
            {
                name = objTransform.name,
                id = id,
                worldObject = objTransform,
                prefabName = prefabName,
                parentBoneName = parent?.name,
                creator = objTransform.GetComponent<CharacterCreation>()
            };

            if (parentDropdown.value > 0 && lastObjectSelected != null)
                lastObjectSelected.childs.Add(newData);
            else
                objects.Add(newData);
            SelectObject(newData);
            UpdateMenuList();
        }
        return objTransform;
    }

    private void DeleteNullObjects(List<ObjectsData> parents)
    {
        for (int i = 0; i < parents.Count; i++)
        {
            var obj = parents[i];
            if (obj.worldObject == null)
            {
                parents.RemoveAt(i);
                i--;
                continue;
            }
            for (int j = 0; j < obj.childs.Count; j++)
            {
                var child = obj.childs[j];
                if (child.worldObject == null)
                {
                    obj.childs.RemoveAt(j);
                    j--;
                    continue;
                }
                else DeleteNullObjects(new List<ObjectsData> { child });
            }
        }
    }

    public void UpdateMenuList()
    {
        foreach (Transform parent in objectUiParent)
            Destroy(parent.gameObject);
        UpdateMenuList(objects, 0);
    }

    public void UpdateMenuList(List<ObjectsData> parents, int level)
    {
        foreach (var obj in parents)
        {
            var line = Instantiate(objectUiPrefab, objectUiParent);
            var button = line.GetComponent<TextButton>();
            obj.UILable = button;
            var text = button.text;
            string name = "";
            for (int i = 0; i < level; i++)
                name += "_ ";
            text.text = name + obj.name;
            text.color = obj.isShown ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            button.onClick.AddListener(() =>
            {
                SelectObject(obj);
                if (lastObjectSelectedText != null && lastObjectSelectedText != button)
                    lastObjectSelectedText.DeSelect();
                lastObjectSelectedText = button;
            });
            if (lastObjectSelected.id == obj.id)
            {
                if (lastObjectSelectedText != null)
                    lastObjectSelectedText.DeSelect();
                lastObjectSelectedText = button;
                button.ApplySelect();
            }
            UpdateMenuList(obj.childs, level + 1);
        }
    }

    public void ToggleMenuButtonsMenu(bool active)
    {
        objectMenu.gameObject.SetActive(active);
        if(lastObjectSelected?.creator != null)
        {
            poseObjectButton.gameObject.SetActive(lastObjectSelected.creator.poseSliderDatas.Length > 0);
            bodyObjectButton.gameObject.SetActive(lastObjectSelected.creator.sliderDatas.Length > 0);
        }
        selectedObjectToggle.SetIsOnWithoutNotify(lastObjectSelected.isShown);
    }

    public void SelectObject(int id)
    {
        var obj = FindObjectById(objects, id);
        if(obj != null)
            SelectObject(obj);
    }

    public void SelectObject(ObjectsData data)
    {
        if(lastObjectSelected != null && lastObjectSelected.creator != null)
                lastObjectSelected.creator.ToggleCreator(false);

        if(data != null)
        {
            if (data.creator != null && data?.creator?.possibleParentBones != null)
            { 
                data.creator.ToggleCreator(true);
                EditMenuBehaviour.Instance.CharacterCreation = data.creator;

                var parentList = new List<string>() { "World" };
                parentList.AddRange(data.creator.possibleParentBones.Select(x => x.name));
                parentDropdown.ClearOptions();
                parentDropdown.AddOptions(parentList.Select(x =>
                    new TMP_Dropdown.OptionData(x)).ToList());
                parentDropdown.Select();
                data.creator.SelectBoneToTranslate(data.worldObject);
            }
            if (lastObjectSelected.id == data.id)
                ToggleMenuButtonsMenu(true);
            lastObjectSelected = data;
            seletedNameInput.SetTextWithoutNotify(data.name);
        }
        else
        {
            RuntimeTransformHandle.Instance.gameObject.SetActive(false);
            RuntimeTransformHandle.Instance.target = null;
        }
    }

    public void ShowDeleteWarning()
    {
        if(lastObjectSelected != null)
        {
            parentDeleteWorning.gameObject.SetActive(true);
        }
    }

    public void HideDeleteWorning()
    {
        parentDeleteWorning.gameObject.SetActive(false);
    }

    public void DeleteSelected()
    {
        Destroy(lastObjectSelected.worldObject.gameObject);
        lastObjectSelected.worldObject = null;
        DeleteNullObjects(objects);
        if (objects.Count > 0)
            SelectObject(objects[0]);
        UpdateMenuList();
        HideDeleteWorning();
    }

    public void OnNameChange()
    {
        if(lastObjectSelected != null)
        {
            lastObjectSelected.name = seletedNameInput.text;
            //lastObjectSelected.UILable.text.text = lastObjectSelected.name;
            UpdateMenuList();
        }
    }

    public WorldData GetWorldData()
    {
        return new WorldData()
        {
            objects = objects.ToArray(),
            sunRotation = sun.transform.eulerAngles,
            sunIntensity = sun.transform.GetComponent<Light>().intensity,
        };
    }

    private void CreateObjectBasedOnHerarcy(ObjectsData curent, ObjectsData parentData)
    {
        Transform parent = worldParent;
        if(parentData != null)
        {
            var parentName = curent.parentBoneName;
            foreach (var bone in parentData.creator.possibleParentBones)
                if (bone.name == parentName)
                {
                    parent = bone;
                    break;
                }
        }
        //
        //var prefab = Resources.Load<Transform>("Prefab/" + curent.prefabName);
        //var transform = Instantiate(prefab, parent);
        //transform.name = curent.prefabName;

        var transform = AddNewObject(curent.prefabName, false, parent);
        transform.gameObject.SetActive(curent.isShown);
        curent.worldObject = transform;
        curent.creator = transform.GetComponent<CharacterCreation>();
        //Debug.Log("Parent: " + curent.worldObject + " " + parent);

        if (curent.creator != null)
        {
            curent.creator.ToggleCreator(false);
            var data = SaveMenusBehaviour.Instance.LoadCaracterData(curent.id);
            if(data != null)
            {
                curent.creator.ApplyData(data);
            }
        }
        if (curent.childs != null)
            foreach (var child in curent.childs)
                CreateObjectBasedOnHerarcy(child, curent);
    }

    public void LoadNewData(WorldData data)
    {
        foreach(Transform obj in worldParent)
            Destroy(obj.gameObject);

        sun.transform.eulerAngles = data.sunRotation;
        sun.transform.GetComponent<Light>().intensity = data.sunIntensity;
        sunIntensity.SetValueWithoutNotify(data.sunIntensity);
        objects = data.objects.ToList();

        foreach (var obj in objects)
            CreateObjectBasedOnHerarcy(obj, null);

        UpdateMenuList();
        SelectObject(objects[0]);
    }

    private void SetNewIdsForObject(ObjectsData obj)
    {
        int id = (new System.Random()).Next();
        while (FindObjectById(objects, id) != null)
            id = (new System.Random()).Next();
        obj.id = id;

        foreach (var c in obj.childs)
            SetNewIdsForObject(c);
    }

    private void CopyCharacterCreationData(ObjectsData original, ObjectsData destination)
    {
        if (original.creator != null)
        {
            var data = original.creator.GetCharacterData();
            destination.creator.ApplyData(data);
        }

        for(int i=0;i< original.childs.Count; i++)
        {
            CopyCharacterCreationData(original.childs[i], destination.childs[i]);
        }
    }

    public void DuplicateSelected()
    {
        if (lastObjectSelected != null)
        {
            var parent = FindObjectParentById(lastObjectSelected.id);

            var copy = lastObjectSelected.CreateCopy();
            SetNewIdsForObject(copy);

            if (parent != null)
                parent.childs.Add(copy);
            else objects.Add(copy);
            CreateObjectBasedOnHerarcy(copy, parent);

            CopyCharacterCreationData(lastObjectSelected, copy);

            UpdateMenuList();
        }
    }

    public void OnSelectedObjectToggle()
    {
        lastObjectSelected.isShown = selectedObjectToggle.isOn;
        lastObjectSelected.worldObject.gameObject.SetActive(selectedObjectToggle.isOn);
        UpdateMenuList();
    }
}
