using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; //  ¡NUEVO! Necesario para las pausas de tiempo

public class UIStatemanager : MonoBehaviour
{
    public enum UIState { MainMenu, Option, Pause, HUD, GameOver, Victory }

    [Header("Paneles")]
    [SerializeField] private GameObject Panel_MainMenu;
    [SerializeField] private GameObject Panel_Options;
    [SerializeField] private GameObject Panel_Pause;
    [SerializeField] private GameObject Panel_HUD;
    [SerializeField] private GameObject Panel_GameOver;
    [SerializeField] private GameObject Panel_Victory;

    [Header("Textos del Temporizador")]
    [SerializeField] private TextMeshProUGUI txtHUDTime;
    [SerializeField] private TextMeshProUGUI txtGameOverTime;
    [SerializeField] private TextMeshProUGUI txtVictoryTime;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI txtStateDebug;

    private UIState currentState;

    private float elapsedTime = 0f;
    private bool isTimerRunning = false;

    private void Start()
    {
        ChangeState(UIState.MainMenu);
        elapsedTime = 0f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == UIState.HUD) ChangeState(UIState.Pause);
            else if (currentState == UIState.Pause) ChangeState(UIState.HUD);
        }

        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateHUDTimer();
        }
    }

    public void ChangeState(UIState nextState)
    {
        currentState = nextState;

        if (Panel_MainMenu != null) Panel_MainMenu.SetActive(false);
        if (Panel_Options != null) Panel_Options.SetActive(false);
        if (Panel_Pause != null) Panel_Pause.SetActive(false);
        if (Panel_HUD != null) Panel_HUD.SetActive(false);
        if (Panel_GameOver != null) Panel_GameOver.SetActive(false);
        if (Panel_Victory != null) Panel_Victory.SetActive(false);

        switch (currentState)
        {
            case UIState.MainMenu:
                if (Panel_MainMenu != null) Panel_MainMenu.SetActive(true);
                isTimerRunning = false;
                PauseGame();
                break;
            case UIState.Option:
                if (Panel_Options != null) Panel_Options.SetActive(true);
                isTimerRunning = false;
                PauseGame();
                break;
            case UIState.Pause:
                if (Panel_Pause != null) Panel_Pause.SetActive(true);
                isTimerRunning = false;
                PauseGame();
                break;
            case UIState.HUD:
                if (Panel_HUD != null) Panel_HUD.SetActive(true);
                isTimerRunning = true;
                ResumeGame();
                break;
            case UIState.GameOver:
                if (Panel_GameOver != null) Panel_GameOver.SetActive(true);
                isTimerRunning = false;
                SetFinalTimeText(txtGameOverTime, "Sobreviviste: ");
                break;
            case UIState.Victory:
                if (Panel_Victory != null) Panel_Victory.SetActive(true);
                isTimerRunning = false;
                SetFinalTimeText(txtVictoryTime, "Tiempo total: ");
                PauseGame();
                break;
        }

        if (txtStateDebug != null) txtStateDebug.text = "Current State: " + currentState.ToString();
    }

    private void PauseGame() { Time.timeScale = 0f; }
    private void ResumeGame() { Time.timeScale = 1f; }

    // ---  NUEVO: Función para retrasar el Game Over ---
    public void ShowGameOverWithDelay(float delayInSeconds)
    {
        // Apagamos el reloj desde el momento en que muere
        isTimerRunning = false;
        StartCoroutine(WaitAndShowGameOver(delayInSeconds));
    }

    private IEnumerator WaitAndShowGameOver(float delay)
    {
        // Espera los segundos que le digamos (ej. 2.5 segundos)
        yield return new WaitForSeconds(delay);
        // Despues de esperar, activa el Game Over
        ChangeState(UIState.GameOver);
    }

    // --- FUNCIONES DEL TEMPORIZADOR ---
    private void UpdateHUDTimer()
    {
        if (txtHUDTime != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            txtHUDTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void SetFinalTimeText(TextMeshProUGUI textElement, string prefix)
    {
        if (textElement != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            textElement.text = prefix + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // --- Funciones para botones ---
    public void OnclickButtonStart() { ChangeState(UIState.HUD); }
    public void OnclickButtonOptions() { ChangeState(UIState.Option); }
    public void OnclickButtonBackToMainMenu() { ChangeState(UIState.MainMenu); }
    public void OnclickButtonResume() { ChangeState(UIState.HUD); }
    public void OnclickButtonPause() { ChangeState(UIState.Pause); }
    public void OnclickButtonRestart()
    {
        ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OnclickButtonExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}