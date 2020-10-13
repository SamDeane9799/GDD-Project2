using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Start is called before the first frame update
    public bool enemiesSearch;
    public bool enemyTurn;

    private Vector2 averageEnemyPosition;

    public Vector2 AverageEnemyPosition
    {
        get { return averageEnemyPosition; }
    }

    private const int RAYSPROJECTED = 8;
    public List<Enemy> Enemies
    {
        get { return enemies; }
    }

    private List<Enemy> enemies = new List<Enemy>();
    void Start()
    {
        enemiesSearch = true;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Enemy e in enemies)
        {
            //If our enemy has its search variable on then it should be searching for the player
            if (enemiesSearch)
            {
                e.DetectPlayer(RAYSPROJECTED);
            }
        }
        //If it is the enemies turn we do this
        if (enemyTurn)
        {
            //Loop through each enemy only continuing to the next one once the previous one finished moving
            for (int i = 0; i < enemies.Count;)
            {
                //If the enemy has an action point then they should take their turn
                if (enemies[i].actionPoints > 0)
                    enemies[i].EnemyTurn();
                //If they don't have an action point and they arent moving then we should go to the next enemy
                else if(!enemies[i].moving )
                    i++;
                //If we have iterated past our list we should return
                if (i >= enemies.Count)
                {
                    enemyTurn = false;
                    return;
                }
            }
        }
    }

    public void OnEnemyTurn()
    {
        //Reseting each enemies action points
        foreach(Enemy e in enemies)
        {
            e.actionPoints = 1;
        }

        CalculateAvgPosition();
    }

    private void CalculateAvgPosition()
    {
        averageEnemyPosition = new Vector2(0, 0);
        foreach(Enemy e in enemies)
        {
            averageEnemyPosition += new Vector2(e.transform.position.x, e.transform.position.y);
        }

        averageEnemyPosition /= enemies.Count;
    }
}
