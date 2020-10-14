using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : Obstacle
{
    public bool burned = false;

    private void Start()
    {
        base.Start();
        //Adding the obstacle to the obstacle position
        GameManager.bushPositions[(int)(9.5f + transform.position.x), (int)(5.5f + transform.position.y)] = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (burned)
        {
            Destroy(this);
        }
    }
}
