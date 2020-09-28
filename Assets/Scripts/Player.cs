using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;

public class Player : MonoBehaviour
{
    #region UI Variables
    [Header("UI Variables")]
    public Canvas titleCanvas;
    public Canvas pauseCanvas;
    public Stack<Canvas> canvasTracker = new Stack<Canvas>();

    public Slider musicSoundVolumeSlider;
    public Slider soundfxVolumeSlider;
    public Slider masterSoundVolumeSlider;
    public Button applyButton;
    #endregion

    #region File IO
    private const string settingsPath = "Assets/txt/settings.txt";
    private StreamWriter writer;
    #endregion

    void Start()
    {
        DontDestroyOnLoad(this);
        canvasTracker.Push(titleCanvas);

        soundfxVolumeSlider.value = GameManager.soundFXVolume;
        musicSoundVolumeSlider.value = GameManager.musicVolume;
        masterSoundVolumeSlider.value = GameManager.masterVolume;
        GameManager.musicSources.Add(GetComponent<AudioSource>());
        applyButton.interactable = false;

        ApplySoundChanges();
    }

    // Update is called once per frame
    void Update()
    {
        //Code that runs in the title screen goes here
        if (SceneManager.GetActiveScene().name == "StartScreen")
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canvasTracker.Count > 1)
                BackButton();
        }

        //Code that runs in the gamescene
        //Right now it only pauses the game and unpauses
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            if (Input.GetKeyDown(KeyCode.Escape) && GameManager.currentGameState == GameState.PLAY)
            {
                GameManager.currentGameState = GameState.PAUSED;
                pauseCanvas.gameObject.SetActive(true);
                canvasTracker.Push(pauseCanvas);
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && GameManager.currentGameState == GameState.PAUSED)
            {
                if (canvasTracker.Count > 1)
                    BackButton();
                else
                {
                    ResumeButton();
                    canvasTracker.Pop();
                }
            }
        }
    }
    //Loads to the next scene
    public void StartButton()
    {
        SceneManager.LoadScene(1);
        titleCanvas.gameObject.SetActive(false);

        canvasTracker.Clear();
    }

    //Adds a new canvas to the stack depending on the canvas passed in
    public void goForwardCanvas(Canvas canvas)
    {
        canvasTracker.Peek().gameObject.SetActive(false);
        canvasTracker.Push(canvas);
        if (SceneManager.GetActiveScene().name != "StartScene")
            canvasTracker.Peek().transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        canvasTracker.Peek().gameObject.SetActive(true);
    }

    //Sets the current canvas to false and pop its off
    //After that sets the canvas at the top of the stack to on
    public void BackButton()
    {
        canvasTracker.Peek().gameObject.SetActive(false);
        canvasTracker.Pop();
        canvasTracker.Peek().gameObject.SetActive(true);
    }

    public void VolumeSlider(int index)
    {
        if (!applyButton.interactable)
            applyButton.interactable = true;

        //Used for all volume sliders to calculate what we should change the sound settings to
        if (index == 0)
        {
            GameManager.masterVolume = masterSoundVolumeSlider.value;
            GameManager.musicVolume = musicSoundVolumeSlider.value * GameManager.masterVolume;
            GameManager.soundFXVolume = soundfxVolumeSlider.value * GameManager.masterVolume;
        }
        else if (index == 1)
            GameManager.soundFXVolume = soundfxVolumeSlider.value * GameManager.masterVolume;
        else if (index == 2)
            GameManager.musicVolume = musicSoundVolumeSlider.value * GameManager.masterVolume;

    }

    //Exits application when clicked
    public void ExitButton()
    {
        Application.Quit();
    }

    //Button used to unpause the game
    public void ResumeButton()
    {
        GameManager.currentGameState = GameState.PLAY;
        pauseCanvas.gameObject.SetActive(false);
    }

    //Button used to apply settings so user can test them in the settings screen
    public void ApplyButton()
    {
        writer = new StreamWriter(settingsPath);
        writer.Write(masterSoundVolumeSlider.value.ToString() + "\n");
        writer.Write(soundfxVolumeSlider.value.ToString() + "\n");
        writer.Write(musicSoundVolumeSlider.value.ToString() + "\n");
        writer.Close();

        applyButton.interactable = false;

        ApplySoundChanges();
    }

    //Button used to go back to the main menu
    public void ReturnToMenu()
    {
        //Destroy(GameObject.Find("GameManager"));
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    //Loops through our lists of audiosources and adjusts their volumes
    private void ApplySoundChanges()
    {
        foreach (AudioSource a in GameManager.musicSources)
        {
            a.volume = GameManager.musicVolume;
        }
        foreach (AudioSource a in GameManager.soundFXSources)
        {
            a.volume = GameManager.soundFXVolume;
        }
    }
}
