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

public enum PlayerState
{ 
    MOVEMENT,
    ABILITYMOVE,
    ABILITYBURN,
    ABILITYFREEZE
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
    //public float SFXVolume
    //{
    //    get { return GetBusVolume(sfxPath, sfxVolume); }
    //    set { SetBusVolume(sfxPath, value); }
    //}

    //public float MusicVolume
    //{
    //    get { return GetBusVolume(musicPath, musicVolume); }
    //    set { SetBusVolume(musicPath, value); }
    //}

    //public float MasterVolume
    //{
    //    get { return GetBusVolume(masterPath, masterVolume); }
    //    set { SetBusVolume(masterPath, value); }
    //}
    #endregion

    #region Player
    //Player prefab
    public PlayerCamera playerCameraPrefab;
    private PlayerCamera playerCam;
    private Player player;
    private bool usingAbility;
    //Keeping track of which tiles our player can currently go to
    private List<Tile> availableTiles;

    // Keeps track of the current game state
    public static GameState currentGameState;

    // Keeps track of the current player state
    public static PlayerState currentPlayerState;

    #endregion

    #region Enemy
    private Enemy testEnemy;
    private EnemyManager enemyManager;
    public EnemyManager enemyManagerPrefab;
    #endregion

    private static bool objectSelected = false;
    private static Obstacle obstacleClicked = null;

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    #region TileBoard
    private Tile winTile;

    public static Tile[,] tileBoard = new Tile[GRID_WIDTH, GRID_HEIGHT];
    public static Obstacle[,] obstaclePositions = new Obstacle[GRID_WIDTH, GRID_HEIGHT];
    public ObstacleManager obstManagerPrefab;
    private ObstacleManager obstManager;

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

        usingAbility = false;

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
        if (SceneManager.GetActiveScene().name == "SamTestScene" || SceneManager.GetActiveScene().name == "WillTestScene")
        {
            if (currentGameState == GameState.PLAYERTURN)
            {
                playerCam.transform.position = Vector2.Lerp(playerCam.transform.position, player.transform.position, .05f);
                playerCam.transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);

                switch (currentPlayerState)
                {
                    case PlayerState.MOVEMENT:
                        //Checking for player right click
                        if (Input.GetMouseButtonDown(1))
                        {
                            RaycastHit2D hit = MouseCollisionCheck();


                            Tile tileClicked = hit.collider.GetComponent<Tile>();
                            if (availableTiles.Contains(tileClicked))
                            {
                                //Reseting the color of tiles before moving
                                foreach (Tile t in availableTiles)
                                {
                                    t.GetComponent<SpriteRenderer>().color = Color.gray;
                                }
                                //Clearing the available tile list and setting the new tile
                                availableTiles.Clear();

                                player.currentTile = tileClicked;

                                player.moving = true;
                                player.actionPoints -= Mathf.Round(tileClicked.dist);

                                //Checking if the player can still move
                                if (player.actionPoints > 1)
                                {
                                    FindAvailableTiles();
                                }
                            }
                        }

                        //When player isn't moving and their actionpoints is below 1 we go to the enemies turn
                        if (!player.moving && player.actionPoints < 1)
                        {
                            //if(player.currentTile.destination)
                            //{
                            //    currentGameState = GameState.WIN;
                            //    Debug.Log("PLAYER WINS");
                            //    return;
                            //}

                            if (OnWinTile(player.currentTile))
                            {
                                player.GetComponent<SpriteRenderer>().color = Color.magenta;
                                Debug.Log("YOU WIN");
                                currentGameState = GameState.WIN;
                                return;
                            }

                            currentGameState = GameState.ENEMYTURN;
                            testEnemy.actionPoints = 2;
                        }
                        break;

                    case PlayerState.ABILITYMOVE:

                        if (usingAbility)
                        {
                            if (objectSelected)
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    RaycastHit2D hit = MouseCollisionCheck();

                                    Tile tileClicked = hit.collider.GetComponent<Tile>();

                                    obstacleClicked.transform.position = tileClicked.transform.position;
                                    obstacleClicked.GetComponent<SpriteRenderer>().color = Color.white;

                                    Debug.Log("Obstacle Changed Position");
                                    objectSelected = false;
                                    usingAbility = false;
                                }
                            }
                            else
                            {
                                obstacleClicked = null;

                                //Checking for player LEFT click
                                if (Input.GetMouseButtonDown(0))
                                {
                                    RaycastHit2D hit = MouseCollisionCheck();

                                    obstacleClicked = hit.collider.GetComponent<Obstacle>();

                                    if (obstacleClicked != null)
                                    {
                                        Debug.Log("ObstacleClicked Position: " + obstacleClicked.X + ", " + obstacleClicked.Y);
                                        Debug.Log("Select a tile to move the obstacle");
                                        obstacleClicked.GetComponent<SpriteRenderer>().color = Color.blue;
                                        objectSelected = true;
                                    }
                                    else
                                    {
                                        Debug.Log("Object Was Not An Obstacle");
                                    }
                                }
                            }
                        }
                        else
                        {
                            currentPlayerState = PlayerState.MOVEMENT;
                        }
                        break;

                    case PlayerState.ABILITYBURN:
                        break;

