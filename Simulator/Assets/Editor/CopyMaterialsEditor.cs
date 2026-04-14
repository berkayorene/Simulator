using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CopyMaterialsEditor : EditorWindow
{
    GameObject sourceObject;
    GameObject targetObject;

    [MenuItem("Tools/Copy Materials Tool")]
    public static void ShowWindow()
    {
        CopyMaterialsEditor window = GetWindow<CopyMaterialsEditor>();
        window.titleContent = new GUIContent("Copy Materials Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label(" Material Copier Tool", EditorStyles.boldLabel);

        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);

        if (GUILayout.Button("Copy Materials"))
        {
            HandleCopyMaterials();
        }
    }

    void HandleCopyMaterials()
    {
        if (AreObjectsAreNull(sourceObject, targetObject))
        {
            Debug.LogError("Source veya Target obje atanmadı!");
            return;
        }

        List<Transform> sourceList = GetAllChildren(sourceObject.transform);
        List<Transform> targetList = GetAllChildren(targetObject.transform);

        foreach (Transform source in sourceList)
        {
            foreach (Transform target in targetList)
            {
                if (IsMatchByName(source.name, target.name))
                {
                    CopyMaterials(source, target);

                }
            }
        }



        /*
        CopyMaterials(sourceObject.transform, targetObject.transform);
        HandleChildMaterials(sourceObject.transform, targetObject.transform);*/
    }

    void CopyMaterials(Transform sourceObject, Transform targetObject)
    {

        Renderer sourceRenderer = sourceObject.GetComponent<Renderer>();
        Renderer targetRenderer = targetObject.GetComponent<Renderer>();

        if (sourceRenderer == null || targetRenderer == null)
        {
            Debug.LogError("Renderer bulunamadı!");
            return;
        }

        Undo.RecordObject(targetRenderer, "Copy Materials");
        targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
        EditorUtility.SetDirty(targetRenderer);

        Debug.Log("Materyaller başarıyla kopyalandı!");

    }

    /*
    void HandleChildMaterials(Transform parentSourceObject, Transform parentTargetObject)
    {
        if (parentSourceObject.childCount > 0 && parentTargetObject.childCount > 0)
        {
            <CopyMaterialsOfChildObjects>(sourceObject.transform, targetObject.transform);
        }
        else
        {
            return;
        }

        for(int i = 0; i < Mathf.Min(parentSourceObject.childCount, parentTargetObject.childCount); i++)
        {
            HandleChildMaterials(parentSourceObject.GetChild(i), parentTargetObject.GetChild(i));
        }
        
    }



    

    void CopyMaterialsOfChildObjects(Transform parentSourceObject, Transform parentTargetObject)
    {
        for (int i = 0; i < parentSourceObject.childCount; i++)
        {
            Transform childOfSourceObject = parentSourceObject.GetChild(i);
            for (int j = 0; j < parentTargetObject.childCount; j++)
            {
                Transform childOfTargetObject = parentTargetObject.GetChild(j);
                CopyMaterials(childOfSourceObject, childOfTargetObject);
            }

        }
    }*/

    bool AreObjectsAreNull(GameObject sourceObject, GameObject targetObject)
    {
        if (sourceObject == null || targetObject == null)
        {
            Debug.LogError("Source veya Target obje atanmadı!");
            return true;
        }
        return false;
    }





    List<Transform> GetAllChildren(Transform parent)
    {
        List<Transform> list = new List<Transform>();
        list.Add(parent); // Kendisini de dahil et

        foreach (Transform child in parent)
        {
            list.AddRange(GetAllChildren(child));
        }

        return list;
    }

    bool IsMatchByName(string sourceName, string targetName)
    {
        // Başlangıcı sourceName olan ve opsiyonel boşluk, rakam, parantez kabul et
        string pattern = $"^{Regex.Escape(sourceName)}(\\.)?d*(\\s*\\(?\\d*\\)?)?$";
        return Regex.IsMatch(targetName, pattern);
    }


    /*ilk hali
    void CopyMaterialss()
    {
        if (sourceObject == null || targetObject == null)
        {
            Debug.LogError("Source veya Target obje atanmadı!");
            return;
        }

        for (int i = 0; i < sourceObject.transform.childCount; i++)
        {
            Transform childOfSourceObject = sourceObject.transform.GetChild(i);
            for (int j = 0; j < targetObject.transform.childCount; j++)
            {
                Transform childOfTargetObject = targetObject.transform.GetChild(j);

                if (childOfSourceObject.name == childOfTargetObject.name)
                {
                    Renderer sourceRenderer = childOfSourceObject.GetComponent<Renderer>();
                    Renderer targetRenderer = childOfTargetObject.GetComponent<Renderer>();

                    if (sourceRenderer == null || targetRenderer == null)
                    {
                        Debug.LogError("Renderer bulunamadı!");
                        return;
                    }

                    Undo.RecordObject(targetRenderer, "Copy Materials");
                    targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                    EditorUtility.SetDirty(targetRenderer);

                    Debug.Log("Materyaller başarıyla kopyalandı!");

                }

            }

        }
    }*/



}