using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    public bool highlight;
    public float colorValue;
    public float targetValue;
    public Color originalColor;

    public virtual int X
    {
        get { return (int)(9.5f + transform.position.x); }
    }
    public virtual int Y
    {
        get { return (int)(5.5f + transform.position.y); }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
        colorValue = 0;
        targetValue = .2f;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (highlight)
        {
            Highlight();
        }
    }

    private void Highlight()
    {
        if (Mathf.Abs(targetValue - colorValue) <= .01f)
        {
            if (targetValue == .2f)
                targetValue = 0;
            else
                targetValue = .2f;
        }
        colorValue = Mathf.Lerp(colorValue, targetValue, .05f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<SpriteRenderer>().color = new Color(originalColor.r - colorValue, originalColor.g - colorValue, originalColor.b, 1);
        }
    }

    public void ResetColorValues()
    {
        colorValue = 0f;
        targetValue = .2f;
        GetComponent<SpriteRenderer>().color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);
    }
}
