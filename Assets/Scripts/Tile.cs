using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : LevelObject
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        GameManager.tileBoard[(int)(10.5f + transform.position.x),(int)(6.5f + transform.position.y)] = this;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
