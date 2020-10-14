using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : LevelObject
{

    // Start is called before the first frame update
    public float dist;
    public bool destination;
    public bool walkable;

    protected override void Start()
    {
        base.Start();
        //Adding the tiles to the tile board
        GameManager.tileBoard[(int)((float)(GameManager.GRID_WIDTH/2) + transform.position.x),(int)((float)(GameManager.GRID_HEIGHT/2) + transform.position.y)] = this;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
