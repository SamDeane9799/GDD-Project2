using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTile : LevelObject
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        //Adding the Wintile to Wintile Position
        GameManager.winTilePostion = this;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
