using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Start is called before the first frame update
    public bool enemiesSearch;

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
        foreach(Enemy e in enemies)
        {
            if (enemiesSearch)
            {
                e.DetectPlayer(RAYSPROJECTED);
            }
        }
    }
}
