using UnityEditor;
using UnityEngine;

/*
Using this class to make the Ground settings (like Shape and Color)
editable directly on the Ground object
*/

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    Ground ground;
    Editor shapeEditor, colorEditor;

    private void OnEnable()
    {
        ground = (Ground) target;
    }

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed) ground.GenerateGround();
        }
        
        // button to generate the ground
        if (GUILayout.Button("Manual Update"))
        {
            ground.GenerateGround();
        }

        DrawSettingsEditor(ground.shape_sets, ground.OnShapeUpdated, ref ground.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingsEditor(ground.color_sets, ground.OnColorUpdated, ref ground.colorSettingsFoldout, ref colorEditor);
    }

    private void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings==null) return;

        foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
        
        // we check for changes in the editor to visually apply those changes on the object
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            
            if (foldout)
            {
                // create the editor box inside the inspector
                CreateCachedEditor(settings, null, ref editor); 
                editor.OnInspectorGUI();

                if (check.changed && onSettingsUpdated!=null)
                {
                    onSettingsUpdated();
                }
            }
            
        }
    }
}
