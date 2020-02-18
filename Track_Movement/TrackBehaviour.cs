using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrackBehaviour : MonoBehaviour
{
    public enum PresetTracks
    {
        square, circle, simi_circle, none
    }

    #region Fields
    public List<Transform> Points;
    public List<TrackPoint> TracksPoints;
    public bool IsLoop;
    public PresetTracks UsePreset = PresetTracks.none;
    public float Radius;
    public float MaxAngle;
    public bool VisualTrack;    
    public float TrackWidth;    
    #endregion


    #region Unity Methods    
    private void OnDrawGizmos()
    {
        if (Points == null)
            return;
        for(int i = 0; i < Points.Count; i++)
        {            
            Gizmos.DrawLine(Points[i].position, i != Points.Count - 1 ? Points[i + 1].position : IsLoop ? Points[0].position : Points[i].position);
            if (TracksPoints[i].IsStoppingPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(Points[i].position, 0.5f);
                Gizmos.color = Color.white;
            }
        }
    }

    private void Update()
    {
        if (VisualTrack)
        {
            GetComponent<LineRenderer>().positionCount = Points.Count;
            for (int i = 0; i < Points.Count; i++)
            {
                GetComponent<LineRenderer>().SetPosition(i, Points[i].transform.position);
            }
        }
    }
    #endregion

    #region Implementation Methods
    /// <summary>
    /// Returns the point in the list that follows the point passed in.
    /// If the point passed in is the last point in the list if the track is looping the function will return the
    /// first node in the list otherwise the list will reverse
    /// </summary>
    /// <param name="currentPoint">Current point in the list we are looking at in the list</param>
    /// <returns>Next point in the track we want to go to</returns>
    public Transform GetNextPoint(Transform currentPoint, bool isRecrusive = false)
    {
        if (currentPoint == Points[Points.Count - 1])
        {
            if (IsLoop)
                return Points[0];
            else
                Points.Reverse();
        }

        if(VisualTrack && !isRecrusive)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].GetComponent<LineRenderer>().startWidth = TrackWidth;
                Points[i].GetComponent<LineRenderer>().endWidth = TrackWidth;
            }
        }
        return Points[Points.IndexOf(currentPoint) + 1];
    }

    private Transform GetPreviousPoint(Transform current)
    {
        if (current == Points[0])
        {
            if (IsLoop)
                return Points[Points.Count - 1];            
        }
        return Points[Points.IndexOf(current) - 1];
    }

    /// <summary>
    /// Finds the closest point on the track to vector passed in and returns it
    /// </summary>
    /// <param name="position">Point we are looking for a point in the track next to</param>
    /// <returns>Closest point in the track next to the argument passed in</returns>
    public Transform ClosestPoint(Vector3 position)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestTransform = null;
        foreach (var p in Points)
        {
            if (Vector3.Distance(p.position, position) < closestDistance)
            {
                closestTransform = p;
                closestDistance = Vector3.Distance(p.position, position);
            }
        }
        return closestTransform;
    }
    #endregion

#region Utility Methods
    public void ClearPath()
    {
        for(int i = 0; i < Points.Count; i++)
        {
            DestroyImmediate(Points[i].gameObject);
        }
        Points = new List<Transform>();
    }
#endregion
}

[System.Serializable]
public class TrackPoint
{
    public int PositionOnTrack;
    public float SpeedModifier = 1.0f;
    public bool IsStoppingPoints;
    public float StopDelay;
    public bool HasBeenReached = false;
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TrackBehaviour)), UnityEditor.CanEditMultipleObjects()]
public class TrackGeneration : UnityEditor.Editor
{
    #region Fields
    //Circle Presets
    float Radius;

    //Simi-Circle Presets
    float MaxAngle;

    //Square
    float Width, Height;    
    #endregion

    #region Unity Methods
    /// <summary>
    /// Overrides the way the component is drawn in the inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        TrackBehaviour t = target as TrackBehaviour;
        if (t.Points == null)
        {
            t.Points = new List<Transform>();
        }

        if(t.TracksPoints == null)
        {
            t.TracksPoints = new List<TrackPoint>();
            foreach(var p in t.Points)
            {
                t.TracksPoints.Add(new TrackPoint() { PositionOnTrack = t.Points.IndexOf(p), SpeedModifier = 1, HasBeenReached = false });
            }
        }
        if(GUILayout.Button("Clear Path"))
        {
            t.ClearPath();
        }

