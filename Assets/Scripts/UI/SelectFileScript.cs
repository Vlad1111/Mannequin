using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class SelectFileScript : MonoBehaviour
{
    public const string NoMetaFile = "*(?<!.meta)";
    public const string MetaFile = "*.meta";
    public const string PngFile = "*.png";
    public const string JpegFile = "*.(jpg|jpeg|jfif|pjpeg|pjp)";
    public const string WebPFile = "*.webp";
    public const string Mp4File = "*.mp4";
    public const string GifFile = "*.gif";
    public const string WebmFile = "*.webm";
    public const string ObjFile = "*.obj";
    public const string FbxFile = "*.fbx";

    private static SelectFileScript Instance;
    private void Awake()
    {
        Instance = this;
    }

    private const string __backFolder = "../";
    private static readonly string[] __folderPermission = new[]{
        Permission.ExternalStorageRead,
        Permission.ExternalStorageWrite
    };

    public bool showFilesToo = false;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI folderLocationText;
    public TextButton lineButtonPrefab;
    public Transform folderListParent;
    private static string[] fileExtensions = new[] { ".*(?<!\\.meta)$" };
    private List<string> folderLocation;
    private string fullPath = null;
    private Action<string> action;

    private static void ChackeFolderPermission()
    {
        foreach (string permission in __folderPermission)
        {
            //if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }

        //if (!.OS.Environment.IsExternalStorageManager)
        //{
        //    Intent intent = new Intent(
        //    Android.Provider.Settings.ActionManageAppAllFilesAccessPermission,
        //    Android.Net.Uri.Parse("package:" + Application.Context.PackageName));
        //
        //    intent.AddFlags(ActivityFlags.NewTask);
        //
        //    Application.Context.StartActivity(intent);
        //}
    }

    private void UpdateFolderLocationText()
    {
        if (folderLocation == null)
            SetOriginalFolder(fullPath);
        if (folderLocation.Count > 0)
        {
            folderLocation[0] = folderLocation[0] + "/";
            fullPath = string.Join('/', folderLocation);
            folderLocation[0] = folderLocation[0].Substring(0, folderLocation[0].Length - 1);
        }
        else fullPath = "";
        if(Directory.Exists(fullPath))
            folderLocationText.text = "Path: " + fullPath;
        else
            folderLocationText.text = "File: " + fullPath;
    }

    public static bool IsCorrectFileType(string name)
    {
        foreach (var reg in fileExtensions)
            if (Regex.IsMatch(name.ToLower(), reg))
                return true;
        return false;
    }

    public static bool IsFileOfType(string name, string extension)
    {
        extension = WildCardToRegular(extension);
        return Regex.IsMatch(name.ToLower(), extension);
    }

    public static void CancelAnyInstane()
    {
        if (Instance)
            Instance.Cancel();
    }

    public void UpdateList()
    {
        ChackeFolderPermission();
        UpdateFolderLocationText();
        foreach (Transform child in folderListParent)
            Destroy(child.gameObject);

        List<string> allFolders = new List<string>() { };
        List<string> allFiles = new List<string>() { };
        try
        {
            if (folderLocation.Count > 0)
            {
                if (Application.platform != RuntimePlatform.Android || folderLocation.Count > 4)
                    allFolders.Add(__backFolder);
                if (Directory.Exists(fullPath))
                {
                    var folders = Directory.GetDirectories(fullPath).Select(x => x.Split('/').Last().Split('\\').Last());
                    allFolders.AddRange(folders);
                }
            }
            else
            {
                allFolders.AddRange(DriveInfo.GetDrives().Select(x => x.Name.Substring(0, x.Name.Length - 1)));
            }

            if (showFilesToo && folderLocation.Count > 0)
            {
                if(Directory.Exists(fullPath))
                {
                    allFiles = Directory.GetFiles(fullPath)
                          .Select(x => x.Split('/').Last().Split('\\').Last()).ToList();
                    if (fileExtensions != null && fileExtensions.Length > 0)
                        allFiles = allFiles.Where(IsCorrectFileType).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            folderLocationText.text = "error: " + ex.GetType() + " " + ex.Message + " " + Directory.Exists(fullPath);
        }
        foreach (string folder in allFolders)
        {
            var button = Instantiate(lineButtonPrefab, folderListParent);
            button.SetText("\t\U0001F5C0 " + folder);
            button.onClick.AddListener(() => { AddNextFolder(folder); });
        }

        foreach (string file in allFiles)
        {
            var button = Instantiate(lineButtonPrefab, folderListParent);
            button.SetText("   \U0001F5BB " + file);
            button.onClick.AddListener(() => { AddNextFolder(file); });
        }
    }

    private void AddNextFolder(string folder)
    {
        if (folder == __backFolder)
        {
            folderLocation.RemoveAt(folderLocation.Count - 1);
        }
        else
        {
            folderLocation.Add(folder);
            if (fullPath.EndsWith("/storage/emulated/0/Android") && folder == "data")
            {
                folderLocation = Application.persistentDataPath.Split('/').ToList();
            }
        }
        UpdateList();
    }

    // If you want to implement both "*" and "?"
    private static String WildCardToRegular(String value)
    {
        return "^" + value.Replace(".", "\\.").Replace("?", ".").Replace("*", ".*") + "$";
    }

    public static string[] GetAllAcceptedExtensions()
    {
        return new[]
            {
                PngFile,
                JpegFile,
                WebPFile,
                ObjFile,
                FbxFile,
            };
    }

    public static string[] GetAllAcceptedExtensionsForImages()
    {
        return new[]
            {
                PngFile,
                JpegFile,
                WebPFile,
            };
    }

    public static string[] GetAllAcceptedExtensionsForMeshes()
    {
        return new[]
            {
                ObjFile,
                FbxFile,
            };
    }

    void Start()
    {
        Pick(
            "Select a file",
            action,
            showFile: true,
            fileExtensionn: GetAllAcceptedExtensions()
        );
    }

    public void SetOriginalFolder(string folder = null)
    {
        string originalPath = folder;
        if (originalPath == null || originalPath == "")
        {
            if (Application.platform != RuntimePlatform.Android)
                originalPath = Application.dataPath;
            else
                originalPath = "/storage/emulated/0";
        }
        folderLocation = originalPath.Split('/').ToList();
    }

    public static void Pick(string title, Action<string> action, string originalFolder = null, bool showFile = true, string[] fileExtensionn = null)
    {
        if (Instance == null)
        {
            var obj = GameObject.FindObjectOfType(typeof(SelectFileScript), true) as SelectFileScript;
            if (obj == null)
            {
                Debug.LogWarning("No SelectFolderScript is present in this scene");
                return;
            }
            Instance = obj;
        }
        Instance.action = action;
        Instance.showFilesToo = showFile;
        if(Instance.titleText != null)
            Instance.titleText.text = title;
        fileExtensions = fileExtensionn == null ? GetAllAcceptedExtensions() : fileExtensionn.Select(x => WildCardToRegular(x)).ToArray();
        if (originalFolder != null && originalFolder != null)
            Instance.SetOriginalFolder(originalFolder);

        Instance.gameObject.SetActive(true);
        Instance.UpdateList();
    }

    public void Select()
    {
        if (action != null)
            action.Invoke(fullPath);
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }
}
