using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Start is called before the first frame update
    #region Turn Management Variables
    public bool enemyTurn;
    private int index = 0;
    #endregion
    #region Enemy Search Variables
    private const int RAYSPROJECTED = 1;
    public bool enemiesSearch;
    #endregion
    #region Enemy List Variables
    public List<Enemy> Enemies
    {
        get { return enemies; }
    }
    [SerializeField]
    private List<Enemy> enemies = new List<Enemy>();
    #endregion

    void Start()
    {
        //All enemies will search for now
        enemiesSearch = true;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Enemy e in enemies)
        {
            //If our enemy has its search variable on then it should be searching for the player
            e.DetectPlayer(RAYSPROJECTED);

        }
        //If it is the enemies turn we do this
        if (enemyTurn)
        {
            //If the enemy has an action point then they should take their turn
            if (enemies[index].actionPoints > 0)
                enemies[index].EnemyTurn();
            //If they don't have an action point and they arent moving then we should go to the next enemy
            else if (!enemies[index].moving)
                index++;
            //If we have iterated past our list we should return
            if (index >= enemies.Count)
            {
                enemyTurn = false;
                index = 0;
                return;
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
    }
}
