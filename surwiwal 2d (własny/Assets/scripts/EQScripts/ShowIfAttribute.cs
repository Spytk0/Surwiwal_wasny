#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedObject obj = property.serializedObject;

        bool show = obj.FindProperty(showIf.propertyName).enumValueIndex == (int)showIf.value;

        if (show)
            EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedObject obj = property.serializedObject;

        bool show = obj.FindProperty(showIf.propertyName).enumValueIndex == (int)showIf.value;

        return show ? EditorGUI.GetPropertyHeight(property) : 0;
    }
}
#endif

[System.AttributeUsage(System.AttributeTargets.Field)]
public class ShowIfAttribute : PropertyAttribute
{
    public string propertyName;
    public ItemType value;

    public ShowIfAttribute(string propertyName, ItemType value)
    {
        this.propertyName = propertyName;
        this.value = value;
    }
}
