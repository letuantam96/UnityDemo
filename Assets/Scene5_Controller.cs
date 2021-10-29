using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Scene5
{
    public class Scene5_Controller : MonoBehaviour
    {
        public static Scene5_Controller Instance;
        int iPath = 0;
        List<List<Scene5_Vertex>> allPaths;
        List<float> allLenghts;
        List<float> allProbality;
        [SerializeField] GameObject debugLinePrefab;
        [SerializeField] Transform debugPathTrf;
        [SerializeField] TMP_Text pathCountTxt;
        [SerializeField] TMP_Text pathInfoTxt;
        [SerializeField] GameObject invalidLinePrefab;
        [SerializeField] Transform invalidPathTrf;

        private void Awake()
        {
            Instance = this;

            allPaths = Scene5_PathFinder.Instance.allPaths;
            allLenghts = Scene5_PathFinder.Instance.allLenghts;
            allProbality = Scene5_PathFinder.Instance.allProbality;
        }

        public void ClearBtnPressed()
        {
            foreach (LineRenderer lr in FindObjectsOfType<LineRenderer>())
            {
                Destroy(lr.gameObject, UnityEngine.Random.Range(0f, 0.2f));
            }
        }

        public void BackBtnPressed()
        {
            SceneManager.LoadScene(0);
        }

        public void ShortestBtnPressed()
        {
            Scene5_PathFinder.Instance.FindShortestPath();
        }

        public void AllPathBtnPressed()
        {
            Scene5_PathFinder.Instance.FindAllPaths();
            iPath = 0;
            DrawDebugPath();
            Scene5_PathFinder.Instance.CheckInvalidPath();
        }

        public void AmbushBtnPressed()
        {
            Scene5_PathFinder.Instance.SearchAllAmbush();
        }


        public void NextPathBtnPressed()
        {
            if (allPaths.Count == 0) return;
            iPath = (iPath + 1) % allPaths.Count;
            DrawDebugPath();
        }

        public void PreviousPathBtnPressed()
        {
            if (allPaths.Count == 0) return;
            iPath = (iPath - 1 + allPaths.Count) % allPaths.Count;
            DrawDebugPath();
        }

        void DrawDebugPath()
        {

            ClearDebugPaths();

            string sPath = "";

            if (allPaths.Count <= 0)
            {

                return;
            }

            List<Scene5_Vertex> path = allPaths[iPath];

            for (int i = 0; i < path.Count - 1; i++)
            {
                DrawOneDebugPath(path[i].transform.position, path[i + 1].transform.position);

                sPath = sPath + path[i].name.ToString() + " -> ";
            }

            pathCountTxt.text = $"{iPath + 1}/{allPaths.Count}";
            //Debug.Log("Path: " + sPath);
            pathInfoTxt.text = $"Len: {allLenghts[iPath].ToString("F1")}\nPro: {(100f * allProbality[iPath]).ToString("F1")}%";
        }


        public void DrawOneDebugPath(Vector3 start, Vector3 end)
        {
            GameObject debugInstance = Instantiate(debugLinePrefab, debugPathTrf);
            LineRenderer lineRen = debugInstance.GetComponent<LineRenderer>();

            lineRen.SetPosition(0, start);
            lineRen.SetPosition(1, end);

            TMP_Text txt = debugInstance.transform.GetChild(0).GetComponent<TMP_Text>();
            float lenght = Vector3.Distance(start, end);
            txt.text = $"{lenght.ToString("F1")}";
            Vector3 pos = (start + end) * 0.5f;
            txt.transform.position = pos;
        }


        public void ClearDebugPaths()
        {
            // clear
            foreach (Transform child in debugPathTrf)
            {
                Destroy(child.gameObject);
            }
        }


        public void ClearInvalidPath()
        {
            // clear
            foreach (Transform child in invalidPathTrf)
            {
                Destroy(child.gameObject);
            }
        }

        public void DrawInvalidPath(Vector3 start, Vector3 end)
        {
            GameObject invalidInstance = Instantiate(invalidLinePrefab, invalidPathTrf);
            LineRenderer lineRen = invalidInstance.GetComponent<LineRenderer>();

            lineRen.SetPosition(0, start);
            lineRen.SetPosition(1, end);
        }
    }
}