                    case PlayerState.ABILITYFREEZE:
                        break;
                }

                if (player.actionPoints >= 1 && !player.moving && availableTiles.Count == 0)
                {
                    FindAvailableTiles();
                }

                //When player isn't moving and their actionpoints is below 1 we go to the enemies turn
                if (!player.moving && player.actionPoints < 1)
                {
                    currentGameState = GameState.ENEMYTURN;
                    testEnemy.actionPoints = 1;
                }
            }
            //Taking care of our enemy's turn
            else if (currentGameState == GameState.ENEMYTURN)
            {
                playerCam.transform.position = Vector2.Lerp(playerCam.transform.position, testEnemy.transform.position, .04f);
                playerCam.transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
                float distance = Vector2.Distance(playerCam.transform.position, testEnemy.transform.position);
                if (distance < .2f)
                {
                    if (testEnemy.actionPoints >= 1 && !testEnemy.moving)
                        testEnemy.EnemyTurn();
                    else if (!testEnemy.moving)
                    {
                        currentGameState = GameState.PLAYERTURN;
                        OnPlayersTurn();
                    }
                }
            }
            else if (currentGameState == GameState.LOSE)
            {
                Debug.Log("LOSER LOL!!!!");
            }
            else if (currentGameState == GameState.WIN)
            {
                Debug.Log("WINNER");
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
            currentPlayerState = PlayerState.MOVEMENT;

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.currentTile = tileBoard[(int)(player.transform.position.x + 9.5f), (int)(player.transform.position.y + 5.5f)];
            player.transform.position = player.currentTile.transform.position;

            winTile = GameObject.FindGameObjectWithTag("WinTile").GetComponent<Tile>();
            winTile.destination = true;
            winTile.GetComponent<SpriteRenderer>().color = Color.yellow;


            LoadInEnemies();
            LoadInObstacles();

            // For now, testEnemy is the first enemy in Enemies
            testEnemy = enemyManager.Enemies[0];
        }
    }

    public void OnPlayersTurn()
    {
        //Setting the GameState to playerturn state
        currentGameState = GameState.PLAYERTURN;
        player.actionPoints = 1f;
    }

    private void FindAvailableTiles()
    {
        //This is our openList
        List<Tile> openList = new List<Tile>();

        //We want to start our search for available tiles with the player's tile since all of them will be near the player
        openList.Add(player.currentTile);

        Tile currentTile;

        //Distance of our player to the point
        float distance;

        List<Vector2> positions = new List<Vector2>();
        foreach (Enemy e in enemyManager.Enemies)
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
            distance = (int)Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(player.currentTile.X, player.currentTile.Y)));

            if (currentTile.X != GRID_WIDTH - 1 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X + 1, currentTile.Y].walkable == true)
                {
                    tileBoard[currentTile.X + 1, currentTile.Y].dist = distance;
                    openList.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                    availableTiles.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                }
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
                if (tileBoard[currentTile.X - 1, currentTile.Y].walkable == true)
                {
                    tileBoard[currentTile.X - 1, currentTile.Y].dist = distance;
                    openList.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
                    availableTiles.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
                }
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
                if (tileBoard[currentTile.X, currentTile.Y + 1].walkable == true)
                {
                    tileBoard[currentTile.X, currentTile.Y + 1].dist = distance;
                    openList.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
                    availableTiles.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
                }
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
                if (tileBoard[currentTile.X, currentTile.Y - 1].walkable == true)
                {
                    tileBoard[currentTile.X, currentTile.Y - 1].dist = distance;
                    openList.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
                    availableTiles.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
                }
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

    private void LoadInEnemies()
    {
        // Instantiate the enemyManager Prefab
        enemyManager = Instantiate<EnemyManager>(enemyManagerPrefab);


        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");

        // loop to create new enemy prefabs
        for (int i = 0; i < enemyArray.Length; i++) 
        {
            // Instantiate at correct positions
            Enemy newEnemy = enemyArray[i].GetComponent<Enemy>();
            newEnemy.currentTile = tileBoard[(int)(newEnemy.transform.position.x + 9.5f), (int)(newEnemy.transform.position.y + 5.5f)];

            // Create unique name for enemy
            newEnemy.name = newEnemy.name + i;

            // Add enemy to enemyManager
            enemyManager.Enemies.Add(newEnemy);
        }
    }

    private void LoadInObstacles()
    {
        //Setting up our obstacle manager
        obstManager = Instantiate<ObstacleManager>(obstManagerPrefab);

        //Finding our obstacles
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        //Looping through our obstacles
        for(int i = 0; i < obstacles.Length; i++)
        {
            //Adding the obstacles to the list
            Obstacle newObstacle = obstacles[i].GetComponent<Obstacle>();
            obstManager.obstacles.Add(newObstacle);
        }
    }

    #region ABILITY METHODS
    public void MoveAbility()
    {
        Debug.Log("Moving Object");
        usingAbility = true;
        currentPlayerState = PlayerState.ABILITYMOVE;
    }
    #endregion

    #region HELPER METHODS
    private RaycastHit2D MouseCollisionCheck()
    {
        //Projecting a ray at the mouse and checking if it hit a collider
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, new Vector2(0, 0));

        return hit;
    }

    #endregion

// Returns true if the currentTile is the win tile, false otherwise
private bool OnWinTile(Tile currentTile)
 {
     return (currentTile.Y == winTile.Y && currentTile.X == winTile.X);
 }

    /*      

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
    */

}
