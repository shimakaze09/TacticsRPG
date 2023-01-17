using UnityEngine;
using UnityEditor;

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
        if (GUILayout.Button("Create Base"))
            current.CreateBase();

        if (GUI.changed)
            current.UpdateMarker();

        var e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftArrow:
                        current.pos += new Point(-1, 0);
                        break;
                    case KeyCode.RightArrow:
                        current.pos += new Point(1, 0);
                        break;
                    case KeyCode.UpArrow:
                        current.pos += new Point(0, 1);
                        break;
                    case KeyCode.DownArrow:
                        current.pos += new Point(0, -1);
                        break;

                    case KeyCode.J:
                        current.Grow();
                        break;
                    case KeyCode.K:
                        current.Shrink();
                        break;
                }

                current.UpdateMarker();
                break;
            }
        }
    }
}