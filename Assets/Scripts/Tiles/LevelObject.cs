using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    // instance of the test sound effect
    private FMOD.Studio.EventInstance testInstance;


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
        testInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Test/Shwing");
        originalColor = GetComponent<SpriteRenderer>().color;
        colorValue = 0;
        targetValue = .2f;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //TestFMOD();
        if (highlight)
        {
            Highlight();
        }
    }

    // Changes color of the prefab when mouse buttons are pressed and plays a test sfx
    protected void TestFMOD()
    {
        if (Input.GetMouseButtonDown(0))
        {
            testInstance.start();
            testInstance.release();
        }

        if (Input.GetMouseButtonDown(1))
        {
            testInstance.start();
            testInstance.release();
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
        colorValue = Mathf.Lerp(colorValue, targetValue, .035f);
        Debug.Log(colorValue);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<SpriteRenderer>().color = new Color(originalColor.r - colorValue, originalColor.g - colorValue, originalColor.b, 1);
        }
    }

    public void ResetColorValues()
    {
        colorValue = 0f;
        targetValue = .2f;
    }
}
