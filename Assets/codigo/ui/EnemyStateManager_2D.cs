using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStateManager_2D : MonoBehaviour
{
    // Estados posibles del enemigo
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("Referencias")]
    [SerializeField] private Transform Player;              // Transform del jugador
    [SerializeField] private PlayerStateManager playerScript; // Script del jugador para aplicar daño
    [SerializeField] private Transform[] waypoint;          // Puntos de patrulla
    [SerializeField] private TextMeshProUGUI stateText;     // Texto de estado del enemigo
    [SerializeField] private TextMeshProUGUI playerTimerText; // Texto que muestra la vida del jugador
    [SerializeField] private Animator animator;             // Animator del enemigo

    [Header("Ajustes de Combate")]
    [SerializeField] private float detectionRange = 5f; // Distancia para empezar a perseguir
    [SerializeField] private float attackRange = 1.2f;  // Distancia para activar animación de ataque
    [SerializeField] private float damageRange = 0.9f;  // Distancia real para aplicar daño
    [SerializeField] private float damageAmount = 1f;   // Cantidad de daño por golpe

    [Header("Movimiento")]
    [SerializeField] private float patrolspeed = 2f; // Velocidad al patrullar
    [SerializeField] private float chasespeed = 4f;  // Velocidad al perseguir
    [Header("Cooldown de ataque")]
    [SerializeField] private float attackCooldown = 1.5f; // tiempo de espera entre ataques
    private float lastAttackTime = -Mathf.Infinity;

    private EnemyState currentState;
    private int currentWaypointIndex = 0;
    private bool touchingPlayer = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // enemigo no cae
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        ChangeState(EnemyState.Patrol);
    }

    void Update()
    {
        if (Player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

        // --- Lógica de decisión ---
        if (distanceToPlayer <= attackRange)
            ChangeState(EnemyState.Attack);
        else if (distanceToPlayer <= detectionRange)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Patrol);

        // --- Lógica de acción ---
        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase: MoveTowards(Player.position, chasespeed); break;
                // En Attack no se mueve, se queda atacando
        }

        // --- Lógica de daño ---
        if (touchingPlayer || distanceToPlayer <= damageRange)
        {
            AplicarDanio();
        }

        // Actualizar contador de vida del jugador
        UpdateTimerUI();
    }

    // Cambia estado y actualiza UI/animaciones
    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        if (stateText != null)
        {
            stateText.text = "Enemy State: " + currentState.ToString();
            if (currentState == EnemyState.Patrol) stateText.color = Color.white;
            if (currentState == EnemyState.Chase) stateText.color = Color.yellow;
            if (currentState == EnemyState.Attack) stateText.color = Color.red;
        }

        if (animator != null)
        {
            animator.SetBool("isAttacking", currentState == EnemyState.Attack);
            animator.SetBool("isChasing", currentState == EnemyState.Chase);
        }
    }

  
    // Aplica daño al jugador 
    private void AplicarDanio()
    {
        // Verificamos si ya pasó el tiempo de cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageAmount); // el Player maneja animación y muerte
                lastAttackTime = Time.time; // actualizamos el último ataque
                Debug.Log("Enemigo aplicó daño al jugador");
            }
        }
    }


    // Movimiento hacia un objetivo
    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (direction.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    // Patrulla entre waypoints
    private void Patrol()
    {
        if (waypoint.Length == 0) return;

        MoveTowards(waypoint[currentWaypointIndex].position, patrolspeed);

        if (Vector2.Distance(transform.position, waypoint[currentWaypointIndex].position) < 0.2f)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoint.Length;
    }

    // Actualiza texto de vida del jugador
    private void UpdateTimerUI()
    {
        if (playerTimerText != null && playerScript != null)
        {
            playerTimerText.text = $"Life: {Mathf.CeilToInt(playerScript.CurrentHealth)}";
        }
    }

    // --- Colisiones físicas ---
    private void OnCollisionEnter2D(Collision2D col) { if (col.gameObject.CompareTag("Player")) touchingPlayer = true; }
    private void OnCollisionExit2D(Collision2D col) { if (col.gameObject.CompareTag("Player")) touchingPlayer = false; }
    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) touchingPlayer = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) touchingPlayer = false; }
}