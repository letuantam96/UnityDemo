using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scene5;

public class Scene5_Line : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text lenghTxt;
    public Scene5_Vertex start = null;
    public Scene5_Vertex end = null;
    public float lenght = 0f;

    private void Start()
    {
        lenghTxt.gameObject.SetActive(false);
    }

    public void SetLength(float lenght)
    {
        lenghTxt.gameObject.SetActive(true);
        lenghTxt.text = $"{lenght.ToString("F1")}";
        Vector3 pos = gameObject.GetComponent<LineRenderer>().GetPosition(0) + gameObject.GetComponent<LineRenderer>().GetPosition(1);
        pos *= 0.5f;
        lenghTxt.gameObject.transform.position = pos;

        this.lenght = lenght;
    }

    public void SetVertex(Scene5_Vertex start, Scene5_Vertex end)
    {
        if (start)
        {
            this.start = start;
        }
        
        if (end)
        {
            this.end = end;
        }
    }
}
