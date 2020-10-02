using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public override void Start()
    {
        base.Start();
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

}
