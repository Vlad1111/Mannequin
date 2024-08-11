using RuntimeHandle;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImportedObjectsBehaviour : MonoBehaviour
{
    private static ImportedObjectsBehaviour _instance;
    public static ImportedObjectsBehaviour Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(ImportedObjectsBehaviour), true) as ImportedObjectsBehaviour;
                if (_instance == null)
                {
                    Debug.LogWarning("No ImportedObjectsBehaviour is present in this scene");
                    return null;
                }
            }
            return _instance;
        }
        set => _instance = value; 
    }

    [System.Serializable]
    public enum ObjectDataType
    {
        Image = 0,
        Mesh = 1
    }
    [System.Serializable]
    public class ObjectData
    {
        public string name;
        public int id;
        public ObjectDataType type;
        public string originalPath;
        public string fileExtension;
        public string[] componentParts;

        public override string ToString()
        {
            return "Imported object " + name;
        }
    }
    [System.Serializable]
    public class ObjectsData
    {
        public ObjectData[] datas;
    }

    private const string objectsListFileName = "_objects";

    public Transform importedObjectPrefab;
    public Sprite notFoundImageSprite;
    public Transform importedImagesParent;
    public Transform importedMeshesParent;

    public Transform editObjectParent;
    public TMP_InputField editedNameInput;
    public Image editedSpriteThumbnail;
    public Transform deletePropmtTransform;

    public static List<ObjectData> objects = new List<ObjectData>();

    private int lastSelectedItemIndex = -1;

    private static void InstantiateSpriteObject(Transform obj, ObjectData objData)
    {
        //var sr = obj.gameObject.AddComponent<SpriteRenderer>();
        //if(sprite != null)
        //{
        //    sr.sprite = sprite.ToSprite();
        //    var col = obj.gameObject.AddComponent<BoxCollider>();
        //    col.size = new Vector3(sprite.width / 100f, sprite.height / 100f, 0.1f);
        //}
        //
        //obj.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        //obj.position = new Vector3(0, sprite.height / 400f, 0);

        var creator = obj.gameObject.AddComponent<CharacterCreation>();

        var child = new GameObject(obj.name).transform;
        child.parent = obj;

        var sprite = SaveMenusBehaviour.Instance.LoadPNG(objData.componentParts[0] + ".png", objData.id.ToString());
        if (sprite != null)
        {
            var col = obj.gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(1, 1, 0.1f);

            var mr = child.gameObject.AddComponent<MeshRenderer>();
            var material = new Material(MaterailScript.SpriteMaterial);
            material.SetTexture("_BaseMap", sprite);
            //material.SetTexture("_EmissionMap", sprite);
            mr.material = material;
            creator.characterMaterials = new[] { material };

            var mf = child.gameObject.AddComponent<MeshFilter>();
            mf.sharedMesh = MaterailScript.QuadMesh;

            obj.position = new Vector3(0, sprite.height / 400f, 0);
            obj.localScale = new Vector3(sprite.width / 200f, sprite.height / 200f, 1f);
            //obj.localEulerAngles = new Vector3(90f, 0f, 0f);
        }

        creator.sliderDatas = new[]
        {
            new CharacterCreation.SliderData()
            {
                name = "Alpha",
                negativeBones = new []
                {
                    new CharacterCreation.SliderData.SliderBoneData()
                    {
                        origin = child
                    }
                },
                marginValues = new Vector2(-1, 0),
                type = CharacterCreation.SliderData.SliderType.Sprite,
            }
        };
    }

    private static SkinnedMeshRenderer InstantiateMeshPartObjects(Transform parent, ObjectData objData, string part, Material baseMaterail)
    {
        var obj = new GameObject(part).transform;
        obj.parent = parent;

        var skinRendere = obj.gameObject.AddComponent<SkinnedMeshRenderer>();
        skinRendere.material = baseMaterail;
        var mesh = SaveMenusBehaviour.Instance.LoadMeshToLocalObjFiles(part, objData.id);
        if (mesh != null)
            skinRendere.sharedMesh = mesh;

        return skinRendere;
    }

    private static void InstantiateMeshObject(Transform obj, ObjectData objData)
    {
        var meshList = new List<SkinnedMeshRenderer>();

        var baseMaterial = new Material(MaterailScript.BaseMaterial);
        foreach (var part in objData.componentParts)
        {
            meshList.Add(InstantiateMeshPartObjects(obj, objData, part, baseMaterial));
        }

        var skinRendere = obj.gameObject.AddComponent<SkinnedMeshRenderer>();
        skinRendere.material = new Material(MaterailScript.SelectedMaterial);
        var mesh = SaveMenusBehaviour.Instance.LoadMeshToLocalObjFiles(objData.componentParts[0], objData.id);
        if (mesh != null)
        {
            skinRendere.sharedMesh = mesh;
            meshList.Add(skinRendere);
            var mc = obj.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }

        var creator = obj.gameObject.AddComponent<CharacterCreation>();
        creator.meshRenderers = meshList.ToArray();
    }

    public static Transform InstantiateObject(int id)
    {
        if (objects == null)
            return null;
        var inx = objects.FindIndex(x => x.id == id);
        if (inx < 0)
            return null;

        var newObj = new GameObject().transform;
        newObj.name = objects[inx].name;

        var restriction = newObj.gameObject.AddComponent<HandleTypeRestiction>();
        restriction.types = new[]
        {
            HandleType.POSITION,
            HandleType.ROTATION,
            HandleType.SCALE
        };

        if (objects[inx].type == ObjectDataType.Image)
            InstantiateSpriteObject(newObj, objects[inx]);
        else
            InstantiateMeshObject(newObj, objects[inx]);

        return newObj;
    }
    public void Start()
    {
        if (objects == null || objects.Count == 0)
            objects = SaveMenusBehaviour.Instance.
                LoadObject<ObjectsData>(SaveMenusBehaviour.ImportedObjectsFolder, objectsListFileName)?.datas?.ToList();
        if (objects == null)
            objects = new List<ObjectData>();
        UpdateShownList();
    }

    public void SaveList()
    {
        SaveMenusBehaviour.Instance.SaveObject(new ObjectsData() { datas = objects.ToArray() },
                                                SaveMenusBehaviour.ImportedObjectsFolder,
                                                objectsListFileName);
    }

    public void TurnOnOff(bool active)
    {
        gameObject.SetActive(active);
        if(!active)
            SelectFileScript.CancelAnyInstane();
        TurnEditObjectOff();
    }

    public void ImportNewObject()
    {
        SelectFileScript.Pick("Import a new file", ImportNewObject, showFile: true);
        //NativeFilePicker.PickFile(new NativeFilePicker.FilePickedCallback(ImportNewObject));
        TurnEditObjectOff();
    }

    private void ImportNewObject(string path)
    {
        if(Directory.Exists(path))
        {
            ImportNewObject(path + "/obj.obj");
            return;
        }
        else if (path.EndsWith(".obj"))
        {
            var meshObj = SaveMenusBehaviour.Instance.LoadObjAsGameObject(path);
            if (meshObj != null)
                AddNewImportedObjectToList(path, ObjectDataType.Mesh, meshObj);
        }
        else
        {
            var text2D = SaveMenusBehaviour.Instance.LoadPNG(path);
            if (text2D != null)
                AddNewImportedObjectToList(path, ObjectDataType.Image, text2D);
        }
        UpdateShownList();
    }
    private Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    public void AddNewImportedObjectToList(string path, ObjectDataType type, object obj)
    {
        var newObjData = new ObjectData();
        newObjData.name = path.Split('/').Last().Split('\\').Last().Split('.').First();
        newObjData.originalPath = path;
        newObjData.fileExtension = path.Split('.').Last();
        newObjData.type = type;
        newObjData.id = (int)(Random.value * int.MaxValue);
        while(objects.FirstOrDefault(x => x.id == newObjData.id) != null)
            newObjData.id = (int)(Random.value * int.MaxValue);

        if (type == ObjectDataType.Image)
        {
            var t2D = (Texture2D)obj;
            newObjData.componentParts = new[] { newObjData.name };
            SaveMenusBehaviour.Instance.SaveTexture2DToLocalSavefiles(t2D, newObjData.name, newObjData.id);
            t2D = Resize(t2D, 100, 100);
            SaveMenusBehaviour.Instance.SaveTexture2DToLocalSavefiles(t2D, "tn.jpg", newObjData.id);
        }
        else if (type == ObjectDataType.Mesh)
        {
            var go = (GameObject)obj;
            var meshes = go.GetComponentsInChildren<MeshFilter>();
            var meshesName = new List<string>();
            foreach (var mesh in meshes)
            {
                var meshName = string.Join("", mesh.name.Where(x => !(new[] { ':', '(', ')', '[', ']', '{', '}' }).Contains(x)));
                meshesName.Add(meshName);
                SaveMenusBehaviour.Instance.SaveMeshToLocalObjFiles(mesh.mesh, meshName, newObjData.id);
            }
            newObjData.componentParts = meshesName.ToArray();
            Destroy(go);
        }

        objects.Add(newObjData);
        SaveList();
    }

    public void ToggleImportedMenu(Transform menu)
    {
        menu.gameObject.SetActive(!menu.gameObject.activeSelf);
    }

    private void UpdateShownListGiven(Transform listParent, ObjectData[] list)
    {
        int inx = 0;
        for (; inx < list.Length; inx++)
        {
            TextButton txtButton = null;
            if (inx < listParent.childCount)
            {
                txtButton = listParent.GetChild(inx).GetComponent<TextButton>();
                if (txtButton == null)
                    Destroy(listParent.GetChild(inx));
            }
            if(txtButton == null)
            {
                txtButton = Instantiate(importedObjectPrefab, listParent).GetComponent<TextButton>();
            }
            if(txtButton.GetText() != list[inx].name)
            {
                txtButton.SetText(list[inx].name);
                var texture = SaveMenusBehaviour.Instance.LoadPNG("tn.jpg", list[inx].id.ToString());
                var sprite = notFoundImageSprite;
                if (texture != null)
                    sprite = texture.ToSprite();
                txtButton.SetButtonImage(sprite);

                int newI = inx;
                txtButton.onClick.AddListener(() => { SelectItem(list[newI].id, sprite); });
            }
        }
        for(; inx < listParent.childCount; inx++)
        {
            Destroy(listParent.GetChild(inx).gameObject);
        }
    }

    public void UpdateShownList()
    {
        //if (importedImagesParent)
        //    foreach (Transform p in importedImagesParent)
        //        Destroy(p.gameObject);
        //if (importedMeshesParent)
        //    foreach (Transform p in importedMeshesParent)
        //        Destroy(p.gameObject);
        importedImagesParent.gameObject.SetActive(false);
        importedMeshesParent.gameObject.SetActive(false);
        var objs = objects.Where(x => x.type == ObjectDataType.Image).ToArray();
        UpdateShownListGiven(importedImagesParent, objs);
        objs = objects.Where(x => x.type == ObjectDataType.Mesh).ToArray();
        UpdateShownListGiven(importedMeshesParent, objs);
        importedImagesParent.gameObject.SetActive(true);
        importedMeshesParent.gameObject.SetActive(true);
    }

    public void SelectItem(int id, Sprite thumbnail = null)
    {
        editObjectParent.gameObject.SetActive(true);

        int inx = objects.FindIndex(x => x.id == id);
        lastSelectedItemIndex = inx;

        editedNameInput.text = objects[inx].name;
        if(thumbnail == null)
        {
            var texture = SaveMenusBehaviour.Instance.LoadPNG("tn.jpg", objects[inx].id.ToString());
        }
        editedSpriteThumbnail.sprite = thumbnail;
    }

    public void TurnEditObjectOff()
    {
        editObjectParent.gameObject.SetActive(false);
        deletePropmtTransform.gameObject.SetActive(false);
        UpdateShownList();
    }

    public void OnSelectedItemnameChange(TMP_InputField inputField)
    {
        if (lastSelectedItemIndex == -1)
            return;
        objects[lastSelectedItemIndex].name = inputField.text;
        SaveList();
    }

    public void AddSelectedObjectToWorld()
    {
        if (lastSelectedItemIndex != -1)
        {
            WorldMenuBehaviour.Instance.AddNewObjectFromImported(objects[lastSelectedItemIndex].id);
            TurnOnOff(false);
        }
    }

    public void DeleteSelectedFile()
    {
        var id = objects[lastSelectedItemIndex].id;
        objects.RemoveAt(lastSelectedItemIndex);
        SaveList();
        SaveMenusBehaviour.Instance.DeletePathFromSave(SaveMenusBehaviour.ImportedObjectsFolder + "/" + id);
        TurnEditObjectOff();
        UpdateShownList();
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    public void ExportSelected()
    {
        SelectFileScript.Pick("Select a folder to exposrt", ExportSelected, showFile: false);
    }

    private void ExportSelected(string path)
    {
        if (lastSelectedItemIndex == -1) return;
        var folder = SaveMenusBehaviour.Instance.GetSaveFileLocation() + "/" + SaveMenusBehaviour.ImportedObjectsFolder + "/" + objects[lastSelectedItemIndex].id;
        path = path + "/" + objects[lastSelectedItemIndex].name;
        CopyFilesRecursively(folder, path);
        SaveMenusBehaviour.Instance.SaveObjectToExtern(objects[lastSelectedItemIndex], path, "_data");
    }

    public void ImportFromExportedFolder()
    {
        SelectFileScript.Pick("Select a folder to import from", ImportFromExportedFolder, showFile: false);
    }
    private void ImportFromExportedFolder(string path)
    {
        var data = SaveMenusBehaviour.Instance.LoadObjectFromExtern<ObjectData>(path, "_data");
        if (data == null) return;

        bool found = false;
        for (int i = 0; i < objects.Count; i++)
            if (objects[i].id == data.id)
            {
                objects[i] = data;
                found = true;
            }
        if(!found)
            objects.Add(data);
        SaveList();

        var folder = SaveMenusBehaviour.Instance.GetSaveFileLocation() + "/" + SaveMenusBehaviour.ImportedObjectsFolder + "/" + data.id;
        //path = path + "/" + objects[lastSelectedItemIndex].name;
        CopyFilesRecursively(path, folder);

        UpdateShownList();
    }
}
