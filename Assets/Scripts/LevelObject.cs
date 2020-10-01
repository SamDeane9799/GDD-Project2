using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    // instance of the test sound effect
    private FMOD.Studio.EventInstance testInstance;

    public int X
    {
        get { return (int)(9.5f + transform.position.x); }
    }
    public int Y
    {
        get { return (int)(5.5f + transform.position.y); }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        testInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Test/Shwing");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //ChangeColor();
    }

    // Changes color of the prefab when mouse buttons are pressed and plays a test sfx
    protected void ChangeColor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
            testInstance.start();
            testInstance.release();
        }

        if (Input.GetMouseButtonDown(1))
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            testInstance.start();
            testInstance.release();
        }
    }
}
