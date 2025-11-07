using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Audio;         
using UnityEngine.InputSystem;   
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panelleri")]
    public GameObject hudPanel;         
    public GameObject pauseMenuPanel;   
    public GameObject gameOverPanel;    
    public GameObject optionsPanel;    

    [Header("Ses Ayarlarý")]
    public AudioMixer mainMixer; 
    public string masterVolumeParameter = "MasterVolume";

    public enum GameState { Playing, Paused, GameOver }
    public GameState currentState;

    private PlayerInputActions playerInputActions;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Time.timeScale = 1f;
        AudioListener.pause = false; 

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Pause.performed += ctx => TogglePauseMenu();
    }

    void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Player.Pause.performed -= ctx => TogglePauseMenu();
            playerInputActions.Player.Disable();
        }
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        currentState = GameState.Playing;
        hudPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    public void TogglePauseMenu()
    {
        if (currentState == GameState.GameOver) return;

        if (currentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (currentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f; 

        hudPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f; 

        hudPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowGameOverScreen()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        hudPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        gameOverPanel.SetActive(true);


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {


        Time.timeScale = 1f;


        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowOptionsScreen()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void ShowPauseScreen()
    {
        optionsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýldý.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Eðer Editör'de deðilsek (yani build alýnmýþ oyundaysak), normal þekilde kapat.
        Application.Quit();
#endif
    }

    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat(masterVolumeParameter, Mathf.Log10(volume) * 20);
    }
}