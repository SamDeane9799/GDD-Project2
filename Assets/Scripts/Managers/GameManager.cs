using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private List<WaterTile> availableWater;
    #endregion

    #region File IO
    private StreamReader reader;
    private const string settingsPath = "Assets/txt/settings.txt";
    #endregion

    #region TileBoard
    private Tile winTile;
    public static int GRID_WIDTH = 21;
    public static int GRID_HEIGHT = 13;

    public static Tile[,] tileBoard = new Tile[GRID_WIDTH, GRID_HEIGHT];
    public static Obstacle[,] obstaclePositions = new Obstacle[GRID_WIDTH, GRID_HEIGHT];
    public ObstacleManager obstManagerPrefab;
    private ObstacleManager obstManager;

    [SerializeField]
    private GameObject RockPrefab;
    #endregion

    #region Button Vars

    private List<Button> abilityButtons;
    #endregion

    private FMOD.Studio.EventInstance instance;

    // Start is called before the first frame update
    void Start()
    {
        //Sets the game state and tell the game to not destroy this
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnLoad;
        if(SceneManager.GetActiveScene().name != "StartScene")
            OnLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        availableObstacles = new List<Obstacle>();
        availableBushes = new List<Bush>();
        availableWater = new List<WaterTile>();

        availableTiles = new List<Tile>();

        obstacleAvailableTiles = new List<Tile>();
        usingAbility = false;

        // FMOD music playing
        instance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/Gameplay");
        instance.start();

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
        if(playerCam == null)
            InitializePlayerCam();

        abilityButtons = new List<Button>();
        abilityButtons.Add(playerCam.moveButton);
        abilityButtons.Add(playerCam.burnButton);
        abilityButtons.Add(playerCam.freezeButton);

        SetUpAbilityButtons();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentGameState);
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            if (currentGameState == GameState.PLAYERTURN)
            {
                Debug.Log(currentPlayerState);
                switch (currentPlayerState)
                {
                    case PlayerState.MOVEMENT:
                        //Checking for player right click
                        if (!player.moving && availableTiles.Count == 0)
                        {
                            FindAvailableTiles();
                        }

                        if (Input.GetMouseButtonDown(0))
                        {
                            //Casting a ray where the player clicked
                            RaycastHit2D hit = MouseCollisionCheck();

                            if (hit)
                            {
                                Tile tileClicked = hit.collider.GetComponent<Tile>();

                                if (availableTiles.Contains(tileClicked))
                                {
                                    //Reseting the color of tiles before moving
                                    ClearAvailableTileList(availableTiles);

                                    player.currentTile = tileClicked;

                                    player.moving = true;
                                    player.actionPoints -= tileClicked.dist;
                                }
                            }
                        }

                        //When player isn't moving and their actionpoints is below 1 we go to the enemies turn
                        if (!player.moving && player.actionPoints < 1)
                        {
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
                                    //Casting a ray for a tile 
                                    RaycastHit2D hit = MouseCollisionCheck();

                                    Tile tileClicked = hit.collider.GetComponent<Tile>();
                                    //If the obstacle can move to the tile then we move it there
                                    if (obstacleAvailableTiles.Contains(tileClicked))
                                    {
                                        // Remove the obstacle's original position from the obstaclesPosition array

                                        obstaclePositions[obstacleClicked.X, obstacleClicked.Y] = null;
                                        Debug.Log(obstaclePositions[obstacleClicked.X, obstacleClicked.Y]);
                                        //obstacleClicked.MoveToCell(new Vector3Int((int)(tileClicked.transform.position.x), (int)(tileClicked.transform.position.y), (int)tileClicked.transform.position.z));
                                        obstacleClicked.transform.position = tileClicked.transform.position;
                                        obstacleClicked.GetComponent<SpriteRenderer>().color = Color.white;

                                        // Put the obstacleClicked into the obstclePositions array with its new coordinates
                                        obstaclePositions[tileClicked.X, tileClicked.Y] = obstacleClicked;


                                        //Clearing the obstacles available tiles and subtracting an action point
                                        ClearAvailableTileList(obstacleAvailableTiles);
                                        player.actionPoints -= 1;

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
                                    //Casting a ray on the terrain layer
                                    RaycastHit2D hit = MouseCollisionCheck(1 << 9);

                                    if (hit)
                                    {
                                        obstacleClicked = hit.collider.GetComponent<Obstacle>();

                                        //If we hit then we reset our obstacle list and set the object selected
                                        if (obstacleClicked != null && availableObstacles.Contains(obstacleClicked))
                                        {
                                            ClearAvailableObstaclesList();
                                            //Checking where the obstacle selected can be moved to
                                            FindAvailableSpotsObst(obstacleClicked);
                                            obstacleClicked.GetComponent<SpriteRenderer>().color = Color.blue;
                                            objectSelected = true;
                                            FMODUnity.RuntimeManager.PlayOneShot("event:/Abilities/Telekenesis/Lift");
                                        }
                                    }
                                }
                            }

                            if (Input.GetKeyDown(KeyCode.Q))
                            {
                                usingAbility = false;
                                playerCam.moveButton.interactable = true;
                                ClearAvailableObstaclesList();
                            }
                        }
                        else
                        {
                            currentPlayerState = PlayerState.MOVEMENT;
                            ClearAvailableObstaclesList();
                        }
                        break;

                    case PlayerState.ABILITYBURN:

                        if (obstManager.bushes.Count > 0)
                        {
                            if (usingAbility)
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    RaycastHit2D hit = MouseCollisionCheck();
                                    if (hit)
                                    {
                                        Bush bushClicked = hit.collider.GetComponent<Bush>();

                                        if (obstManager.bushes.Contains(bushClicked))
                                        {

                                            FMODUnity.RuntimeManager.PlayOneShot("event:/Abilities/Burn");

                                            //bushClicked.burned = true;

                                            obstaclePositions[bushClicked.X, bushClicked.Y] = null;

                                            Destroy(bushClicked.gameObject);

                                            FindAvailableTiles();

                                            usingAbility = false;
                                        }
                                        else
                                        {
                                        }
                                    }
                                }

                                if (Input.GetKeyDown(KeyCode.Q))
                                {
                                    usingAbility = false;
                                    playerCam.burnButton.interactable = true;
                                    ClearAvailableBushesList();
                                }
                            }
                            else
                            {
                                currentPlayerState = PlayerState.MOVEMENT;
                            }
                        }

                        break;

                    case PlayerState.ABILITYFREEZE:
                        if (obstManager.waterTiles.Count > 0)
                        {
                            if (usingAbility)
                            {
                                if (Input.GetMouseButton(0))
                                {
                                    RaycastHit2D hit = MouseCollisionCheck();

                                    if (hit)
                                    {
                                        WaterTile waterClicked = hit.collider.GetComponent<WaterTile>();

                                        if (obstManager.waterTiles.Contains(waterClicked))
                                        {

                                            FMODUnity.RuntimeManager.PlayOneShot("event:/Abilities/Freeze");

                                            waterClicked.GetComponent<SpriteRenderer>().color = Color.cyan;
                                            waterClicked.walkable = true;

                                            GameObject newObstacle = Instantiate(RockPrefab);
                                            newObstacle.transform.position = waterClicked.transform.position;
                                            obstManager.obstacles.Add(newObstacle.GetComponent<Obstacle>());

                                            //bushClicked.burned = true;

                                            FindAvailableTiles();

                                            usingAbility = false;
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                            }
                            else
                            {
                                currentPlayerState = PlayerState.MOVEMENT;
                            }
                        }
                        else
                        {
                        }

                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                            usingAbility = false;
                            playerCam.freezeButton.interactable = true;
                            ClearAvailableWaterList();
                        }

                        break;
                }

                //If the player has action points and nowhere to move then we should be looking for available tiles


                //If the player is no longer moving and does not have an action point then we cycle to the enemies turn
                if(!player.moving && player.currentTile.destination)
                {
                    player.GetComponent<SpriteRenderer>().color = Color.magenta;
                    Debug.Log("YOU WIN");
                    currentGameState = GameState.WIN;
                    playerCam.Level += 1;
                    LoadNextScene();
                    return;
                }
                if (enemyManager.Enemies.Count == 0)
                {
                    OnPlayersTurn();
                }
                else if (!player.moving && player.actionPoints < 1)
                {
                    OnEnemyTurn();
                }
            }

            //Taking care of our enemy's turn
            //If it is the enemies turn we do this
            else if (currentGameState == GameState.ENEMYTURN)
            {
                //If it is no longer the enemies turn we switch to the players turn and reset their values
                if (!enemyManager.enemyTurn)
                {
                    OnPlayersTurn();
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




    #region Searching Methods
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

        if (obstManager.bushes.Count > 0)
        {
            foreach (Bush b in obstManager.bushes)
            {
                distance = Vector2.Distance(b.transform.position, player.currentTile.transform.position);

                if (distance <= player.moveDist)
                {
                    availableBushes.Add(b);
                    b.highlight = true;
                }
            }
        }
    }

    private void FindAvailableWater()
    {
        float distance;

        if (obstManager.waterTiles.Count > 0)
        {
            foreach (WaterTile water in obstManager.waterTiles)
            {
                distance = Vector2.Distance(water.transform.position, player.currentTile.transform.position);

                if (distance <= player.moveDist)
                {
                    availableWater.Add(water);
                    water.highlight = true;
                }
            }
        }
    }

    #endregion

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

        GameObject[] waters = GameObject.FindGameObjectsWithTag("Water");

        //Looping through our obstacles
        for (int i = 0; i < obstacles.Length; i++)
        {
            //Adding the obstacles to the list
            Obstacle newObstacle = obstacles[i].GetComponent<Obstacle>();
            obstManager.obstacles.Add(newObstacle);
        }

        for (int i = 0; i < bushes.Length; i++)
        {
            //Adding the obstacles to the list
            Bush newBush = bushes[i].GetComponent<Bush>();
            obstManager.bushes.Add(newBush);
        }

        for (int i = 0; i < waters.Length; i++)
        {
            //Adding the obstacles to the list
            WaterTile newWater = waters[i].GetComponent<WaterTile>();
            obstManager.waterTiles.Add(newWater);
        }
    }
    public void InitializePlayerCam()
    {
        playerCam = Instantiate<PlayerCamera>(playerCameraPrefab);
    }

    public void OnLoad(Scene scene, LoadSceneMode mode)
    {
        //Using this to load in the player when we load into the specific scene
        if (scene.name != "StartScene")
        {
            Debug.Log("Called");
            //Setting the player state and current game state
            currentGameState = GameState.PLAYERTURN;
            currentPlayerState = PlayerState.MOVEMENT;

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.currentTile = tileBoard[(int)(player.transform.position.x + (float)(GRID_WIDTH/2)), (int)(player.transform.position.y + (float)(GRID_HEIGHT/2))];
            player.transform.position = player.currentTile.transform.position;


            winTile = GameObject.FindGameObjectWithTag("WinTile").GetComponent<Tile>();
            winTile.destination = true;
            winTile.GetComponent<SpriteRenderer>().color = Color.yellow;


            LoadInEnemies();
            LoadInObstacles();

            currentGameState = GameState.PLAYERTURN;
            player.actionPoints = 1f;
        }
    }

    #region BUTTON METHODS
    public void MoveAbility()
    {
        usingAbility = true;
        ClearAvailableTileList(availableTiles);
        ClearAvailableBushesList();
        ClearAvailableWaterList();
        FindAvailableObstacles();
        currentPlayerState = PlayerState.ABILITYMOVE;
        SetButtonOff(playerCam.moveButton);
    }

    public void BurnAbility()
    {
        usingAbility = true;
        ClearAvailableTileList(availableTiles);
        ClearAvailableWaterList();
        ClearAvailableObstaclesList();
        FindAvailableBushes();
        currentPlayerState = PlayerState.ABILITYBURN;
        SetButtonOff(playerCam.burnButton);
    }

    public void FreezeAbility()
    {
        usingAbility = true;
        FindAvailableWater();
        ClearAvailableTileList(availableTiles);
        ClearAvailableBushesList();
        ClearAvailableObstaclesList();
        currentPlayerState = PlayerState.ABILITYFREEZE;
        SetButtonOff(playerCam.freezeButton);
    }

    public void PassButton()
    {
        currentGameState = GameState.ENEMYTURN;
        enemyManager.enemyTurn = true;
        enemyManager.OnEnemyTurn();
        player.actionPoints = 0;
        ClearAvailableTileList(availableTiles);
    }

    public void ResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        Destroy(playerCam.gameObject);
        Destroy(gameObject);
    }

    private void SetUpAbilityButtons()
    {
        playerCam.moveButton.onClick.AddListener(MoveAbility);
        playerCam.freezeButton.onClick.AddListener(FreezeAbility);
        playerCam.burnButton.onClick.AddListener(BurnAbility);
        playerCam.passButton.onClick.AddListener(PassButton);
        playerCam.resetButton.onClick.AddListener(ResetButton);
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

    private void ClearAvailableTileList(List<Tile> listToClear)
    {
        foreach (Tile t in listToClear)
        {
            t.highlight = false;
            t.GetComponent<SpriteRenderer>().color = t.originalColor;
        }

        listToClear.Clear();
    }

    private void ClearAvailableObstaclesList()
    {
        foreach (Obstacle o in availableObstacles)
        {
            o.highlight = false;
            o.GetComponent<SpriteRenderer>().color = o.originalColor;
        }

        availableObstacles.Clear();
    } 

    private void ClearAvailableBushesList()
    {
        foreach(Bush b in availableBushes)
        {
            b.highlight = false;
            b.ResetColorValues();
        }
    }

    private void ClearAvailableWaterList()
    {
        foreach(WaterTile w in availableWater)
        {
            w.highlight = false;
            w.ResetColorValues();
        }
    }
    
    // Returns true if the currentTile is the win tile, false otherwise
    private bool OnWinTile(Tile currentTile)
    {
        return (currentTile.Y == winTile.Y && currentTile.X == winTile.X);
    }
    public void OnPlayersTurn()
    {
        //Setting the GameState to playerturn state
        currentGameState = GameState.PLAYERTURN;
        if (abilityButtons != null)
        {
            foreach (Button b in abilityButtons)
            {
                b.gameObject.SetActive(true);
                b.interactable = true;
            }
        }
        playerCam.passButton.gameObject.SetActive(true);
        playerCam.passButton.interactable = true;
        player.actionPoints = 1f;
    }

    public void OnEnemyTurn()
    {
        //The current gamestate is switched to the enemyturn. We call the OnEnemyTurn to reset their values for their turn
        currentGameState = GameState.ENEMYTURN;
        enemyManager.OnEnemyTurn();
        enemyManager.enemyTurn = true;
        foreach(Button b in abilityButtons)
        {
            b.gameObject.SetActive(false);
        }
        playerCam.passButton.gameObject.SetActive(false);
    }

    //Disables a button 
    //Mainly used when the button itself is clicked
    private void SetButtonOff(Button buttonToSetOff)
    {
        foreach(Button b in abilityButtons)
        {
            if (b == buttonToSetOff)
                b.interactable = false;
            else
                b.interactable = true;
        }
    }
    
    private void LoadNextScene()
    {
        SceneManager.LoadScene(playerCam.Level);
        playerCam.UpdateLevels();
    }
    #endregion



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
