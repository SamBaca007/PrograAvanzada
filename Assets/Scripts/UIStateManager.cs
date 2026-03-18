using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Asegúrate de usar TextMeshPro para el texto

public class UIStateManager : MonoBehaviour
{
    // --- CAMBIO 1: Añadimos "Victory" a los posibles estados ---
    public enum GameState { MainMenu, Options, Gameplay, Pause, GameOver, Victory }
    public GameState currentState;

    [Header("UI")]
    // Nota: Como usas dos escenas, deja vacíos los paneles que no existan en la escena actual.
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject gameplayHUD;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    // --- CAMBIO 2: Añadimos el espacio para el panel de victoria ---
    public GameObject victoryPanel;

    [Header("Estado y Textos")]
    public TextMeshProUGUI stateText;

    // --- CAMBIO 3: Añadimos los espacios para los textos de puntuación ---
    public TextMeshProUGUI scoreText; // El del HUD principal
    public TextMeshProUGUI finalScoreText; // El que aparece en la pantalla de Game Over
    public TextMeshProUGUI victoryScoreText; // El que aparece en la pantalla de Victoria

    // --- CAMBIO 4: Las variables matemáticas para controlar la puntuación ---
    [Header("Configuración de Puntuación")]
    public int currentScore = 0;
    public int scoreToWin = 5000;

    [Header("Escenas")]
    public string menuSceneName = "MainMenu"; // Cambia esto por el nombre real de tu escena
    public string gameplaySceneName = "Gameplay"; // Cambia esto por el nombre real de tu escena

    void Start()
    {
        // Esto asegura que el tiempo corra normal al iniciar cualquier escena
        Time.timeScale = 1f;

        // Detectamos en qué escena estamos para asignar el estado correcto automáticamente
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == gameplaySceneName)
        {
            currentState = GameState.Gameplay;
        }
        else if (currentScene == menuSceneName)
        {
            currentState = GameState.MainMenu;
        }

        // Ahora sí, actualizamos el texto con el estado correcto
        UpdateStateText();

        // --- CAMBIO 5: Inicializamos el texto del Score para que empiece en 0 ---
        UpdateScoreUI();
    }

    // --- MÉTODOS PARA EL MENÚ PRINCIPAL ---

    public void PlayGame()
    {
        // Al cargar la escena se hace un "hard reset" de todo (HP, posiciones, etc.)
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenOptions()
    {
        ChangeState(GameState.Options);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        ChangeState(GameState.MainMenu);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit(); // Cierra el juego en la build final

        // Detiene el "Play Mode" dentro del editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- MÉTODOS PARA EL GAMEPLAY ---

    public void PauseGame()
    {
        ChangeState(GameState.Pause);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Congela el juego
    }

    public void ResumeGame()
    {
        ChangeState(GameState.Gameplay);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Descongela el juego
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Fundamental para que el menú no esté congelado
        SceneManager.LoadScene(menuSceneName);
    }

    // --- UTILIDADES ---

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        UpdateStateText();
    }

    private void UpdateStateText()
    {
        if (stateText != null)
        {
            stateText.text = "State: " + currentState.ToString();
        }
    }

    // --- NUEVAS FUNCIONES DE PUNTUACIÓN ---

    // --- CAMBIO 6: La función que llamará el orbe para darnos puntos ---
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();

        if (currentScore >= scoreToWin)
        {
            TriggerVictory();
        }
    }

    // --- CAMBIO 7: La función que actualiza el texto del HUD ---
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    // --- FUNCIONES DE FIN DE JUEGO ---

    public void TriggerGameOver()
    {
        Debug.Log("PASO 3: UIManager recibió la señal. Cambiando estado a GameOver.");
        ChangeState(GameState.GameOver);

        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("PASO 4: Panel de Game Over activado con éxito en la jerarquía.");
        }
        else
        {
            Debug.LogError("¡ERROR!: El GameOver Panel no está referenciado en el UIManager.");
        }

        // --- CAMBIO 8: Mostrar la puntuación final al morir ---
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + currentScore;
        }

        Time.timeScale = 0f;
    }

    // --- CAMBIO 9: La función que detiene todo y muestra que ganaste ---
    public void TriggerVictory()
    {
        Debug.Log("¡Meta alcanzada! Cambiando estado a Victory.");
        ChangeState(GameState.Victory);

        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // --- NUEVO: Mostrar la puntuación final al ganar ---
        if (victoryScoreText != null)
        {
            victoryScoreText.text = "Final Score: " + currentScore;
        }

        Time.timeScale = 0f;
    }
}