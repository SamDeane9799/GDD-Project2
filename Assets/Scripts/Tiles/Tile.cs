﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : LevelObject
{

    // Start is called before the first frame update
    public float dist;
    public bool destination;
    public bool walkable;

    void Awake()
    {
        //Adding the tiles to the tile board
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
