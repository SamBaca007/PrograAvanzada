using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStateManager : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation & UI")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshProUGUI playerStateText;

    [Header("Damage Feedback")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Vida del Jugador")]
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;
    private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityTime = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private string lastState = "";

    // Hashes para Animator
    private static readonly int HashIsMoving = Animator.StringToHash("isMoving");
    private static readonly int HashTakeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int HashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // ❌ sin gravedad, ya no hay salto
        rb.freezeRotation = true;

        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        //  Movimiento en 8 direcciones
        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;

        moveInput = new Vector2(x, y).normalized;

        UpdateAnimator(moveInput);
        UpdatePlayerStateMessage(moveInput);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdatePlayerStateMessage(Vector2 input)
    {
        if (playerStateText == null) return;

        string currentState = "";

        if (input.magnitude > 0.01f)
        {
            currentState = "Move";
            playerStateText.color = Color.blue;
        }
        else
        {
            currentState = "Idle";
            playerStateText.color = Color.white;
        }

        if (currentState != lastState)
        {
            playerStateText.text = "Player State: " + currentState;
            lastState = currentState;
        }
    }

    private void UpdateAnimator(Vector2 input)
    {
        if (animator == null) return;

        bool isMoving = input.magnitude > 0.01f;
        animator.SetBool(HashIsMoving, isMoving);

        // 
        if (spriteRenderer != null && isMoving)
        {
            spriteRenderer.flipX = input.x < 0f;
        }
    }

    public void TakeDamage(float amount = 1f)
    {
        if (currentHealth <= 0) return;
        if (isInvulnerable) return;

        currentHealth -= amount;
        animator.SetTrigger(HashTakeDamage);
        StartCoroutine(FlashDamage());
        StartCoroutine(Invulnerability());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = normalColor;
        }
    }

    private IEnumerator Invulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private void Die()
    {
        animator.SetTrigger(HashDie);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        StartCoroutine(ShowGameOverAfterDeath());
    }

    private IEnumerator ShowGameOverAfterDeath()
    {
        // 1. Buscamos el UI Manager
        UIStatemanager ui = FindObjectOfType<UIStatemanager>();

        if (ui != null)
        {
            // 2. Le decimos al UI Manager que lance el Game Over PERO que se espere 2.5 segundos
            ui.ShowGameOverWithDelay(2.5f);
        }

        yield return null; // Terminamos esta corrutina del jugador
    }


    public float CurrentHealth => currentHealth;
   
    public void Heal(float amount)
    {
        if (currentHealth <= 0) return; // Si ya murió, no se puede curar

        currentHealth += amount;

        // Evitamos que se cure más allá de la vida máxima
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Jugador curado. Vida actual: " + currentHealth);
    }
}
