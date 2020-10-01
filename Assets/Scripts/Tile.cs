using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : LevelObject
{

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //Adding the tiles to the tile board
        //GameManager.tileBoard[(int)(9.5f + transform.position.x),(int)(5.5f + transform.position.y)] = this;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
