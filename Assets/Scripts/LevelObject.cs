using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    private FMOD.Studio.EventInstance instance;

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
        //instance = FMODUnity.RuntimeManager.CreateInstance("event:/Test/Shwing");
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        ChangeColor();
    }

    // Changes color of the prefab when mouse buttons are pressed
    protected void ChangeColor()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //GetComponent<SpriteRenderer>().color = Color.blue;
            instance.start();
            instance.release();
        }

        if (Input.GetMouseButtonDown(1))
        {
            //GetComponent<SpriteRenderer>().color = Color.white;
            instance.start();
            instance.release();
        }
    }
}
