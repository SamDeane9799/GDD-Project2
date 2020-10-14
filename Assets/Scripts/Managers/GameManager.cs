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
    public Color tileColor = new Color(.79f, .83f, .79f);

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

    #region Obstacles
    private static bool objectSelected = false;
    private static Obstacle obstacleClicked = null;

    private List<Tile> obstacleAvailableTiles;
    private const int obstacleMovementRange = 1;

    private List<Obstacle> availableObstacles;
    private List<Bush> availableBushes;
    #endregion

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    #region TileBoard
    private Tile winTile;

    public static Tile[,] tileBoard = new Tile[GRID_WIDTH, GRID_HEIGHT];
    public static Obstacle[,] obstaclePositions = new Obstacle[GRID_WIDTH, GRID_HEIGHT];
    public static Bush[,] bushPositions = new Bush[GRID_WIDTH, GRID_HEIGHT];
    public ObstacleManager obstManagerPrefab;
    private ObstacleManager obstManager;

    #endregion

    private FMOD.Studio.EventInstance instance;


    // Start is called before the first frame update
    void Start()
    {
        //Sets the game state and tell the game to not destroy this
        DontDestroyOnLoad(this);
        //SceneManager.sceneLoaded += OnLoad;
        OnLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        availableObstacles = new List<Obstacle>();

        availableTiles = new List<Tile>();

        obstacleAvailableTiles = new List<Tile>();
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
        if (SceneManager.GetActiveScene().name != "StartScene")
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
                                    t.highlight = false;
                                    t.GetComponent<SpriteRenderer>().color = t.originalColor;
                                }
                                //Clearing the available tile list and setting the new tile
                                availableTiles.Clear();

                                player.currentTile = tileClicked;

                                player.moving = true;
                                player.actionPoints -= tileClicked.dist;
                            }
                        }

                        //When player isn't moving and their actionpoints is below 1 we go to the enemies turn
                        if (!player.moving && player.actionPoints < 1)
                        {
                            if (OnWinTile(player.currentTile))
                            {
                                player.GetComponent<SpriteRenderer>().color = Color.magenta;
                                Debug.Log("YOU WIN");
                                currentGameState = GameState.WIN;
                                return;
                            }

                            currentGameState = GameState.ENEMYTURN;
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
                                    if (obstacleAvailableTiles.Contains(tileClicked))
                                    {
                                        // Remove the obstacle's original position from the obstaclesPosition array
                                        obstaclePositions[obstacleClicked.X, obstacleClicked.Y] = null;

                                        obstacleClicked.transform.position = tileClicked.transform.position;
                                        obstacleClicked.GetComponent<SpriteRenderer>().color = Color.white;

                                        // Put the obstacleClicked into the obstclePositions array with its new coordinates
                                        obstaclePositions[obstacleClicked.X, obstacleClicked.Y] = obstacleClicked;

                                        Debug.Log("Obstacle Changed Position");

                                        foreach (Tile t in obstacleAvailableTiles)
                                        {
                                            t.highlight = false;
                                            t.GetComponent<SpriteRenderer>().color = t.originalColor;
                                        }

                                        FindAvailableTiles();

                                        obstacleAvailableTiles.Clear();
                                        FMODUnity.RuntimeManager.PlayOneShot("event:/Abilities/Telekenesis/Place");
                                        objectSelected = false;
                                        usingAbility = false;
                                    }
                                }
                            }
                            else
                            {
                                obstacleClicked = null;

                                //Checking for player LEFT click
                                if (Input.GetMouseButtonDown(0))
                                {
                                    RaycastHit2D hit = MouseCollisionCheck(1 << 9);

                                    obstacleClicked = hit.collider.GetComponent<Obstacle>();

                                    if (obstacleClicked != null && availableObstacles.Contains(obstacleClicked))
                                    {                                        
                                        foreach(Obstacle o in availableObstacles)
                                        {
                                            o.highlight = false;
                                            o.ResetColorValues();
                                        }
                                        availableObstacles.Clear();
                                        FindAvailableSpotsObst(obstacleClicked);
                                        obstacleClicked.GetComponent<SpriteRenderer>().color = Color.blue;
                                        objectSelected = true;
                                        FMODUnity.RuntimeManager.PlayOneShot("event:/Abilities/Telekenesis/Lift");
                                    }
                                    else
                                    {
                                        Debug.Log("Object Was Not An Obstacle");
                                    }
                                }
                            }

                            if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                usingAbility = false;
                                Debug.Log("Cancelled Move Object Ability");
                            }
                        }
                        else
                        {
                            currentPlayerState = PlayerState.MOVEMENT;
                        }
                        break;

                    case PlayerState.ABILITYBURN:
                        if (usingAbility)
                        {
                            Debug.Log("Select a bush");

                            RaycastHit2D hit = MouseCollisionCheck();

                            Bush bushClicked = hit.collider.GetComponent<Bush>();

                            if (bushClicked != null)
                            {
                                bushClicked.burned = true;
                            }
                            else
                            {
                                Debug.Log("Bush not clicked");
                            }

                            if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                usingAbility = false;
                                Debug.Log("Cancelled Move Object Ability");
                            }
                        }
                        else
                        {
                            currentPlayerState = PlayerState.MOVEMENT;
                        }

                        break;

                    case PlayerState.ABILITYFREEZE:
                        break;
                }


                if (player.actionPoints >= 1 && !player.moving && availableTiles.Count == 0)
                {
                    FindAvailableTiles();
                }

                //When player isn't moving and their actionpoints is below 1 we go to the enemies turn
                //If the player is no longer moving and does not have an action point then we cycle to the enemies turn
                if (!player.moving && player.actionPoints < 1)
                {
                    //The current gamestate is switched to the enemyturn. We call the OnEnemyTurn to reset their values for their turn
                    currentGameState = GameState.ENEMYTURN;
                    enemyManager.OnEnemyTurn();
                    enemyManager.enemyTurn = true;
                }
            }
            //Taking care of our enemy's turn
            //If it is the enemies turn we do this
            else if (currentGameState == GameState.ENEMYTURN)
            {
                if(enemyManager.Enemies.Count == 0)
                {
                    currentGameState = GameState.PLAYERTURN;
                    OnPlayersTurn();
                    return;
                }
                //Moving our camera to the average position of the enemies
                playerCam.transform.position = Vector2.Lerp(playerCam.transform.position, enemyManager.AverageEnemyPosition, .04f);
                playerCam.transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
                //Calculating the distance of the camera to the average positions of enemies
                float distance = Vector2.Distance(playerCam.transform.position, enemyManager.AverageEnemyPosition);
                if (distance < .2f)
                {
                    //If it is no longer the enemies turn we switch to the players turn and reset their values
                    if (!enemyManager.enemyTurn)
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
        if (scene.name != "StartScene")
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

            // FMOD music playing
            instance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/Gameplay");
            instance.start();

            OnPlayersTurn();
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

            //Handling our X positive neighbor
            //Checking to make sure the X doesn't go under 0, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X + 1, currentTile.Y);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(player.currentTile.X, player.currentTile.Y)));
            if (currentTile.X != GRID_WIDTH - 1
                && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null
                && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null
                && distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
                && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile  && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X + 1, currentTile.Y].walkable == true)
                {
                    tileBoard[currentTile.X + 1, currentTile.Y].dist = Mathf.Round(distance);
                    openList.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                    availableTiles.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                }
            }

            //Handling our X negative neighbor
            //Checking to make sure the X doesn't go over our limit, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X - 1, currentTile.Y);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(player.currentTile.X, player.currentTile.Y)));

            if (currentTile.X != 0 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= player.actionPoints && !availableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X - 1, currentTile.Y].walkable == true)
                {
                    tileBoard[currentTile.X - 1, currentTile.Y].dist = Mathf.Round(distance);
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
                    tileBoard[currentTile.X, currentTile.Y + 1].dist = Mathf.Round(distance);
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
                    tileBoard[currentTile.X, currentTile.Y - 1].dist = Mathf.Round(distance);
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
            t.ResetColorValues();
            t.highlight = true;
        }
    }

    private void FindAvailableSpotsObst(Obstacle obstToMove)
    {
        //This is our openList
        List<Tile> openList = new List<Tile>();

        //We want to start our search for available tiles with the player's tile since all of them will be near the player
        openList.Add(tileBoard[obstToMove.X, obstToMove.Y]);

        Tile currentTile;

        //Distance of our player to the point
        float distance;

        List<Vector2> positions = new List<Vector2>();
        foreach (Enemy e in enemyManager.Enemies)
        {
            positions.Add(new Vector2(e.X, e.Y));
        }
        positions.Add(new Vector2(player.X, player.Y));

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
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(obstToMove.X, obstToMove.Y)));

            if (currentTile.X != GRID_WIDTH - 1 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= obstacleMovementRange && !obstacleAvailableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X + 1, currentTile.Y].walkable == true)
                {
                    tileBoard[currentTile.X + 1, currentTile.Y].dist = distance;
                    openList.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                    obstacleAvailableTiles.Add(tileBoard[currentTile.X + 1, currentTile.Y]);
                }
            }

            //Handling our X negative neighbor
            //Checking to make sure the X doesn't go over our limit, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X - 1, currentTile.Y);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(obstToMove.X, obstToMove.Y)));
            if (currentTile.X != 0 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= obstacleMovementRange && !obstacleAvailableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X - 1, currentTile.Y].walkable == true)
                {
                    tileBoard[currentTile.X - 1, currentTile.Y].dist = distance;
                    openList.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
                    obstacleAvailableTiles.Add(tileBoard[currentTile.X - 1, currentTile.Y]);
                }
            }

            //Handling our Y Positve neighbor
            //Checking to make sure the Y doesn't go over our limit, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X, currentTile.Y + 1);
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(obstToMove.X, obstToMove.Y)));
            if (currentTile.Y != GRID_HEIGHT - 1 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= obstacleMovementRange && !obstacleAvailableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X, currentTile.Y + 1].walkable == true)
                {
                    tileBoard[currentTile.X, currentTile.Y + 1].dist = distance;
                    openList.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
                    obstacleAvailableTiles.Add(tileBoard[currentTile.X, currentTile.Y + 1]);
                }
            }

            //Handling our y negative neighbor
            //Checking to make sure the Y doesn't go under 0, To make sure there is an actual tile there, that there is not an obstacle there, The tile is in range of the player,
            //that the player is contained in the availabletiles and that the tile is not the one we started on
            posToBeChecked = new Vector2(currentTile.X, currentTile.Y - 1); 
            distance = Mathf.Abs(Vector2.Distance(posToBeChecked, new Vector2(obstToMove.X, obstToMove.Y)));
            if (currentTile.Y != 0 && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != null && obstaclePositions[(int)posToBeChecked.x, (int)posToBeChecked.y] == null &&
            distance <= obstacleMovementRange && !obstacleAvailableTiles.Contains(tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y])
            && tileBoard[(int)posToBeChecked.x, (int)posToBeChecked.y] != player.currentTile && !positions.Contains(posToBeChecked))
            {
                if (tileBoard[currentTile.X, currentTile.Y - 1].walkable == true)
                {
                    tileBoard[currentTile.X, currentTile.Y - 1].dist = distance;
                    openList.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
                    obstacleAvailableTiles.Add(tileBoard[currentTile.X, currentTile.Y - 1]);
                }
            }

            //After checking all neighbors we remove the tile we just checked and then go to the next one to check it's neighbors(If there is another tile on the openlist)
            openList.RemoveAt(0);
        }

        //Changing all the tiles to a yellow color
        foreach (Tile t in obstacleAvailableTiles)
        {
            t.ResetColorValues();
            t.highlight = true;
        }
    }

    private void FindAvailableObstacles()
    {
        float distance;
        foreach (Obstacle o in obstManager.obstacles)
        {
            distance = Vector2.Distance(o.transform.position, player.currentTile.transform.position);
            if (distance <= player.moveDist)
            {
                availableObstacles.Add(o);
                o.highlight = true;
            }
        }
    }

    private void FindAvailableBushes()
    {
        float distance;

        foreach (Bush o in obstManager.bushes)
        {
            distance = Vector2.Distance(o.transform.position, player.currentTile.transform.position);
            if (distance <= player.moveDist)
            {
                availableObstacles.Add(o);
                o.highlight = true;
            }
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
        GameObject[] bushes = GameObject.FindGameObjectsWithTag("Bush");

        //Looping through our obstacles
        for (int i = 0; i < obstacles.Length; i++)
        {
            //Adding the obstacles to the list
            Debug.Log("Found obstacle");
            Obstacle newObstacle = obstacles[i].GetComponent<Obstacle>();
            obstManager.obstacles.Add(newObstacle);
        }

        for (int i = 0; i < bushes.Length; i++)
        {
            //Adding the obstacles to the list
            Debug.Log("Found bush");
            Bush newBush = bushes[i].GetComponent<Bush>();
            obstManager.bushes.Add(newBush);
        }
    }

    #region ABILITY METHODS
    public void MoveAbility()
    {
        Debug.Log("Moving Object");
        usingAbility = true;
        FindAvailableObstacles();
        currentPlayerState = PlayerState.ABILITYMOVE;
    }

    public void BurnAbility()
    {
        Debug.Log("Burning Object");
        usingAbility = true;
        FindAvailableBushes();
        currentPlayerState = PlayerState.ABILITYBURN;
    }

    public void FreezeAbility()
    {
        Debug.Log("Burning Object");
        usingAbility = true;
        //FindAvailableWater();
        currentPlayerState = PlayerState.ABILITYFREEZE;
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
    private RaycastHit2D MouseCollisionCheck(int layerMask)
    {
        //Projecting a ray at the mouse and checking if it hit a collider
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, new Vector2(0, 0), 1.0f, layerMask);

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
