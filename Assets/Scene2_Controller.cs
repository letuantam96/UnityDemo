using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene2_Controller : MonoBehaviour
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
}
