using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public float moveDist;
    public override void Start()
    {
        base.Start();
        DontDestroyOnLoad(this);
        moveDist = 3;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

}
