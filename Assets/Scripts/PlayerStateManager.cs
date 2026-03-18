using UnityEngine;
using TMPro; // Esencial para manipular los textos de TextMeshPro

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateManager : MonoBehaviour
{
    // --- NUEVO: La Máquina de Estados del Jugador ---
    public enum PlayerState { Idle, Moving, Hurt, Dead }
    public PlayerState currentState = PlayerState.Idle;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI playerStateText; // Aquí arrastraremos tu texto del Canvas

    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public bool canMove = true;

    public Vector2 AnimDirection { get; private set; }

    private Rigidbody2D rb;
    private Vector2 movement;

    private enum AxisPriority { None, Horizontal, Vertical }
    private AxisPriority currentPriority = AxisPriority.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        UpdateStateText(); // Actualizamos el texto desde el inicio
    }

    void Update()
    {
        // Si el jugador está muerto o el juego pausado, no se mueve
        if (!canMove || currentState == PlayerState.Dead)
        {
            movement = Vector2.zero;
            AnimDirection = Vector2.zero;
            UpdateStateText();
            return;
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(inputX, inputY).normalized;

        // Lógica de Prioridad para la animación
        if (inputX == 0 && inputY == 0)
        {
            currentPriority = AxisPriority.None;
            AnimDirection = Vector2.zero;

            // SOLO cambiamos a Idle si no estamos lastimados
            if (currentState != PlayerState.Hurt)
            {
                currentState = PlayerState.Idle;
            }
        }
        else
        {
            // SOLO cambiamos a Moving si no estamos lastimados
            if (currentState != PlayerState.Hurt)
            {
                currentState = PlayerState.Moving;
            }

            if (currentPriority == AxisPriority.None)
            {
                if (inputX != 0) currentPriority = AxisPriority.Horizontal;
                else if (inputY != 0) currentPriority = AxisPriority.Vertical;
            }
            else if (currentPriority == AxisPriority.Horizontal && inputX == 0 && inputY != 0)
            {
                currentPriority = AxisPriority.Vertical;
            }
            else if (currentPriority == AxisPriority.Vertical && inputY == 0 && inputX != 0)
            {
                currentPriority = AxisPriority.Horizontal;
            }

            if (currentPriority == AxisPriority.Horizontal)
            {
                AnimDirection = new Vector2(inputX, 0);
            }
            else if (currentPriority == AxisPriority.Vertical)
            {
                AnimDirection = new Vector2(0, inputY);
            }
        }

        // Reflejamos el estado actual en la UI cada frame
        UpdateStateText();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // --- NUEVO: Función para actualizar el texto ---
    private void UpdateStateText()
    {
        if (playerStateText != null)
        {
            playerStateText.text = "Player State: " + currentState.ToString();
        }
    }

    // --- NUEVO: Función pública para que PlayerHealth pueda matarlo después ---
    public void Die()
    {
        currentState = PlayerState.Dead;
        canMove = false;
        UpdateStateText();
    }

    public void SetHurtState(bool isHurt)
    {
        // Si ya está muerto, ignoramos esto
        if (currentState == PlayerState.Dead) return;

        if (isHurt)
        {
            currentState = PlayerState.Hurt;
        }
        else
        {
            // Al terminar la invencibilidad, volvemos a Idle o Moving según si nos estamos moviendo
            if (movement == Vector2.zero) currentState = PlayerState.Idle;
            else currentState = PlayerState.Moving;
        }

        UpdateStateText();
    }
}