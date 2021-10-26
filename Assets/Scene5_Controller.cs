using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene5
{
    public class Scene5_Controller : MonoBehaviour
    {
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
        }
    }
}
