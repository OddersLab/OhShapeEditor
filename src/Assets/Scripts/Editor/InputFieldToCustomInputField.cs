using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using UnityEditor.SceneManagement;

public class InputFieldToCustomInputField : Editor
{
    [MenuItem("Tools/Input Field to Custom Field")]
    public static void UpdateInputFields()
    {
        InputField[] inputfields = Resources.FindObjectsOfTypeAll(typeof(InputField)) as InputField[];        

        foreach (InputField inputfield in inputfields)
        {
            InputField source = inputfield;
            GameObject go = inputfield.gameObject;
            DestroyImmediate(inputfield);
            CustomInputField customInputField = go.AddComponent<CustomInputField>();
            UpdateForType(typeof(InputField), source, customInputField);
            customInputField.colors = inputfield.colors;            
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        bool saveOK = EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Saved Scene " + (saveOK ? "OK" : "Error!"));
    }

    private static void UpdateForType(Type type, UnityEngine.Object source, UnityEngine.Object destination)
    {
        FieldInfo[] myObjectFields = type.GetFields(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo fi in myObjectFields)
        {
            fi.SetValue(destination, fi.GetValue(source));
        }
    }
}
