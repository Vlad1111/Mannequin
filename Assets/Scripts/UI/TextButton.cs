using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextButton : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI text;
    public Image background;
    public Color selectColor;
    public Color deselectColor;
    // add callbacks in the inspector like for buttons
    public UnityEvent onClick;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        ApplySelect();
        // invoke your event
        onClick.Invoke();
    }

    internal void DeSelect()
    {
        if (background)
            background.color = deselectColor;
    }

    public void ApplySelect()
    {
        if (background)
            background.color = selectColor;
    }

    internal void SetText(string text)
    {
        if(text != null)
            this.text.text = text;
    }

    public string GetText()
    {
        return text.text;
    }

    public void SetButtonImage(Sprite image)
    {
        if (background != null)
            background.sprite = image;
    }
}
