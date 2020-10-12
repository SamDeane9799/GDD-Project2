using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    // instance of the test sound effect
    private FMOD.Studio.EventInstance testInstance;


    public bool highlight;
    public float blueValue;
    public float targetBlue;
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
        blueValue = originalColor.b - .2f;
        targetBlue = originalColor.b;
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
        if (Mathf.Abs(targetBlue - blueValue) <= .01f)
        {
            if (targetBlue == originalColor.b)
                targetBlue = originalColor.b - .2f;
            else
                targetBlue = originalColor.b;
        }
        blueValue = Mathf.Lerp(blueValue, targetBlue, .035f);
        GetComponent<SpriteRenderer>().color = new Color(blueValue, blueValue, originalColor.b, 1);
    }

    public void ResetColorValues()
    {
        blueValue = 1f;
        targetBlue = originalColor.b;
    }
}
