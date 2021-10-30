using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Scene5
{
    public class Scene5_DrawController : MonoBehaviour
    {
        public static Scene5_DrawController Instance;


        public Scene5_Vertex start;
        public Scene5_Vertex end;
        public List<Scene5_Vertex> vers = new List<Scene5_Vertex>();
        public List<Scene5_Vertex> allOriginVers;

        [Header("Other")]
        public Camera m_camera;
        public GameObject brush;
        public Transform paths;
        public Transform vertexsTrf;
        LineRenderer currentLineRenderer;
        public TMP_Text inkTxt;

        [Header("Config")]
        [SerializeField] private float snapDistance = 0.1f;
        [SerializeField] private float startDrawSnapDistance = 0.1f;
        [SerializeField] private float maxInk = 100f;
        private float currentInk;

        private float CurrentLineInk => currentLineRenderer ? Vector3.Distance(currentLineRenderer.GetPosition(0), currentLineRenderer.GetPosition(1)) : 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            allOriginVers = new List<Scene5_Vertex>(vers);
            allOriginVers.Add(start);
            allOriginVers.Add(end);

            currentInk = maxInk;
        }


        private void Update()
        {
            Drawing();
        }

        void Drawing()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                CreateBrush();
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                OnDraging();
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                OnReleasing();
            }
        }

        void CreateBrush()
        {

            Scene5_Vertex tempVer = SnapMousePosIntoVertex();
            if (!tempVer)
            {
                DeleteLineBehind();
                return;
            }

            GameObject brushInstance = Instantiate(brush, paths);
            currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

            //because you gotta have 2 points to start a line renderer, 
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);

            currentLineRenderer.SetPosition(0, tempVer.transform.position);
            currentLineRenderer.SetPosition(1, mousePos);

            currentLineRenderer.gameObject.GetComponent<Scene5_Line>().SetVertex(tempVer, null);
        }

        void OnDraging()
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            if (currentLineRenderer)
            {
                currentLineRenderer.SetPosition(1, mousePos);
                UpdateInkTxt(currentInk - CurrentLineInk);
            }
        }

        void OnReleasing()
        {
            Scene5_Vertex mouseVertex = SnapMousePosIntoVertex();
            Scene5_Vertex tempVer = FindNearestVertexToLine();
            if (!tempVer
                || currentInk < CurrentLineInk
                || CurrentLineInk <= 1f)
            {
                if (currentLineRenderer)
                    Destroy(currentLineRenderer.gameObject);

                if (currentInk < CurrentLineInk)
                {
                    MessageManager.Instance.ShowMessage("Out of points, let's tap any roads to regain some then redraw");
                }
            }
            else
            {
                currentLineRenderer.SetPosition(1, tempVer.transform.position);
                currentInk -= CurrentLineInk;
                Scene5_Line line = currentLineRenderer.gameObject.GetComponent<Scene5_Line>();
                //line.SetLength(CurrentLineInk);
                line.SetVertex(null, tempVer);

                Scene5_PathFinder.Instance.allLines.Add(line);
                Scene5_PathFinder.Instance.AddIntersect(line);

                //Scene5_PathFinder.Instance.LineCreated(line);
                
            }

            UpdateInkTxt(currentInk);


            if (tempVer && mouseVertex != tempVer)
            {
                // create new
                GameObject brushInstance = Instantiate(brush, paths);
                currentLineRenderer = brushInstance.GetComponent<LineRenderer>();
                Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                currentLineRenderer.SetPosition(0, tempVer.transform.position);
                currentLineRenderer.SetPosition(1, mousePos);
                currentLineRenderer.gameObject.GetComponent<Scene5_Line>().SetVertex(tempVer, null);

                OnReleasing();
            }

            currentLineRenderer = null;
        }

        Scene5_Vertex SnapMousePosIntoVertex()
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            foreach (Scene5_Vertex ver in allOriginVers)
            {
                if (Vector3.Distance(ver.gameObject.transform.position, mousePos) <= startDrawSnapDistance)
                {
                    return ver;
                }
            }
            return null;
        }

        Scene5_Vertex FindNearestVertexToLine()
        {
            if (!currentLineRenderer) return null;

            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            Scene5_Vertex result = null;
            float minDis = Mathf.Infinity;
            foreach (Scene5_Vertex ver in allOriginVers)
            {
                if (Intersection.Instance.DistancePointToLine(ver.transform.position,
                    currentLineRenderer.GetPosition(0),
                    mousePos) < snapDistance)
                {
                    if (Vector3.Distance(currentLineRenderer.GetPosition(0), ver.transform.position) < minDis
                        && Vector3.Distance(currentLineRenderer.GetPosition(0), ver.transform.position) > 0.1f)
                    {
                        result = ver;
                        minDis = Vector3.Distance(currentLineRenderer.GetPosition(0), ver.transform.position);
                    }
                }
            }
            return result;
        }

        void UpdateInkTxt(float ink)
        {
            inkTxt.text = $"{(int)(ink)}";
        }








        void DeleteLineBehind()
        {


            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            // find line
            for (int i = 0; i < Scene5_PathFinder.Instance.allLines.Count; i++)
            {
                Scene5_Line line = Scene5_PathFinder.Instance.allLines[i];
                if (Intersection.Instance.DistancePointToLine(mousePos, 
                    line.start.transform.position, 
                    line.end.transform.position) < 0.5f)
                {
                    Vector3 hinhChieu = Intersection.Instance.NearestPointOnFiniteLine(line.start.transform.position, line.end.transform.position, mousePos);
                    if (Vector3.Distance(line.start.transform.position, hinhChieu) + Vector3.Distance(hinhChieu, line.end.transform.position) 
                        - Vector3.Distance(line.start.transform.position, line.end.transform.position) < Mathf.Epsilon)
                    {
                        // delete this line

                        Scene5_Controller.Instance.ClearDebugPaths();
                        Scene5_Controller.Instance.ClearInvalidPath();


                        currentInk += Vector3.Distance(line.start.transform.position, line.end.transform.position);
                        
                        
                        Scene5_PathFinder.Instance.allLines.Remove(line);
                        Scene5_PathFinder.Instance.RemoveIntersect(line);
                        Destroy(line.gameObject);

                        i--;
                    }
                }
            }
        }
    }
}
