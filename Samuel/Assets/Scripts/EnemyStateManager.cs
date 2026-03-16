using UnityEngine;
using TMPro; // Necesario para la UI

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStateManager : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI enemyStateText; // Aquí arrastraremos el texto del Canvas

    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;

    [Header("Configuración de Patrullaje")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float patrolSpeed = 2f;

    [Header("Configuración de Persecución y Ataque")]
    public float chaseSpeed = 3.5f;
    public float detectionRadius = 4f;
    public float attackRadius = 1f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    public event System.Action OnAttackEvent;
    public Vector2 MovementDirection { get; private set; }
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        UpdateStateText(); // Inicializamos el texto desde que arranca el juego
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Guardamos el estado en el que estábamos antes de procesar la lógica
        EnemyState previousState = currentState;

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                ChaseBehavior(distanceToPlayer);
                break;

            case EnemyState.Attack:
                AttackBehavior(distanceToPlayer);
                break;
        }

        // Si el estado cambió en este frame, actualizamos el texto de la UI
        if (currentState != previousState)
        {
            UpdateStateText();
        }
    }

    void FixedUpdate()
    {
        if (currentState != EnemyState.Attack)
        {
            rb.MovePosition(rb.position + MovementDirection * currentSpeed * Time.fixedDeltaTime);
        }
    }

    private void PatrolBehavior()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        MovementDirection = (targetWaypoint.position - transform.position).normalized;
        currentSpeed = patrolSpeed;

        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void ChaseBehavior(float distanceToPlayer)
    {
        MovementDirection = (player.position - transform.position).normalized;
        currentSpeed = chaseSpeed;

        if (distanceToPlayer <= attackRadius)
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer > detectionRadius)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void AttackBehavior(float distanceToPlayer)
    {
        MovementDirection = Vector2.zero;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("¡El enemigo ha iniciado la animación de ataque!");
            OnAttackEvent?.Invoke(); // Solo llamamos a la animación

            lastAttackTime = Time.time;
        }

        if (distanceToPlayer > attackRadius)
        {
            currentState = EnemyState.Chase;
        }
    }

    // --- NUEVO: Función para actualizar el texto ---
    private void UpdateStateText()
    {
        if (enemyStateText != null)
        {
            enemyStateText.text = "Enemy State: " + currentState.ToString();
        }
    }
    public void DealDamage()
    {
        // Solo hacemos daño si el jugador sigue vivo y dentro del rango
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Le damos un pequeñísimo margen extra de gracia (+0.5f) por si el jugador apenas iba saliendo
            if (distanceToPlayer <= attackRadius + 0.5f)
            {
                player.GetComponent<PlayerHealth>()?.TakeDamage(1);
                Debug.Log("¡Golpe conectado! HP reducido.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}