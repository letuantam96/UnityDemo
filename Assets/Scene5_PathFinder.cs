using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scene5;
using System.Linq;
using System;

public class Scene5_PathFinder : MonoBehaviour
{
    public static Scene5_PathFinder Instance;

    List<Path> paths = new List<Path>();
    //Dictionary<Scene5_Vertex, List<Scene5_Line>> connectLines = new Dictionary<Scene5_Vertex, List<Scene5_Line>>();
    Dictionary<Scene5_Vertex, List<Scene5_Vertex>> connectVers = new Dictionary<Scene5_Vertex, List<Scene5_Vertex>>();
    Scene5_Vertex start;
    Scene5_Vertex end;

    public List<Scene5_Line> allLines = new List<Scene5_Line>();
    List<Scene5_Vertex> intersecs = new List<Scene5_Vertex>();
    [SerializeField] GameObject vertexPrefab;


    // ALL PATH
    List<Scene5_Vertex> visitedVertexs = new List<Scene5_Vertex>();
    public List<List<Scene5_Vertex>> allPaths = new List<List<Scene5_Vertex>>();
    public List<float> allLenghts = new List<float>();
    public List<float> allProbality = new List<float>();
    int count;


    // AMBUSH
    [SerializeField] float AMBUSH_DISTANCE;
    [SerializeField] float AMBUSH_DISTOROAD;
    List<Scene5_Ambush> ambushs = new List<Scene5_Ambush>();
    [SerializeField] GameObject ambushPrefab;
    [SerializeField] Transform ambushTrf;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        start = Scene5_DrawController.Instance.start;
        end = Scene5_DrawController.Instance.end;
    }

    //public void LineCreated(Scene5_Line line)
    //{
    //    //AddLineTo(line.start, line);
    //    //AddLineTo(line.end, line);
    //    AddVerTo(line.start, line.end);
    //    AddVerTo(line.start, line.end);
    //    allLines.Add(line);
    //}

    //void AddVerTo(Scene5_Vertex ver, Scene5_Vertex otherVer)
    //{
    //    if (!connectVers.ContainsKey(ver))
    //    {
    //        connectVers.Add(ver, new List<Scene5_Vertex>());
    //    }

    //    if (!connectVers[ver].Contains(otherVer))
    //    {
    //        connectVers[ver].Add(otherVer);
    //    }
    //}

    //void AddLineTo(Scene5_Vertex ver, Scene5_Line line)
    //{
    //    if (!connectLines.ContainsKey(ver))
    //    {
    //        connectLines.Add(ver, new List<Scene5_Line>());
    //    }

    //    if (!connectLines[ver].Contains(line))
    //    {
    //        connectLines[ver].Add(line);
    //    }
    //}

    public void AddIntersect(Scene5_Line line)
    {
        //Debug.Log("AddIntersect");
        foreach (Scene5_Line otherLine in allLines)
        {
            Vector2 interPos = new Vector2();
            bool isIntersect = Intersection.LineIntersection(line.start.transform.position, line.end.transform.position,
                otherLine.start.transform.position, otherLine.end.transform.position,
                ref interPos);
            if (isIntersect && IsNewVertex(interPos))
            {
                //Debug.Log($"new point: {interPos}");
                //Debug.DrawLine(interPos + new Vector2(0f, -0.5f), interPos + new Vector2(0f, 0.5f), Color.black, Mathf.Infinity);
                //Debug.DrawLine(interPos + new Vector2(0.5f, 0f), interPos + new Vector2(-0.5f, 0f), Color.black, Mathf.Infinity);

                GameObject interObj = Instantiate(vertexPrefab, Scene5_DrawController.Instance.vertexsTrf);
                Scene5_Vertex inter = interObj.GetComponent<Scene5_Vertex>();
                
                inter.transform.position = interPos;
                
                inter.type = VertexType.Intersec;

                line.intersecs.Add(inter);
                otherLine.intersecs.Add(inter);
                intersecs.Add(inter);

                interObj.name = $"inter {intersecs.Count - 1}";
            }

            UpdateConnectVertex(line);
            UpdateConnectVertex(otherLine);
        }
    }

    bool IsNewVertex(Vector2 newPos)
    {
        foreach (Scene5_Vertex ver in Scene5_DrawController.Instance.allOriginVers)
        {
            if (Vector3.Distance(ver.transform.position, (Vector3)newPos) < 0.1f)
                return false;
        }
        return true;
    }




    void UpdateConnectVertex(Scene5_Line line)
    {
        //Debug.Log("UpdateConnectVertex");
        List<Scene5_Vertex> allvers = new List<Scene5_Vertex>();
        allvers.Add(line.start);
        if (line.intersecs.Count > 0)
        {
            allvers.AddRange(line.intersecs);
        }
        allvers.Add(line.end);

        //Debug.Log("Before: " + PrinDebug(allvers));
        allvers = allvers.OrderBy(x => Vector3.Distance(x.transform.position, line.start.transform.position)).ToList();
        //Debug.Log("After: " + PrinDebug(allvers));

        foreach (Scene5_Vertex ver in allvers)
        {
            //Debug.Log("adding ..." + ver);
            if (!connectVers.ContainsKey(ver))
            {
                connectVers.Add(ver, new List<Scene5_Vertex>());
            }
        }

        //clear
        for (int i = 0; i < allvers.Count; i++)
        {
            for (int j = 0; j < allvers.Count; j++)
            {
                if (connectVers[allvers[i]].Contains(allvers[j]))
                {
                    connectVers[allvers[i]].Remove(allvers[j]);
                }
            }
        }

        // re-add
        for (int i = 0; i < allvers.Count; i++)
        {
            if (i < allvers.Count - 1)
            {
                connectVers[allvers[i]].Add(allvers[i + 1]);
            }

            if (i > 0)
            {
                connectVers[allvers[i]].Add(allvers[i - 1]);
            }
        }
    }


    string PrinDebug(List<Scene5_Vertex> allvers)
    {
        string s = "";
        foreach (var ver in allvers)
        {
            s = s + ver.name + " > ";
        }
        return s;
    }






    public void FindShortestPath()
    {
        Dictionary<Scene5_Vertex, float> distance = new Dictionary<Scene5_Vertex, float>();
        List<Scene5_Vertex> greenNodes = new List<Scene5_Vertex>();
        Dictionary<Scene5_Vertex, Scene5_Vertex> parents = new Dictionary<Scene5_Vertex, Scene5_Vertex>();

        List<Scene5_Vertex> allVers = new List<Scene5_Vertex>(Scene5_DrawController.Instance.allOriginVers);
        allVers.AddRange(intersecs);

        foreach (var ver in allVers)
        {
            distance.Add(ver, Mathf.Infinity);
        }
        distance[start] = 0f;

        Scene5_Vertex tempVer;

        while (tempVer = GetMinVertex(distance, greenNodes))
        {
            //Debug.Log($"tempVer: {tempVer}");
            greenNodes.Add(tempVer);
            foreach (Scene5_Vertex otherVer in connectVers[tempVer])
            {
                //Scene5_Vertex otherVer = GetOtherVer(tempVer, line);
                if (!greenNodes.Contains(otherVer))
                {
                    float lenght = Vector3.Distance(tempVer.transform.position, otherVer.transform.position);

                    //Debug.Log($"otherVer: {otherVer}");
                    if (distance[tempVer] + lenght < distance[otherVer])
                    {
                        parents[otherVer] = tempVer;
                        distance[otherVer] = distance[tempVer] + lenght;
                    }
                    //Debug.Log($"distance[otherVer]: {distance[otherVer]}");
                }
            }
        }


        Debug.Log($"FindShortestPath: {distance[end]}");
        // draw
        Scene5_Controller.Instance.ClearDebugPaths();
        tempVer = end;
        while (parents.ContainsKey(tempVer))
        {
            //Debug.DrawLine(tempVer.transform.position, parents[tempVer].transform.position, Color.blue, Mathf.Infinity);

            Scene5_Controller.Instance.DrawOneDebugPath(tempVer.transform.position, parents[tempVer].transform.position);
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
        allPaths.Clear();

        FindPath(start);
        Debug.Log($"FindAllPaths: {count}");

        FindProbalityAllPath();
    }

    void FindPath(Scene5_Vertex ver)
    {
        visitedVertexs.Add(ver);
        //Debug.Log("visitedVertexs: " + visitedVertexs.Count);
        foreach (Scene5_Vertex otherVer in connectVers[ver])
        {
            //Scene5_Vertex otherVer = GetOtherVer(ver, line);
            if (!visitedVertexs.Contains(otherVer))
            {
                if (otherVer == end)
                {
                    count++;
                    allPaths.Add(new List<Scene5_Vertex>(visitedVertexs));
                    allPaths[allPaths.Count - 1].Add(otherVer);
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



    void FindProbalityAllPath()
    {
        allLenghts.Clear();
        allProbality.Clear();

        // a(i)
        for (int i = 0; i < allPaths.Count; i++)
        {
            float len = 0f;
            var path = allPaths[i];
            for (int j = 0; j < path.Count - 1; j++)
            {
                len += Vector3.Distance(path[j].transform.position, path[j + 1].transform.position);
            }
            allLenghts.Add(len);
        }

        float sumLen = allLenghts.Sum(x => x);

        // bi
        List<float> bHarmean = new List<float>();
        foreach (float len in allLenghts)
        {
            bHarmean.Add((sumLen + len) / (2f * sumLen * len));
        }

        float sumHarmean = bHarmean.Sum(x => x);

        // probality
        foreach (float b in bHarmean)
        {
            allProbality.Add(b / sumHarmean);
        }
    }



    public void SearchAllAmbush()
    {
        //float DISTANCE = 1f;

        ambushs.Clear();

        List<Scene5_Line> sortedList = allLines.OrderBy(x => x.start.transform.position.x).ThenBy(x => x.end.transform.position.x).ToList();

        Vector2 considerPos;

        // follow line
        foreach (Scene5_Line line in sortedList)
        {
            Vector3 start, end;
            start = line.start.transform.position.x < line.end.transform.position.x ? line.start.transform.position : line.end.transform.position;
            end = line.start.transform.position.x < line.end.transform.position.x ? line.end.transform.position : line.start.transform.position;
            float lineLenght = Vector2.Distance(start, end);
            for (float dis = -0.5f; dis < lineLenght + 0.5f; dis += 0.1f)
            {
                Vector2 pos = start + (end - start).normalized * dis;

                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 normalVector = (end - start).normalized;
                    normalVector = new Vector2(-normalVector.y, normalVector.x);

                    considerPos = pos + normalVector * i * AMBUSH_DISTOROAD;
                    if (IsThisPosAvaiableForAmbush(line, considerPos))
                    {
                        ambushs.Add(new Scene5_Ambush(considerPos));

                        // create 
                        var ambushOb = Instantiate(ambushPrefab, ambushTrf);
                        ambushOb.transform.position = considerPos;

                        //Debug.DrawLine(considerPos + new Vector2(0.4f, 0.4f), considerPos + new Vector2(-0.4f, -0.4f), Color.yellow, Mathf.Infinity);
                        //Debug.DrawLine(considerPos + new Vector2(0.4f, -0.4f), considerPos + new Vector2(-0.4f, 0.4f), Color.yellow, Mathf.Infinity);
                    }
                }


            }
        }


        // follow 2 point
        List<Vector3> allVertexAndIntersecs = new List<Vector3>();
        foreach (Scene5_Vertex ver in Scene5_DrawController.Instance.allOriginVers) allVertexAndIntersecs.Add(ver.transform.position);
        foreach (Scene5_Vertex inter in intersecs) allVertexAndIntersecs.Add(inter.transform.position);
        allVertexAndIntersecs = allVertexAndIntersecs.OrderBy(x => x.x).ToList();
        foreach (Vector3 ver in allVertexAndIntersecs)
        {
            for (float angle = 0f; angle < 2f * Mathf.PI; angle += 2f * Mathf.PI / 360f)
            {
                Vector2 chiPhuong = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                considerPos = (Vector2)ver + chiPhuong.normalized * AMBUSH_DISTOROAD;
                if (IsThisPosAvaiableForAmbush(null, considerPos))
                {
                    ambushs.Add(new Scene5_Ambush(considerPos));

                    // create 
                    var ambushOb = Instantiate(ambushPrefab, ambushTrf);
                    ambushOb.transform.position = considerPos;

                    //Debug.DrawLine(considerPos + new Vector2(0.4f, 0.4f), considerPos + new Vector2(-0.4f, -0.4f), Color.yellow, Mathf.Infinity);
                    //Debug.DrawLine(considerPos + new Vector2(0.4f, -0.4f), considerPos + new Vector2(-0.4f, 0.4f), Color.yellow, Mathf.Infinity);
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
                if (Intersection.Instance.DistancePointToLine((Vector3)pos, otherLine.start.transform.position, otherLine.end.transform.position) < AMBUSH_DISTOROAD)
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