        t.VisualTrack = UnityEditor.EditorGUILayout.ToggleLeft("Show Tracks", t.VisualTrack);       
        if(t.VisualTrack)
        {
            t.TrackWidth = UnityEditor.EditorGUILayout.FloatField("Track Width", t.TrackWidth);
            if (t.TrackWidth < 0.01f)
                t.TrackWidth = 0.01f;
            //t.HasZipperEffect = UnityEditor.EditorGUILayout.ToggleLeft("Has Zipper Effect", t.HasZipperEffect);
        }
        UnityEditor.EditorGUILayout.LabelField("Preset Tracks");
        t.IsLoop = UnityEditor.EditorGUILayout.ToggleLeft("Is Looping", t.IsLoop);
        t.UsePreset = (TrackBehaviour.PresetTracks)UnityEditor.EditorGUILayout.EnumPopup(t.UsePreset);
        switch(t.UsePreset)
        {
            case TrackBehaviour.PresetTracks.none:

                break;
            case TrackBehaviour.PresetTracks.circle:
                t.Radius = UnityEditor.EditorGUILayout.FloatField("Circle Radius", t.Radius);
                if (GUILayout.Button("Generate Circle"))
                {
                    t.IsLoop = true;
                    t.ClearPath();
                    AssignPoints(GenerateRotationPoints(t.gameObject));
                }
                break;
            case TrackBehaviour.PresetTracks.simi_circle:                
                t.Radius = UnityEditor.EditorGUILayout.FloatField("Circle Radius", t.Radius);
                t.MaxAngle = UnityEditor.EditorGUILayout.FloatField("Simi-Circle Angle", t.MaxAngle);
                if (GUILayout.Button("Generate Simi-Circle"))
                {
                    t.IsLoop = false;
                    t.ClearPath();
                    AssignPoints(GenerateRotationPoints(t.gameObject, t.MaxAngle));
                }
                break;
            case TrackBehaviour.PresetTracks.square:                
                Width = UnityEditor.EditorGUILayout.FloatField("Width", Width);
                Height = UnityEditor.EditorGUILayout.FloatField("Height", Height);
                if(GUILayout.Button("Generate Square"))
                {
                    t.IsLoop = true;
                    t.ClearPath();
                    List<Vector3> points = new List<Vector3>() {
                    new Vector3(t.transform.position.x - (Width / 2), t.transform.position.y + (Height / 2), 0), //tL
                    new Vector3(t.transform.position.x - (Width / 2), t.transform.position.y - (Height / 2), 0), //bL                    
                    new Vector3(t.transform.position.x + (Width / 2), t.transform.position.y - (Height / 2), 0),//bR
                    new Vector3(t.transform.position.x + (Width / 2), t.transform.position.y + (Height / 2), 0) }; //tR
                    AssignPoints(points.ToArray());
                }
                break;
        }

        if (t.VisualTrack)
        {
            t.GetComponent<LineRenderer>().enabled = true;
            t.GetComponent<LineRenderer>().positionCount = t.Points.Count;
            for (int i = 0; i < t.Points.Count; i++)
            {                
                t.GetComponent<LineRenderer>().SetPosition(i, t.Points[i].transform.position);
                if (i == t.Points.Count - 1 && t.IsLoop)
                {
                    t.GetComponent<LineRenderer>().positionCount = t.Points.Count + 1;
                    t.GetComponent<LineRenderer>().SetPosition(i + 1, t.Points[0].transform.position);
                }
            }
            t.GetComponent<LineRenderer>().startWidth = t.TrackWidth;
            t.GetComponent<LineRenderer>().endWidth = t.TrackWidth;
        }
        else
        {
            if (t != null)
                t.GetComponent<LineRenderer>().enabled = false;
        }


