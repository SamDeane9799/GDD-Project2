using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : LevelObject
{
    // Fields
    #region Fields
    public Grid gridRef; //All Obstacles should have a reference to the grid they are on; can just click and drag the grid to this
    public float speed = 1f;
    #endregion

    //These fields are just for Obstacles to update their own movement, we wouldn't need them if we were calling MoveToCell() from another class's Update()
    #region Other Fields
    private Vector3 destination;
    private Vector3 direction;
    private bool moving = false;
    #endregion

    // Properties
    #region Properties
    public Vector3Int CellPosistion { get { return gridRef.LocalToCell(transform.position); } }
    public Vector3Int LeftCell { get { return CellPosistion + Vector3Int.left; } }
    public Vector3Int RightCell { get { return CellPosistion + Vector3Int.right; } }
    public Vector3Int UpCell { get { return CellPosistion + Vector3Int.up; } }
    public Vector3Int DownCell { get { return CellPosistion + Vector3Int.down; } }
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //Adding the obstacle to the obstacle position
        GameManager.obstaclePositions[(int)(9.5f + transform.position.x), (int)(5.5f + transform.position.y)] = this;
        //SnapToCell();
        //MoveToCell(new Vector3Int(1, 3, 0));
        //MoveToCell(LeftCell);
        //MoveToCell(DownCell);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        AnimateMovement();
    }

    // Methods

    //This method exist because I'm too lazy to make sure that the obstacle is exactly on the grid when I click and drag it somewhere
    private void SnapToCell()
    {
        Vector3 cellCenter = gridRef.GetCellCenterLocal(CellPosistion);
        Vector3 posDifference = cellCenter - transform.position;
        transform.Translate(posDifference);
    }

    //I don't know from where exactly we will be calling these methods (probably player abilities?). They can also be used with the properties that signify the nearby cells
    public void MoveToCell(/*Vector3Int cellPosDestination*/ Vector3 cellPosDestination, float movSpeed)
    {
        // From back when the grid squares were centered at whole numbers
        //Vector3 cellPosDesCenter = gridRef.GetCellCenterLocal(cellPosDestination);
        //Vector3 posDifference = cellPosDesCenter - transform.position;

        Vector3 posDifference = cellPosDestination - transform.position;
        Vector3 direction = posDifference.normalized;
        Debug.Log(cellPosDestination);

        ////Movement would be performed here if we were calling MoveToCell from another class's update
        //Vector3 movement = direction * speed * Time.deltaTime;
        //if (Vector3.Distance(destination, transform.position) >= movement.magnitude)
        //    transform.Translate(direction * speed * Time.deltaTime);

        //Otherwise we just update variables for use by AnimateMovement()
        //destination = cellPosDesCenter;
        destination = cellPosDestination;
        this.direction = direction;
        speed = movSpeed;
        moving = true;
    }

    //You can also call it using a speed value attached to the object idk which we prefer
    public void MoveToCell(/*Vector3Int cellPosDestination*/ Vector3 cellPosDestination)
    {
        MoveToCell(cellPosDestination, speed);
    }

    //This helper method is just for Obstacles to update their own movement, we wouldn't need it if we were calling MoveToCell() from another class's Update()
    private void AnimateMovement()
    {
        if (moving)
        {
            Vector3 movement = direction * speed * Time.deltaTime;
            if (Vector3.Distance(destination, transform.position) >= movement.magnitude)
                transform.Translate(direction * speed * Time.deltaTime);
            else
            {
                moving = false;
                SnapToCell();
            }
        }
    }
}
