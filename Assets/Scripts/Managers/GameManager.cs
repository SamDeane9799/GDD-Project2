using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum GameState
{
    NONE,
    PAUSED,
    PLAYERTURN,
    ENEMYTURN,
    WIN,
    LOSE
}
public class GameManager : MonoBehaviour
{
    public const int GRID_WIDTH = 20;
    public const int GRID_HEIGHT = 12;

    #region Properties OLD
    ////Get and set properties for the volume levels to make it easier to read and code
    //public static float soundFXVolume
    //{
    //    get { return FMODUnity.RuntimeManager.GetBus("bus:/SFX").getVolume(out SFXVolume); }
    //    set { soundSettings[1] = value; }
    //}
    //public static float musicVolume
    //{
    //    get { return soundSettings[2]; }
    //    set { soundSettings[2] = value; }
    //}
    //public static float masterVolume
    //{
    //    get { return soundSettings[0]; }
    //    set { soundSettings[0] = value; }
    //}
    #endregion

    #region Sound Variables
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float sfxVolume;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float musicVolume;
    [SerializeField]
    [Range(0.0f, 100.0f)]
    private float masterVolume;

    private const string sfxPath = "bus:/SFX";
    private const string musicPath = "bus:/Music";
    private const string masterPath = "bus:/";

    //public static List<AudioSource> musicSources;
    //public static List<AudioSource> soundFXSources;

    //private static List<float> soundSettings;
    #endregion

    #region Sound Properties
    public float SFXVolume
    {
        get { return GetBusVolume(sfxPath, sfxVolume); }
        set { SetBusVolume(sfxPath, value); }
    }

    public float MusicVolume
    {
        get { return GetBusVolume(musicPath, musicVolume); }
        set { SetBusVolume(musicPath, value); }
    }

    public float MasterVolume
    {
        get { return GetBusVolume(masterPath, masterVolume); }
        set { SetBusVolume(masterPath, value); }
    }
    #endregion

    #region Player
    //Player prefab
    public PlayerCamera playerCameraPrefab;
    private PlayerCamera playerCam;
    public Player playerPrefab;
    private Player player;
    //Keeping track of which tiles our player can currently go to
    private List<Tile> availableTiles;
    public static GameState currentGameState;
    #endregion

    #region Enemy
    public Enemy enemyPrefab;
    private Enemy testEnemy;
    private EnemyManager enemyManager;
    public EnemyManager enemyManagerPrefab;
    #endregion

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    #region TileBoard
    public static Tile[,] tileBoard = new Tile[GRID_WIDTH, GRID_HEIGHT];
    public static Obstacle[,] obstaclePositions = new Obstacle[GRID_WIDTH, GRID_HEIGHT];
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        //Sets the game state and tell the game to not destroy this
        DontDestroyOnLoad(this);
        //SceneManager.sceneLoaded += OnLoad;
        OnLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        availableTiles = new List<Tile>();
        player.actionPoints = 2;
        OnPlayersTurn();

        //musicSources = new List<AudioSource>();
        //soundFXSources = new List<AudioSource>();

