using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NetworkedPrefab
{
    public GameObject Prefab;
    public string Path;

    public NetworkedPrefab(GameObject obj, string path)
    {
        Prefab = obj;
        Path = ReturnPrefabPathModified(path);
        //Assets/Resources/File.prefab //7 ext length. 7 on resources.
        //Resources/File
    }

    private string ReturnPrefabPathModified(string path)
    {
        int extentionLength = System.IO.Path.GetExtension(path).Length; //확장명 (. 포함) 반환
        int additionalLength = 10;
        int startIndex = path.ToLower().IndexOf("resources");

        if (startIndex == -1)
            return string.Empty;
        else
            return path.Substring(startIndex + additionalLength, path.Length - (additionalLength + startIndex + extentionLength));
    }
}
