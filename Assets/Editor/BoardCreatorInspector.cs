using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BoardCreator))]
public class BoardCreatorInspector : Editor
{
    public BoardCreator current => (BoardCreator)target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Clear"))
            current.Clear();
        if (GUILayout.Button("Grow"))
            current.Grow();
        if (GUILayout.Button("Shrink"))
            current.Shrink();
        if (GUILayout.Button("Grow Area"))
            current.GrowArea();
        if (GUILayout.Button("Shrink Area"))
            current.ShrinkArea();
        if (GUILayout.Button("Save"))
            current.Save();
        if (GUILayout.Button("Load"))
            current.Load();

        if (GUI.changed)
            current.UpdateMarker();

        if (GUILayout.Button("Create Base"))
            current.CreateBase();


        var e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftArrow:
                        current.SetPos(current.GetPos() + new Point(-1, 0));
                        break;
                    case KeyCode.RightArrow:
                        current.SetPos(current.GetPos() + new Point(1, 0));
                        break;
                    case KeyCode.UpArrow:
                        current.SetPos(current.GetPos() + new Point(0, 1));
                        break;
                    case KeyCode.DownArrow:
                        current.SetPos(current.GetPos() + new Point(0, -1));
                        break;

                    case KeyCode.J:
                        current.Grow();
                        current.UpdateMarker();
                        break;
                    case KeyCode.K:
                        current.Shrink();
                        current.UpdateMarker();
                        break;
                }

                current.UpdateMarker();
                break;
            }
        }
    }
}