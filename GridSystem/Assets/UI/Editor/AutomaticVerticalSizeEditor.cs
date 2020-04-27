using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutomaticVerticalSize))]
public class NewBehaviourScript : Editor
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
