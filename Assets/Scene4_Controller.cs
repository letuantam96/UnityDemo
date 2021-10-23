using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene4_Controller : MonoBehaviour
{
    public void BackBtnPressed()
    {
        SceneManager.LoadScene(0);
    }
}
