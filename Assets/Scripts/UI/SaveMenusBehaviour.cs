using Dummiesman;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class SaveMenusBehaviour : MonoBehaviour
{
    public static SaveMenusBehaviour Instance;
    private void Awake()
    {
        Instance = this;
    }

    public const string FileExtensions = ".json";
    public const string TypeFolder = "Types";
    public const string WorldFolder = "Worlds";
    public const string WorldObjectsFolder = "Objects";
    public const string WorldObjectsDateFile = "Data";
    public const string ImportedObjectsFolder = "Imported objects";
    private string root = null;

    public TextMeshProUGUI debug;
    public Transform typeMenuParent;
    public Transform typeListParent;
    public TextMeshProUGUI typeListActionText;
    private bool isForLoadType = true;
    public TMP_InputField bodyTypeName;

    public TextMeshProUGUI worldListTitleAction;
    public Transform loadWorldParent;
    public Transform loadWorldListParent;
    public Transform loadWorldOptionsParent;
    public TMP_InputField worldName;

    private EditMenuBehaviour editMenu;
    private WorldMenuBehaviour worldMenu;

    void Start()
    {
        if (Application.isMobilePlatform)
            root = Application.persistentDataPath + "/SavedData";
        else
            root = Application.dataPath + "/../SavedData";
        if (!Directory.Exists(root))
            Directory.CreateDirectory(root);
        editMenu = EditMenuBehaviour.Instance;
        worldMenu = WorldMenuBehaviour.Instance;


        //if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        //{
        //    // The user authorized use of the microphone.
        //}
        //else
        //{
        //    bool useCallbacks = false;
        //    if (!useCallbacks)
        //    {
        //        // We do not have permission to use the microphone.
        //        // Ask for permission or proceed without the functionality enabled.
        //        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        //    }
        //    else
        //    {
        //        var callbacks = new PermissionCallbacks();
        //        //callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
        //        //callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        //        //callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
        //        Permission.RequestUserPermission(Permission.ExternalStorageWrite, callbacks);
        //    }
        //}
        //Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        //Permission.RequestUserPermission(Permission.ExternalStorageRead);
    }

    public string GetSaveFileLocation() => root;
    public string[] GetCurentPrefabTypes(string prefabName = null)
    {
        if (worldMenu.seletedNameInput == null)
            return new string[] { };
        if (prefabName == null)
            prefabName = worldMenu.lastObjectSelected.prefabName;
        var folder = root + "/" + TypeFolder  + "/" + prefabName;
        if(!Directory.Exists(folder))
            return new string[] { };
        var files = Directory.GetFiles(folder);
        debug.text = string.Join("\n", files);
        return files.Select(x => x.Split('\\').Last().Split('/').Last().Split('.')[0]).ToArray();
    }

    public void SaveCurentObjectAsPrefabType()
    {
        if (worldMenu.lastObjectSelected.creator == null)
            return;
        if (worldMenu.lastObjectSelected.creator.sliderValues.Count == 0)
            return;

        if(worldMenu == null)
            worldMenu = WorldMenuBehaviour.Instance;
        var values = GENERAL.GetObjectSliderValues(worldMenu.lastObjectSelected.creator, false);
        var json = JsonUtility.ToJson(new DictionaryKeyValues<string, float>(values));

        var folder = root + "/" + TypeFolder + "/" + worldMenu.lastObjectSelected.prefabName;
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        var file = folder + "/" + bodyTypeName.text + FileExtensions;
        //if(!File.Exists(file))
        //    File.Create(file);
        File.WriteAllText(file, json);
    }

    public void ShowExistingTypeForLoad()
    {
        typeListActionText.text = "Load";
        isForLoadType = true;
        ShowExistingTypes();
    }

    public void ShowExistingTypeForDelete()
    {
        typeListActionText.text = "Delete";
        isForLoadType = false;
        ShowExistingTypes();
    }

    public void ShowExistingTypes()
    {
        typeMenuParent.gameObject.SetActive(true);
        foreach (Transform item in typeListParent)
            Destroy(item.gameObject);

        var types = GetCurentPrefabTypes();
        if(types == null || types.Length == 0)
        {
            var prefPath = worldMenu.lastObjectSelected.prefabName;
            var prefName = prefPath.Split('/').Last().Split('\\').Last();

            prefName = worldMenu.FindTheNewPrefabPath(prefName);
            if(prefName != prefPath)
                types = GetCurentPrefabTypes(prefName);
        }
        if (isForLoadType)
            if (worldMenu.lastObjectSelected.creator?.defaultPrefabs?.Length > 0)
            {
                var line = Instantiate(worldMenu.objectUiPrefab, typeListParent);
                var text = line.GetComponent<TextButton>();
                text.text.text = "::: default :::";
                text.text.alignment = TextAlignmentOptions.Center;
                foreach (var type in worldMenu.lastObjectSelected.creator.defaultPrefabs)
                {
                    line = Instantiate(worldMenu.objectUiPrefab, typeListParent);
                    text = line.GetComponent<TextButton>();
                    text.text.text = type.key;
                    text.text.alignment = TextAlignmentOptions.Center;
                    var typeToLoad = type;
                    text.onClick.AddListener(() =>
                    {
                        //if (isForLoadType)
                        LoadDefoultType(typeToLoad.value, typeToLoad.key);
                        //else DeleteType(type);
                        typeMenuParent.gameObject.SetActive(false);
                    });
                }

                line = Instantiate(worldMenu.objectUiPrefab, typeListParent);
                text = line.GetComponent<TextButton>();
                text.text.text = "::: custume :::";
                text.text.alignment = TextAlignmentOptions.Center;
            }

        foreach (var type in types)
        {
            var line = Instantiate(worldMenu.objectUiPrefab, typeListParent);
            var text = line.GetComponent<TextButton>();
            text.text.text = type;
            text.text.alignment = TextAlignmentOptions.Center;
            text.onClick.AddListener(() =>
            {
                if(isForLoadType)
                    LoadType(type);
                else DeleteType(type);
                typeMenuParent.gameObject.SetActive(false);
            });
        }
    }

    public void HideExistingTypes()
    {
        typeMenuParent.gameObject.SetActive(false);
    }

    public void LoadDefoultType(DictionaryKeyValue<string, float>[] values, string name)
    {
        var slidersValues = new Dictionary<string, float>();
        var originalSliders = worldMenu.lastObjectSelected.creator.sliderValues;
        foreach (var value in values)
        {
            slidersValues.Add(value.key, value.value);
        }
        foreach (var kv in originalSliders)
        {
            if (worldMenu.lastObjectSelected.creator.poseSliderDatas.FirstOrDefault(
                x => x.name == kv.Key) != null)
                slidersValues.Add(kv.Key, kv.Value);
        }
        worldMenu.lastObjectSelected.creator.sliderValues = slidersValues;
        //worldMenu.lastObjectSelected.creator.ApplySliders();
        editMenu.ResetSliders();
        bodyTypeName.SetTextWithoutNotify(name);
    }

    public void LoadType(string name)
    {
        if (worldMenu.lastObjectSelected == null) return;

        var folder = root + "/" + TypeFolder + "/" + worldMenu.lastObjectSelected.prefabName;
        var file = folder + "/" + name + FileExtensions;
        if (!File.Exists(file))
        {
            Debug.Log("Load failed. Trying new path");
            var prefPath = worldMenu.lastObjectSelected.prefabName;
            var prefName = prefPath.Split('/').Last().Split('\\').Last();

            prefName = worldMenu.FindTheNewPrefabPath(prefName);
            Debug.Log("New path: " + prefName);
            if (prefName != prefPath)
            {
                worldMenu.lastObjectSelected.prefabName = prefName;
                LoadType(name);
            }
            return;
        }

        var json = File.ReadAllText(file);
        var keyValues = JsonUtility.FromJson<DictionaryKeyValues<string, float>>(json).keyValues;
        var slidersValues = new Dictionary<string, float>();
        foreach(var kv in keyValues)
            slidersValues.Add(kv.key, kv.value);

        foreach (var kv in keyValues)
            Debug.Log(kv.key + " " + kv.value);
        var originalSliders = worldMenu.lastObjectSelected.creator.sliderValues;
        foreach (var kv in originalSliders)
        {
            if (worldMenu.lastObjectSelected.creator.poseSliderDatas.FirstOrDefault(
                x => x.name == kv.Key) != null)
                slidersValues.Add(kv.Key, kv.Value);
        }
        worldMenu.lastObjectSelected.creator.sliderValues = slidersValues;
        //worldMenu.lastObjectSelected.creator.ApplySliders();
        editMenu.ResetSliders();
        bodyTypeName.SetTextWithoutNotify(name);
    }

    internal void DeletePathFromSave(string path)
    {
        path = root + "/" + path;
        if(Directory.Exists(path))
            Directory.Delete(path, true);
    }

    public void DeleteType(string name)
    {
        var folder = root + "/" + TypeFolder + "/" + worldMenu.lastObjectSelected.prefabName;
        var file = folder + "/" + name + FileExtensions;
        if (!File.Exists(file))
            return;

        File.Delete(file);
    }

    public void HideWorldList()
    {
        loadWorldParent.gameObject.SetActive(false);
    }

    public void ToggleWoldOptions(bool active)
    {
        loadWorldOptionsParent.gameObject.SetActive(active);
    }

    public void ShowWorldList()
    {
        //SwitchToLoadWorld();
        loadWorldParent.gameObject.SetActive(true);
        foreach (Transform o in loadWorldListParent)
            Destroy(o.gameObject);
        var list = GetWorldList();
        foreach (var world in list)
        {
            var line = Instantiate(worldMenu.objectUiPrefab, loadWorldListParent.transform);
            var text = line.GetComponent<TextButton>();
            text.text.text = world;
            text.text.alignment = TextAlignmentOptions.Center;

            text.onClick.AddListener(() => {
                worldListTitleAction.text = world;
                worldName.text = world;
                ToggleWoldOptions(true);
                foreach(Transform otherWorldElement in loadWorldListParent)
                    if(otherWorldElement != line.transform)
                    {
                        var otherLine = otherWorldElement.GetComponent<TextButton>();
                        if (otherLine != null)
                            otherLine.DeSelect();
                    }
            });
        }
    }

    public void SaveWorld()
    {
        HideWorldList();

        var folder = root + "/" + WorldFolder + "/" + worldName.text;
        SaveWorldToFolder(folder);
    }

    public void SaveWorldToOutsideFolder()
    {
        HideWorldList();

        if(Application.isMobilePlatform)
        {
            string folder = string.Format("storage/emulated/0/Manequine", Application.persistentDataPath);
            SaveWorldToFolder(folder);
        }
    }

    private void SaveWorldToFolder(string folder)
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
        Directory.CreateDirectory(folder);

        var data = worldMenu.GetWorldData();
        if (data.objects.Length == 0)
            return;
        var json = JsonUtility.ToJson(data);

        var dataFile = folder + "/" + WorldObjectsDateFile + FileExtensions;
        File.WriteAllText(dataFile, json);

        var objectsFolder = folder + "/" + WorldObjectsFolder;
        Directory.CreateDirectory(objectsFolder);

        Queue<WorldMenuBehaviour.ObjectsData> dataToSave = new Queue<WorldMenuBehaviour.ObjectsData>();
        foreach (var obj in data.objects)
            dataToSave.Enqueue(obj);
        while (dataToSave.Count > 0)
        {
            var obj = dataToSave.Dequeue();

            if (obj.creator == null)
                continue;
            var objData = obj.creator.GetCharacterData();

            json = JsonUtility.ToJson(objData);
            var objFolder = objectsFolder + "/" + obj.id + FileExtensions;
            File.WriteAllText(objFolder, json);
            foreach (var obj_c in obj.childs)
                dataToSave.Enqueue(obj_c);
        }
    }

    public void LoadWorld()
    {
        //worldListTitleAction.text = "Select to Load";
        //isForLoadWorld = true;
        LoadWorld(worldName.text);
    }

    public void DelteWorld()
    {
        //worldListTitleAction.text = "Select to Delete";
        //isForLoadWorld = false;
        DeleteWorld(worldName.text);
    }

    private string[] GetWorldList()
    {
        var folder = root + "/" + WorldFolder;
        if (!Directory.Exists(folder))
            return new string[] { };
        return Directory.GetDirectories(folder).Select(x => x.Split('\\').Last().Split('/').Last()).ToArray();
    }

    //private void WorldSeleted(string name)
    //{
    //    HideWorldList();
    //    if (isForLoadWorld)
    //        LoadWorld(name);
    //    else DeleteWorld(name);
    //}

    private void LoadWorld(string name)
    {
        var folder = root + "/" + WorldFolder + "/" + name;
        if (!Directory.Exists(folder))
            return;
        
        var dataFile = folder + "/" + WorldObjectsDateFile + FileExtensions;
        if (!File.Exists(dataFile))
            return;
        var json = File.ReadAllText(dataFile);
        var data = JsonUtility.FromJson<WorldMenuBehaviour.WorldData>(json);

        worldName.SetTextWithoutNotify(name);
        worldMenu.LoadNewData(data);
    }

    public CharacterCreation.CharacterData LoadCaracterData(int id)
    {
        var folder = root + "/" + WorldFolder + "/" + worldName.text;
        var objectsFolder = folder + "/" + WorldObjectsFolder + "/" + id + FileExtensions;
        if (!File.Exists(objectsFolder))
            return null;

        var json = File.ReadAllText(objectsFolder);
        return JsonUtility.FromJson<CharacterCreation.CharacterData>(json);
    }

    private void DeleteWorld(string name)
    {
        var folder = root + "/" + WorldFolder + "/" + name;
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
    }

    public void SaveObject<T>(T obj, string folderPath, string fileName)
    {
        var folder = root + "/" + folderPath;
        SaveObjectToExtern(obj, folder, fileName);
    }
    public void SaveObjectToExtern<T>(T obj, string folderPath, string fileName)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        folderPath += "/" + fileName + FileExtensions;
        var json = JsonUtility.ToJson(obj);
        File.WriteAllText(folderPath, json);
    }

    public T LoadObject<T>(string folderPath, string fileName)
    {
        if (root == null)
            Start();
        var folder = root + "/" + folderPath;
        return LoadObjectFromExtern<T>(folder, fileName);
    }

    public T LoadObjectFromExtern<T>(string folderPath, string fileName)
    {
        if (!Directory.Exists(folderPath))
            return default;
        folderPath += "/" + fileName + FileExtensions;
        if (!File.Exists(folderPath))
            return default;
        var json = File.ReadAllText(folderPath);
        return JsonUtility.FromJson<T>(json);
    }

    public Texture2D LoadPNG(string filePath, string fromSaveFolder = null)
    {
        if(fromSaveFolder != null)
        {
            if (root == null)
                Start();
            filePath = root + "/" + ImportedObjectsFolder + "/" + fromSaveFolder + "/" + filePath;
        }
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public GameObject LoadObjAsGameObject(string filePath, string fromSaveFolder = null)
    {
        if (fromSaveFolder != null)
        {
            if (root == null)
                Start();
            filePath = root + "/" + ImportedObjectsFolder + "/" + fromSaveFolder + "/" + filePath;
        }

        var mesh = new OBJLoader().Load(filePath);
        return mesh;
    }

    public void SaveTexture2DToLocalSavefiles(Texture2D t, string name, int id)
    {
        if (root == null)
            Start();
        var folder = root + "/" + ImportedObjectsFolder + "/" + id;
        if(!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        var bytes = t.EncodeToPNG();
        var file = folder + "/" + name;
        if (!name.Contains('.'))
            file += ".png";
        File.WriteAllBytes(file, bytes);
    }

    public void SaveMeshToLocalObjFiles(Mesh m, string name, int id)
    {
        SaveObject<MyMeshData>(m, ImportedObjectsFolder + "/" + id, name);
    }

    public Mesh LoadMeshToLocalObjFiles(string name, int id)
    {
        var mesh = LoadObject<MyMeshData>(ImportedObjectsFolder + "/" + id, name);
        return mesh;
    }
}
