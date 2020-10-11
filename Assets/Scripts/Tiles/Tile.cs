using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : LevelObject
{

    // Start is called before the first frame update
    public float dist;
    public bool destination;
    public bool walkable;
    public bool highlight;
    private float blueValue;
    private float targetBlue;
    public Color tileColor;

    protected override void Start()
    {
        base.Start();
        //Adding the tiles to the tile board
        GameManager.tileBoard[(int)(9.5f + transform.position.x),(int)(5.5f + transform.position.y)] = this;


        tileColor = GetComponent<SpriteRenderer>().color;
        blueValue = 1f;
        targetBlue = tileColor.b;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (highlight)
            Highlight();
    }

    private void Highlight()
    {
        if (Mathf.Abs(targetBlue - blueValue) <= .01f)
        {
            if (targetBlue == tileColor.b)
                targetBlue = 1f;
            else
                targetBlue = tileColor.b;
        }
        blueValue = Mathf.Lerp(blueValue, targetBlue, .035f);
        GetComponent<SpriteRenderer>().color = new Color(tileColor.r, tileColor.g, blueValue, 1);
    }

    public void ResetColorValues()
    {
        blueValue = 1f;
        targetBlue = tileColor.b;
    }
}
