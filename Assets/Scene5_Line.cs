using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Scene5_Line : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text lenghTxt;

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
    }
}
