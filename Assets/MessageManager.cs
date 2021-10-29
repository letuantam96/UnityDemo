using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text messageTxt;

    public static MessageManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        messageTxt.gameObject.SetActive(false);
    }

    public void ShowMessage(string mess)
    {
        StartCoroutine(IShowMess(mess));
    }

    IEnumerator IShowMess(string mess)
    {
        messageTxt.gameObject.SetActive(true);
        messageTxt.text = mess;
        yield return new WaitForSeconds(3f);
        messageTxt.gameObject.SetActive(false);
    }
}
