using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Scene5
{
    public class Scene5_Controller : MonoBehaviour
    {
        int iPath = 0;
        List<List<Scene5_Vertex>> allPaths;
        List<float> allLenghts;
        List<float> allProbality;
        [SerializeField] GameObject debugLinePrefab;
        [SerializeField] Transform debugPathTrf;
        [SerializeField] TMP_Text pathCountTxt;
        [SerializeField] TMP_Text pathInfoTxt;

        private void Awake()
        {
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
            // clear
            foreach (Transform child in debugPathTrf)
            {
                Destroy(child.gameObject);
            }

            string sPath = "";

            List<Scene5_Vertex> path = allPaths[iPath];

            for (int i = 0; i < path.Count - 1; i++)
            {
                GameObject debugInstance = Instantiate(debugLinePrefab, debugPathTrf);
                LineRenderer lineRen = debugInstance.GetComponent<LineRenderer>();

                lineRen.SetPosition(0, path[i].transform.position);
                lineRen.SetPosition(1, path[i + 1].transform.position);

                TMP_Text txt = debugInstance.transform.GetChild(0).GetComponent<TMP_Text>();
                float lenght = Vector3.Distance(lineRen.GetPosition(0), lineRen.GetPosition(1));
                txt.text = $"{lenght.ToString("F1")}";
                Vector3 pos = (lineRen.GetPosition(0) + lineRen.GetPosition(1)) * 0.5f;
                txt.transform.position = pos;

                sPath = sPath + path[i].name.ToString() + " -> ";
            }

            pathCountTxt.text = $"{iPath + 1}/{allPaths.Count}";
            //Debug.Log("Path: " + sPath);
            pathInfoTxt.text = $"Len: {allLenghts[iPath].ToString("F1")}\nPro: {(100f * allProbality[iPath]).ToString("F1")}%";
        }
    }
}
