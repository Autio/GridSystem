using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutomaticVerticalSize))]
public class AutomaticVerticalSizeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if( GUILayout.Button("Recalc size") )
        {
            ((AutomaticVerticalSize)target).AdjustSize();
        }

    }
}
