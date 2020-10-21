using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    private const float visionLength = 1.0f;
    private const int visionConeDegrees = 70;
    private bool foundEnemy;
    public Vector3 targetRotation;
    private Vector3 currentRotation;

    private Vector3 rayDir;
    void Start()
    {
        foundEnemy = false;
        currentRotation = transform.rotation.eulerAngles;
        targetRotation = currentRotation;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (Vector3.Distance(transform.rotation.eulerAngles, targetRotation) >= .1f)
        {
            currentRotation = Vector3.Slerp(transform.rotation.eulerAngles, targetRotation, .05f);
            transform.rotation = Quaternion.Euler(currentRotation);
        }
        else if(targetRotation != transform.rotation.eulerAngles)
        {
            transform.rotation = Quaternion.Euler(targetRotation);
        }
    }

    //Here we move our enemy forward depending on which way it's rotated and we rotate it if it's in front of an obstacle
    //We should also be checking for the player being in front of the enemy here
    public void EnemyTurn()
    {
        //Checking if we can continue to move in our direction that we are already in
        Vector2 nextPosition = new Vector2(currentTile.X + Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), currentTile.Y + Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        Debug.Log(GameManager.tileBoard[(int)Mathf.Round(nextPosition.x), (int)Mathf.Round(nextPosition.y)].transform.position);
        if((nextPosition.x != -1 && nextPosition.x != GameManager.GRID_WIDTH) && (nextPosition.y != -1 && nextPosition.y != GameManager.GRID_HEIGHT) && 
            GameManager.obstaclePositions[(int)nextPosition.x, (int)nextPosition.y] == null && GameManager.tileBoard[(int)Mathf.Round(nextPosition.x), (int)Mathf.Round(nextPosition.y)].walkable)
        {
            //If we can then we move the enemy
            currentTile = GameManager.tileBoard[(int)Mathf.Round(nextPosition.x), (int)Mathf.Round(nextPosition.y)];
            actionPoints -= 1;
        }
        else
        {
            actionPoints -= 1;
        }
    }

    //This is our method that is used to detect players
    public void DetectPlayer(int numOfRays)
    {
        //Setting up the layer masks needed to check for in our rays
        int characterMask = 1 << 8;
        int terrainMask = 1 << 9;
        int layerMask = terrainMask | characterMask;

        Player potentialPlayer = null;
        rayDir = new Vector3(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), 0);
        //Casting the ray
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, visionLength, layerMask);
        //Checking if we hit a player
        if (hit)
        {
            if (hit.collider.TryGetComponent<Player>(out potentialPlayer))
            {
                //Declaring that the player has lost and changing the gamestate
                GameManager.currentGameState = GameState.LOSE;
                FoundPlayer(hit.transform.position);
            }
        }
    }

    public void CheckForTurn()
    {
        //If we cant then we want to turn around and move in that direction
        Vector3 nextPosition = new Vector2(currentTile.X + Mathf.Round(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)), currentTile.Y + Mathf.Round(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)));
        if (!((nextPosition.x != -1 && nextPosition.x != GameManager.GRID_WIDTH) && (nextPosition.y != -1 && nextPosition.y != GameManager.GRID_HEIGHT) &&
        GameManager.obstaclePositions[(int)nextPosition.x, (int)nextPosition.y] == null && GameManager.tileBoard[(int)nextPosition.x, (int)nextPosition.y].walkable))
        {
            if(targetRotation.z > 180)
                targetRotation = new Vector3(targetRotation.x, targetRotation.y, targetRotation.z - 180);
            else
                targetRotation = new Vector3(targetRotation.x, targetRotation.y, targetRotation.z + 180);
        }
    }

    //Telling the enemy what to do when it finds the player
    private void FoundPlayer(Vector3 playerPos)
    {
        //We find the direction of the player and then walk towards it and look at it
        Vector3 dir = playerPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public override void MoveToTile()
    {
        base.MoveToTile();
        if (!moving)
            CheckForTurn();
    }
}
