using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

namespace Scene5
{
    public class Scene5_DrawController : MonoBehaviour
    {
        public static Scene5_DrawController Instance;


        public Scene5_Vertex start;
        public Scene5_Vertex end;
        public List<Scene5_Vertex> vers = new List<Scene5_Vertex>();
        public List<Scene5_Vertex> allVers;

        [Header("Other")]
        public Camera m_camera;
        public GameObject brush;
        public Transform paths;
        LineRenderer currentLineRenderer;
        public TMP_Text inkTxt;

        [Header("Config")]
        [SerializeField] private float snapDistance = 0.1f;
        [SerializeField] private float maxInk = 100f;
        private float currentInk;

        private float CurrentLineInk => currentLineRenderer ? Vector3.Distance(currentLineRenderer.GetPosition(0), currentLineRenderer.GetPosition(1)) : 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            allVers = new List<Scene5_Vertex>(vers);
            allVers.Add(start);
            allVers.Add(end);

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
            if (!tempVer) return;


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
            Scene5_Vertex tempVer = FindNearestVertexToLine();
            if (!tempVer
                || currentInk < CurrentLineInk
                || CurrentLineInk <= 1f)
            {
                if (currentLineRenderer)
                    Destroy(currentLineRenderer.gameObject);
            }
            else
            {
                currentLineRenderer.SetPosition(1, tempVer.transform.position);
                currentInk -= CurrentLineInk;
                Scene5_Line line = currentLineRenderer.gameObject.GetComponent<Scene5_Line>();
                line.SetLength(CurrentLineInk);
                line.SetVertex(null, tempVer);

                Scene5_PathFinder.Instance.AddIntersect(line);

                Scene5_PathFinder.Instance.LineCreated(line);
                
            }

            UpdateInkTxt(currentInk);
            currentLineRenderer = null;
        }

        Scene5_Vertex SnapMousePosIntoVertex()
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            foreach (Scene5_Vertex ver in allVers)
            {
                if (Vector3.Distance(ver.gameObject.transform.position, mousePos) <= snapDistance)
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
            foreach (Scene5_Vertex ver in allVers)
            {
                if (HandleUtility.DistancePointLine(ver.transform.position,
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
    }
}
