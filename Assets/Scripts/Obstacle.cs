using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : LevelObject
{
    // Fields
    public Grid gridRef; //All Obstacles should have a reference to the grid they are on; can just click and drag the grid to this
    public float speed = 1f;

    // Properties
    public Vector3Int CellPosistion { get { return gridRef.LocalToCell(transform.position); } }
    public Vector3Int LeftCell { get { return CellPosistion + Vector3Int.left; } }
    public Vector3Int RightCell { get { return CellPosistion + Vector3Int.right; } }
    public Vector3Int UpCell { get { return CellPosistion + Vector3Int.up; } }
    public Vector3Int DownCell { get { return CellPosistion + Vector3Int.down; } }

    //Methods

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SnapToCell();

        //Testing
        MoveToCell(LeftCell);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    //This method exist because I'm too lazy to make sure that the obstacle is exactly on the grid when I click and drag it somewhere
    private void SnapToCell()
    {
        Vector3 cellCenter = gridRef.GetCellCenterLocal(CellPosistion);
        Vector3 posDifference = cellCenter - transform.position;
        transform.Translate(posDifference);
    }

    //I don't know from where exactly we will be calling these methods (probably player abilities?). They can also be used with the properties that signify the nearby cells
    public void MoveToCell(Vector3Int cellPosDestination, float movSpeed)
    {
        Vector3 cellPosDesCenter = gridRef.GetCellCenterLocal(cellPosDestination);
        Vector3 posDifference = cellPosDesCenter - transform.position;
        Vector3 direction = posDifference.normalized;

        //Loop until the distance is acheived
        while (Mathf.Pow(cellPosDestination.x - transform.position.x,2) + Mathf.Pow(cellPosDestination.y - transform.position.y, 2) > 0)
            transform.Translate(direction * movSpeed * Time.deltaTime);
    }
    //You can also call it using a speed value attached to the object idk which we prefer
    public void MoveToCell(Vector3Int cellPosDestination)
    {
        MoveToCell(cellPosDestination, speed);
    }
}
