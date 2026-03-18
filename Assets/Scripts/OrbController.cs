using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OrbController : MonoBehaviour
{
    [Header("Configuración del Orbe")]
    public int pointsValue = 100;
    public Transform[] spawnPoints; // Lista de lugares seguros

    private UIStateManager uiManager;

    void Start()
    {
        // Buscamos el UIManager automáticamente al iniciar
        uiManager = FindObjectOfType<UIStateManager>();

        // Nos movemos a un punto al azar desde el principio
        MoveToRandomSpawnPoint();
    }

    // Esta función se activa cuando alguien atraviesa el orbe
    void OnTriggerEnter2D(Collider2D other)
    {
        // Revisamos si quien lo tocó fue el Jugador
        if (other.CompareTag("Player"))
        {
            if (uiManager != null)
            {
                uiManager.AddScore(pointsValue);
            }

            MoveToRandomSpawnPoint();
        }
    }

    private void MoveToRandomSpawnPoint()
    {
        if (spawnPoints.Length > 0)
        {
            // Elegimos un número al azar entre 0 y la cantidad de puntos que hayas puesto
            int randomIndex = Random.Range(0, spawnPoints.Length);

            // Movemos el orbe a esa posición
            transform.position = spawnPoints[randomIndex].position;
        }
    }
}