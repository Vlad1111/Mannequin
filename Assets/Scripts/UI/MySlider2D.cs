using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Slider;

public class MySlider2D : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public RectTransform Slider;
    public RectTransform Handle;

    private Vector2 _value;
    public Vector2 Value
    {
        get => _value;
        set
        {
            _value = value;
            SetHandlePosition();
        }
    }

    public Vector2 MinValues;
    public Vector2 MaxValues;
    public bool isCircular;
    public UnityEvent onValueChanged;

    public void OnDrag(PointerEventData eventData)
    {
        Slide(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Slide(eventData);
    }

    private void SetHandlePosition()
    {
        var poz = _value;
        poz.x = (poz.x - MinValues.x) / (MaxValues.x - MinValues.x);
        poz.y = (poz.y - MinValues.y) / (MaxValues.y - MinValues.y);

        Vector3[] v = new Vector3[4];
        Slider.GetWorldCorners(v);
        var downCorner = v[0];
        var upCorner = v[2];

        poz.x = (upCorner.x - downCorner.x) * poz.x + downCorner.x;
        poz.y = (upCorner.y - downCorner.y) * poz.y + downCorner.y;

        Handle.position = poz;
    }

    public void Slide(PointerEventData eventData)
    {
        var poz = eventData.position;

        Vector3[] v = new Vector3[4];
        Slider.GetWorldCorners(v);
        var downCorner = v[0];
        var upCorner = v[2];

        if(poz.x < downCorner.x)
            poz.x = downCorner.x;
        else if (poz.x > upCorner.x)
            poz.x = upCorner.x;

        if (poz.y < downCorner.y)
            poz.y = downCorner.y;
        else if (poz.y > upCorner.y)
            poz.y = upCorner.y;

        _value.x = (poz.x - downCorner.x) / (upCorner.x - downCorner.x);
        _value.y = (poz.y - downCorner.y) / (upCorner.y - downCorner.y);
        if(isCircular && (_value - new Vector2(0.5f, 0.5f)).magnitude > 0.5)
            _value = (_value - new Vector2(0.5f, 0.5f)).normalized / 2 + new Vector2(0.5f, 0.5f);

        _value.x = (MaxValues.x - MinValues.x) * _value.x + MinValues.x;
        _value.y = (MaxValues.y - MinValues.y) * _value.y + MinValues.y;

        Debug.Log(_value);


        Handle.position = poz;
        if (isCircular)
            SetHandlePosition();

        onValueChanged.Invoke();
    }
}
