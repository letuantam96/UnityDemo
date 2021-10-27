using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scene5;
using UnityEditor;

public class Scene5_PathFinder : MonoBehaviour
{
    public static Scene5_PathFinder Instance;

    List<Path> paths = new List<Path>();
    Dictionary<Scene5_Vertex, List<Scene5_Line>> connectLines = new Dictionary<Scene5_Vertex, List<Scene5_Line>>();
    Scene5_Vertex start;
    Scene5_Vertex end;

    List<Scene5_Line> allLines = new List<Scene5_Line>();


    // ALL PATH
    List<Scene5_Vertex> visitedVertexs = new List<Scene5_Vertex>();
    int count;


    // AMBUSH
    [SerializeField] float AMBUSH_DISTANCE;
    [SerializeField] float AMBUSH_DISTOROAD;
    List<Scene5_Ambush> ambushs = new List<Scene5_Ambush>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        start = Scene5_DrawController.Instance.start;
        end = Scene5_DrawController.Instance.end;
    }

    public void LineCreated(Scene5_Line line)
    {
        AddLineTo(line.start, line);
        AddLineTo(line.end, line);
        allLines.Add(line);
    }

    void AddLineTo(Scene5_Vertex ver, Scene5_Line line)
    {
        if (!connectLines.ContainsKey(ver))
        {
            connectLines.Add(ver, new List<Scene5_Line>());
        }

        if (!connectLines[ver].Contains(line))
        {
            connectLines[ver].Add(line);
        }
    }

    public void AddIntersect(Scene5_Line line)
    {
        foreach (Scene5_Line otherLine in allLines)
        {
            Vector2 interPos = new Vector2();
            bool isIntersect = Intersection.LineIntersection(line.start.transform.position, line.end.transform.position,
                otherLine.start.transform.position, otherLine.end.transform.position,
                ref interPos);
            if (isIntersect && IsNewVertex(interPos))
            {
                Debug.Log($"new point: {interPos}");
                Debug.DrawLine(interPos + new Vector2(0f, -0.5f), interPos + new Vector2(0f, 0.5f), Color.black, Mathf.Infinity);
                Debug.DrawLine(interPos + new Vector2(0.5f, 0f), interPos + new Vector2(-0.5f, 0f), Color.black, Mathf.Infinity);
            }
        }
    }

    bool IsNewVertex(Vector2 newPos)
    {
        foreach (Scene5_Vertex ver in Scene5_DrawController.Instance.allVers)
        {
            if (Vector3.Distance(ver.transform.position, (Vector3)newPos) < 0.1f)
                return false;
        }
        return true;
    }

    public void FindShortestPath()
    {
        Dictionary<Scene5_Vertex, float> distance = new Dictionary<Scene5_Vertex, float>();
        List<Scene5_Vertex> greenNodes = new List<Scene5_Vertex>();
        Dictionary<Scene5_Vertex, Scene5_Vertex> parents = new Dictionary<Scene5_Vertex, Scene5_Vertex>();
        Dictionary<Scene5_Vertex, Scene5_Line> parentLine = new Dictionary<Scene5_Vertex, Scene5_Line>();

        foreach (var ver in Scene5_DrawController.Instance.allVers)
        {
            distance.Add(ver, Mathf.Infinity);
        }
        distance[start] = 0f;

        Scene5_Vertex tempVer;

        while (tempVer = GetMinVertex(distance, greenNodes))
        {
            //Debug.Log($"tempVer: {tempVer}");
            greenNodes.Add(tempVer);
            foreach (Scene5_Line line in connectLines[tempVer])
            {
                Scene5_Vertex otherVer = GetOtherVer(tempVer, line);
                if (!greenNodes.Contains(otherVer))
                {
                    //Debug.Log($"otherVer: {otherVer}");
                    if (distance[tempVer] + line.lenght < distance[otherVer])
                    {
                        parents[otherVer] = tempVer;
                        parentLine[otherVer] = line;
                        distance[otherVer] = distance[tempVer] + line.lenght;
                    }
                    //Debug.Log($"distance[otherVer]: {distance[otherVer]}");
                }
            }
        }


        Debug.Log($"FindShortestPath: {distance[end]}");
        // draw
        tempVer = end;
        while (parents.ContainsKey(tempVer))
        {
            Debug.DrawLine(tempVer.transform.position, parents[tempVer].transform.position, Color.blue, Mathf.Infinity);
            tempVer = parents[tempVer];
        }
    }


    Scene5_Vertex GetOtherVer(Scene5_Vertex ver, Scene5_Line line)
    {
        return line.start == ver ? line.end : line.start;
    }

    Scene5_Vertex GetMinVertex(Dictionary<Scene5_Vertex, float> distances, List<Scene5_Vertex> greenNodes)
    {
        float min = Mathf.Infinity;
        Scene5_Vertex ver = null;
        foreach (KeyValuePair<Scene5_Vertex, float> pair in distances)
        {
            if (distances[pair.Key] < min && !greenNodes.Contains(pair.Key))
            {
                ver = pair.Key;
                min = distances[pair.Key];
            }
        }
        return ver;
    }


    // ALL PATH
    public void FindAllPaths()
    {
        visitedVertexs.Clear();
        count = 0;

        FindPath(start);
        Debug.Log($"FindAllPaths: {count}");
    }

    void FindPath(Scene5_Vertex ver)
    {
        visitedVertexs.Add(ver);
        //Debug.Log("visitedVertexs: " + visitedVertexs.Count);
        foreach (Scene5_Line line in connectLines[ver])
        {
            Scene5_Vertex otherVer = GetOtherVer(ver, line);
            if (!visitedVertexs.Contains(otherVer))
            {
                if (otherVer == end)
                {
                    count++;
                }
                else
                {
                    FindPath(otherVer);
                }

                if (visitedVertexs[visitedVertexs.Count - 1] == otherVer)
                {
                    visitedVertexs.RemoveAt(visitedVertexs.Count - 1);
                }
            }
        }
    }





    public void SearchAllAmbush()
    {
        //float DISTANCE = 1f;

        ambushs.Clear();

        foreach (Scene5_Line line in allLines)
        {
            float lineLenght = Vector2.Distance(line.start.transform.position, line.end.transform.position);
            for (float dis = Random.Range(0f, 0.1f) - AMBUSH_DISTANCE; dis < lineLenght + AMBUSH_DISTANCE; dis += 0.1f)
            {
                Vector2 pos = line.start.transform.position +
                    (line.end.transform.position - line.start.transform.position).normalized * dis;

                for (int i = -1; i <= 1; i+= 2)
                {
                    Vector2 normalVector = (line.end.transform.position - line.start.transform.position).normalized;
                    normalVector = new Vector2(-normalVector.y, normalVector.x);

                    Vector2 considerPos = pos + normalVector * i * AMBUSH_DISTOROAD;
                    if (IsThisPosAvaiableForAmbush(line, considerPos))
                    {
                        ambushs.Add(new Scene5_Ambush(considerPos));

                        Debug.DrawLine(considerPos + new Vector2(0.4f, 0.4f), considerPos + new Vector2(-0.4f, -0.4f), Color.yellow, Mathf.Infinity);
                        Debug.DrawLine(considerPos + new Vector2(0.4f, -0.4f), considerPos + new Vector2(-0.4f, 0.4f), Color.yellow, Mathf.Infinity);
                    }
                }


            }
        }
    }

    public bool IsThisPosAvaiableForAmbush(Scene5_Line line, Vector2 pos)
    {
        foreach (Scene5_Line otherLine in allLines)
        {
            if (otherLine != line)
            {
                if (HandleUtility.DistancePointLine((Vector3) pos, otherLine.start.transform.position, otherLine.end.transform.position) < AMBUSH_DISTOROAD)
                {
                    return false;
                }
            }
        }

        foreach (Scene5_Ambush ambush in ambushs)
        {
            if (Vector2.Distance(ambush.pos, pos) < AMBUSH_DISTANCE - 0.01f)
            {
                return false;
            }
        }

        return true;
    }
}

public class Path
{
    public List<Scene5_Line> lines = new List<Scene5_Line>();
    public float lenght = 0f;
}