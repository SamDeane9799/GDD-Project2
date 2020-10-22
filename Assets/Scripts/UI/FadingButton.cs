using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FadingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Start is called before the first frame update
    private float targetAlpha;
    public float maxAlpha;
    public float minAlpha;
    private Color currentColor;
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetAlpha = maxAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetAlpha = minAlpha;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GetComponent<Image>().color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.0f);
        targetAlpha = minAlpha;
    }

    void Start()
    {
        currentColor = GetComponent<Image>().color;
        targetAlpha = minAlpha;
    }

    void Update()
    {
        currentColor = GetComponent<Image>().color;
        if (Mathf.Abs(currentColor.a - targetAlpha) >= .01f)
            GetComponent<Image>().color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(currentColor.a, targetAlpha, .1f));
    }

}
