using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour
{
    public const string DRAW_PATH_SPRITE = "2.DrawPath_Sprite";
    public const string DRAW_PATH_OUTLINE = "3.DrawPath_Outline";
    public const string TERRAIN = "4.Terrain";

    public void DrawPathSpriteBtnPressed()
    {
        SceneManager.LoadScene(DRAW_PATH_SPRITE);
    }

    public void DrawPathOutlineBtnPressed()
    {
        SceneManager.LoadScene(DRAW_PATH_OUTLINE);
    }

    public void TerrainBtnPressed()
    {
        SceneManager.LoadScene(TERRAIN);
    }
}
