using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Changes color of the prefab when mouse buttons are pressed
    protected void ChangeColor() 
    {
        if (Input.GetMouseButton(0)) 
        {
            GetComponent<SpriteRenderer>().color = Color.blue;
        }

        if (Input.GetMouseButton(1))
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
