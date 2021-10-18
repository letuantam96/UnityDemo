using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class CustomButton : Button
{
    public Sprite imgNormal;
    public Sprite imgDisable;

    public Sprite imgPressed;
    public GameObject fetchIcon;
    public float scaleValue = 1.1f;
    public int customOnClickSound = -1;
    private Vector3 oriScale;
    private bool _isInteractable;

    protected override void Start()
    {
        base.Start();
        //oriScale = transform.localScale;
        oriScale = Vector3.one;
        try
        {
            if (imgNormal == null)
                imgNormal = GetComponent<Image>().sprite;
        }
        catch
        {
            Debug.LogError("Error while GetComponent<Image>().sprite of: " + gameObject.name);
        }
    }

    void OnStartScale()
    {
        if (!this.interactable)
            return;

        if (imgPressed)
            image.sprite = imgPressed;
    }

    void OnScaleEnd()
    {
        if (!this.interactable)
            return;

        if (imgNormal)
            image.sprite = imgNormal;
    }

    public void SetEnable(bool isEnable)
    {
        if (isEnable)
        {
            interactable = true;
            if (imgNormal)
                image.sprite = imgNormal;
        }
        else
        {
            interactable = false;
            if (imgDisable)
                image.sprite = imgDisable;
        }
    }

    public void UpdateContent(bool isEnable, bool isVideoBtn = false)
    {
        if (isEnable)
        {
            SetEnable(true);
        }
        else
        {
            SetEnable(false);
        }
        if (isVideoBtn)
        {
            if (fetchIcon)
                fetchIcon.SetActive(!isEnable);
        }
    }
    public override void OnPointerDown(PointerEventData pointerEventData)
    {
        base.OnPointerDown(pointerEventData);
        //Output the name of the GameObject that is being clicked
        //Debug.Log(name + "Game Object Click in Progress");
        transform.localScale *= scaleValue;
        if (customOnClickSound != -1)
        {
            //SoundManager.Instance.PlayFxSound((SoundType)customOnClickSound);
            // mute
        }
        else
        {
            //SoundManager.Instance.PlayFxSound(SoundType.ButtonClick);
        }
        OnStartScale();
    }

    //Detect if clicks are no longer registering
    public override void OnPointerUp(PointerEventData pointerEventData)
    {
        base.OnPointerDown(pointerEventData);
        //Debug.Log(name + "No longer being clicked");
        transform.localScale = oriScale;
        OnScaleEnd();
    }

    [ContextMenu("TEST - Disable this button")]
    public void TestSetDisableButton()
    {
        SetEnable(false);
    }
}