        ////Reading in our settings from a text file
        ////Here is one example using volume
        /*soundSettings = new List<float>(3);
        reader = new StreamReader(settingsPath);

        soundSettings.Add(float.Parse(reader.ReadLine()));
        soundSettings.Add(float.Parse(reader.ReadLine()));
        soundSettings.Add(float.Parse(reader.ReadLine()));

        reader.Close();*/
        //Initializing player
        InitializePlayerCam();
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "SamTestScene" || SceneManager.GetActiveScene().name == "WillTestScene")
        {
            if (currentGameState == GameState.PLAYERTURN)
            {
                //Checking for player right click
                if (Input.GetMouseButtonDown(1))
                {
                    //Projecting a ray at the mouse and checking if it hit a collider
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                    RaycastHit2D hit = Physics2D.Raycast(mousePosition, new Vector2(0, 0));

                    Tile tileClicked = hit.collider.GetComponent<Tile>();
                    if (availableTiles.Contains(tileClicked))
                    {
                        //Reseting the color of tiles before moving
                        foreach (Tile t in availableTiles)
                        {
                            t.GetComponent<SpriteRenderer>().color = new Color(0.6f, 1.0f, 0.6f);
                        }
                        //Clearing the available tile list and setting the new tile
                        availableTiles.Clear();

                        player.currentTile = tileClicked;
                        
                        player.moving = true;
                        player.actionPoints -= Mathf.Round(tileClicked.dist);

                        if (player.currentTile.destination)
                        {
                            currentGameState = GameState.WIN;
                            Debug.Log("WIN");
                            return;
                        }
                        //Checking if the player can still move
                    }
                }
                if (player.actionPoints >= 1 && !player.moving && availableTiles.Count == 0)
                {
                    FindAvailableTiles();
                }
                //When player isn't moving and their actionpoints is below 1 we go to the enemies turn
                if (!player.moving && player.actionPoints < 1)
                {
                    currentGameState = GameState.ENEMYTURN;
                    testEnemy.actionPoints = 2;                    
                }
            }
            //Taking care of our enemy's turn
            else if(currentGameState == GameState.ENEMYTURN)
            {
                if(testEnemy.actionPoints >= 1 && !testEnemy.moving)
                    testEnemy.EnemyTurn();
                else if(!testEnemy.moving)
                {
                    currentGameState = GameState.PLAYERTURN;
                    OnPlayersTurn();
                }
            }
            else if(currentGameState == GameState.LOSE)
            {
                Debug.Log("LOSER LOL!!!!");
            }
        }
    }


    //Sets up our player in the game
    //For testing purposes I am setting it up in the title screen so I can test audio
    public void InitializePlayerCam()
    {
        playerCam = Instantiate<PlayerCamera>(playerCameraPrefab);
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
        //Using this to load in the player when we load into the specific scene
        if (scene.name == "SamTestScene" || scene.name == "WillTestScene")
        {
            currentGameState = GameState.PLAYERTURN;
            player = Instantiate(playerPrefab);
            player.currentTile = tileBoard[0, 0];
            player.transform.position = player.currentTile.transform.position;

            testEnemy = Instantiate<Enemy>(enemyPrefab);
            testEnemy.currentTile = tileBoard[5, 7];
            testEnemy.transform.position = testEnemy.currentTile.transform.position;
            enemyManager = Instantiate<EnemyManager>(enemyManagerPrefab);
            enemyManager.Enemies.Add(testEnemy);
        }
    }

    public void OnPlayersTurn()
    {
        //Setting the GameState to playerturn state
        currentGameState = GameState.PLAYERTURN;
        player.actionPoints = 2f;
    }

    public static Vector2 WorldToGamePoint(Vector2 point)
    {
        return new Vector2((int)(point.x + 9.5f), (int)(point.y + 5.5f));
    }

    public static Vector2 WorldToGamePoint(float pointX, float pointY)
    {
        return new Vector2((int)(pointX + 9.5), (int)(pointY + 5.5));
    }

    private void FindAvailableTiles()
    {
        //This is our openList
        List<Tile> openList = new List<Tile>();

        //We want to start our search for available tiles with the player's tile since all of them will be near the player
        openList.Add(player.currentTile);

        // Checks if player is on win tile and "ends the game"
        // Currently "ends the game" means stopping the search for available tiles
 /*       if (OnWinTile(player.currentTile))
        {
            Debug.Log("You Win!");
            return;
        }*/

        Tile currentTile;

        //Distance of our player to the point
        float distance;

        List<Vector2> positions = new List<Vector2>();
        foreach(Enemy e in enemyManager.Enemies)
        {
            positions.Add(new Vector2(e.X, e.Y));
        }

        Vector2 posToBeChecked = new Vector2();

        //We iterate through till we have nothing viable anymore
        while (openList.Count > 0)
        {
            currentTile = openList[0];

            //Debug.Log(currentTile.X + " | " + currentTile.Y);

            //Handling our X positive neighbor
            //Checking to make sure the X doesn't go under 0, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X + 1, currentTile.Y);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(player.currentTile.X, player.currentTile.Y)));
            if (currentTile.X != GRID_WIDTH - 1 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                tileBoard[currentTile.X + 1, currentTile.Y].dist = distance;
                openList.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                availableTiles.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
            }

            //Handling our X negative neighbor
            //Checking to make sure the X doesn't go over our limit, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X - 1, currentTile.Y);
            distance = Mathf.Abs(Vector2.Distance(new Vector2(currentTile.X - 1, currentTile.Y), new Vector2(player.currentTile.X, player.currentTile.Y)));
            if (currentTile.X != 0 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                tileBoard[currentTile.X - 1, currentTile.Y].dist = distance;
                openList.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
                availableTiles.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
            }

            //Handling our Y Positve neighbor
            //Checking to make sure the Y doesn't go over our limit, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X, currentTile.Y + 1);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(player.currentTile.X, player.currentTile.Y)));
            if (currentTile.Y != GRID_HEIGHT - 1 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                tileBoard[currentTile.X, currentTile.Y + 1].dist = distance;
                openList.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
                availableTiles.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
            }

            //Handling our y negative neighbor
            //Checking to make sure the Y doesn't go under 0, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X, currentTile.Y - 1);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(player.currentTile.X, player.currentTile.Y)));
            if (currentTile.Y != 0 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                tileBoard[currentTile.X, currentTile.Y - 1].dist = distance;
                openList.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
                availableTiles.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
            }

            //After checking all neighbors we remove the tile we just checked and then go to the next one to check it's neighbors(If there is another tile on the openlist)
            openList.RemoveAt(0);
        }

        //Changing all the tiles to a yellow color
        foreach (Tile t in availableTiles)
        {
            t.GetComponent<SpriteRenderer>().color = new Color(Color.white.r, Color.white.g, Color.white.b, .85f);
        }
    }

    // Returns true if the currentTile is the win tile, false otherwise
   /* private bool OnWinTile(Tile currentTile)
    {
        return (currentTile.Y == winTilePostion.Y && currentTile.X == winTilePostion.X);
    }*/

    #region Sound Methods

        // Sets the volume of a specific bus to the given volume
        private void SetBusVolume(string busPath, float volume)
    {
        FMODUnity.RuntimeManager.GetBus(busPath).setVolume(volume);
    }

    // Gets the volume of a specific bus and assigns it to a given variable
    private float GetBusVolume(string busPath, float busVariable)
    {
        FMODUnity.RuntimeManager.GetBus(busPath).getVolume(out busVariable);

        return busVariable;
    }

    #endregion
}
