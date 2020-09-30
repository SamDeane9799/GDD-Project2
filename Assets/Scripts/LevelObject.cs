using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        ChangeColor();
    }

    // Changes color of the prefab when mouse buttons are pressed
    protected void ChangeColor()
    {
        if (Input.GetMouseButton(0))
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Test/Shwing");
        }

        if (Input.GetMouseButton(1))
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Test/Shwing");
        }
    }
}
