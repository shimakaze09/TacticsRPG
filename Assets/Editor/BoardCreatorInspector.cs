using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardCreator))]
public class BoardCreatorInspector : Editor
{
    public BoardCreator Current => (BoardCreator)target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Clear"))
            Current.Clear();
        if (GUILayout.Button("Grow"))
            Current.Grow();
        if (GUILayout.Button("Shrink"))
            Current.Shrink();
        if (GUILayout.Button("Grow Area"))
            Current.GrowArea();
        if (GUILayout.Button("Shrink Area"))
            Current.ShrinkArea();
        if (GUILayout.Button("Save"))
            Current.Save();
        if (GUILayout.Button("Load"))
            Current.Load();
        if (GUILayout.Button("Create Base"))
            Current.CreateBase();

        if (GUI.changed)
            Current.UpdateMarker();

        var e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.LeftArrow:
                        Current.pos += new Point(-1, 0);
                        break;
                    case KeyCode.RightArrow:
                        Current.pos += new Point(1, 0);
                        break;
                    case KeyCode.UpArrow:
                        Current.pos += new Point(0, 1);
                        break;
                    case KeyCode.DownArrow:
                        Current.pos += new Point(0, -1);
                        break;

                    case KeyCode.J:
                        Current.Grow();
                        break;
                    case KeyCode.K:
                        Current.Shrink();
                        break;
                }

                Current.UpdateMarker();
                break;
            }
        }
    }
}