using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour
{
    public const string DRAW_PATH_OUTLINE = "2.DrawPath_Outline";

    public void DrawPathOutlineBtnPressed()
    {
        SceneManager.LoadScene(DRAW_PATH_OUTLINE);
    }
}