        var selectedObjects = UnityEditor.Selection.gameObjects;
        foreach (var obj in selectedObjects)
        {
            if (t.Points.Contains(obj.transform))
            {
                GUILayout.Label(obj.name + "[Track Position: " + t.Points.IndexOf(obj.transform) + "]");                
                t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier = UnityEditor.EditorGUILayout.FloatField("Speed Modifier: ", 
                    t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier);
                t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier =
                    GUILayout.HorizontalSlider(t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier, 0.1f, 2.0f);
                t.TracksPoints[t.Points.IndexOf(obj.transform)].IsStoppingPoints =
                    GUILayout.Toggle(t.TracksPoints[t.Points.IndexOf(obj.transform)].IsStoppingPoints, "Is Stopping Point");
                if(t.TracksPoints[t.Points.IndexOf(obj.transform)].IsStoppingPoints)
                {
                    t.TracksPoints[t.Points.IndexOf(obj.transform)].StopDelay = 
                        UnityEditor.EditorGUILayout.FloatField("Stop Delay: ", t.TracksPoints[t.Points.IndexOf(obj.transform)].StopDelay);
                }
                if (t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier > 2)
                    t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier = 2;
                else if (t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier < 0.1)
                    t.TracksPoints[t.Points.IndexOf(obj.transform)].SpeedModifier = 0.1f;
            }
        }
        if (selectedObjects.Length > 1)
            if (GUILayout.Button("Merge Tracks"))
            {
                var selectedTracks = new List<TrackBehaviour>();
                foreach (var obj in selectedObjects)
                {
                    if (obj.GetComponent<TrackBehaviour>())
                    {
                        selectedTracks.Add(obj.GetComponent<TrackBehaviour>());
                    }
                }
                MergeTracks(selectedTracks.ToArray());
            }        
        foreach (var obj in selectedObjects)
        {
            if (obj.GetComponent<TrackBehaviour>())
            {
                if (obj.name != t.name)
                    GUILayout.Label(obj.name);
            }
        }


    }

    /// <summary>
    /// Extends the functionality of the scene view so that the user can draw paths without having to create the 
    /// objects in the inspector first then move them around
    /// </summary>
    private void OnSceneGUI()
    {
        TrackBehaviour t = target as TrackBehaviour;

        if (t == null)
            return;

        Event e = Event.current;

        switch (e.type)
        {
            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.P)
                {
                    Ray ray = UnityEditor.HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    if (t.Points.Count > 0)
                    {
                        if (Vector2.Distance(t.Points[t.Points.Count - 1].position, new Vector3(ray.origin.x, ray.origin.y, 0)) < 1.0f)
                        {
                            break;
                        }
                    }

                    var newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newObj.name = t.name + "Point " + t.Points.Count;
                    newObj.GetComponent<MeshRenderer>().enabled = false;
                    newObj.transform.position = new Vector3(ray.origin.x, ray.origin.y, 0);
                    t.Points.Add(newObj.transform);
                    newObj.transform.parent = t.transform;
                }
                if (Event.current.keyCode == KeyCode.O)
                {
                    if (t.Points.Count > 0)
                    {
                        DestroyImmediate(t.Points[t.Points.Count - 1].gameObject);
                        t.Points.RemoveAt(t.Points.Count - 1);
                    }
                }
                if (Event.current.keyCode == KeyCode.I)
                {
                    foreach (var p in t.Points)
                    {
                        p.GetComponent<MeshRenderer>().enabled = !p.GetComponent<MeshRenderer>().enabled;
                    }
                }
                if (Event.current.keyCode == KeyCode.U)
                {
                    if (t.Points.Contains(UnityEditor.Selection.activeTransform))
                    {
                        t.Points.Remove(UnityEditor.Selection.activeTransform);
                        DestroyImmediate(UnityEditor.Selection.activeTransform.gameObject);
                    }
                }
                break;
        }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Takes the points in the argument passed in and creates scene objects at that position for the user to 
    /// visualize the track in the scene
    /// </summary>
    /// <param name="points">The positions of the points in the track</param>
    void AssignPoints(Vector3[] points)
    {
        TrackBehaviour t = target as TrackBehaviour;
        foreach (var point in points)
        {
            var newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObj.name = t.name + "Point " + t.Points.Count;
            newObj.GetComponent<MeshRenderer>().enabled = false;
            newObj.transform.position = point;
            t.Points.Add(newObj.transform);
            t.TracksPoints.Add(new TrackPoint() { PositionOnTrack = t.Points.Count - 1, SpeedModifier = 1 });
            newObj.transform.parent = t.transform;
        }
    }

    /// <summary>
    /// Generates a circle of points around an origin point and returns them
    /// </summary>
    /// <param name="target">Center of the circle</param>
    /// <param name="maxAngle">Angle in degrees of the circle we want to generate</param>
    /// <returns></returns>
    Vector3[] GenerateRotationPoints(GameObject target, float maxAngle = 360)
    {
        List<Vector3> oribtPoints = new List<Vector3>();                        
        for (int i = 0; i < maxAngle; i += 10)
        {
            Quaternion spread = Quaternion.AngleAxis(i, target.transform.forward);
            Vector3 direction = spread * target.transform.right;
            Vector3 p = target.transform.position + (direction * target.GetComponent<TrackBehaviour>().Radius);
            oribtPoints.Add(new Vector3(p.x, p.y, 0));
        }        
        return oribtPoints.ToArray();
    }

    void MergeTracks(TrackBehaviour[] SelectedTracks)
    {
        TrackBehaviour t = target as TrackBehaviour;
        if (SelectedTracks.Length <= 1)
            return;
        else
        {
            foreach (var track in SelectedTracks)
            {
                if (track != t)
                {
                    t.IsLoop = false;
                    track.IsLoop = false;
                    t.Points.AddRange(track.Points);
                    t.TracksPoints.AddRange(track.TracksPoints);
                    track.transform.parent = t.transform;
                    DestroyImmediate(track);
                }
            }            
        }        
    }

    Transform[] ClosestPoint(Transform[] points1, Transform[] points2)
    {
        Transform[] closestset = new Transform[] { points1[0], points2[0]};
        float distance = Vector3.Distance(closestset[0].position, closestset[1].position);

        foreach(var p1 in points1)
        {
            foreach(var p2 in points2)
            {
                if(Vector3.Distance(p1.position, p2.position) < distance)
                {
                    closestset[0] = p1;
                    closestset[1] = p2;
                    distance = distance = Vector3.Distance(closestset[0].position, closestset[1].position);
                }
            }
        }
        return closestset;        
    }
    #endregion
}
#endif