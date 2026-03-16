using UnityEngine;
using System.Collections.Generic;

public class RandomDoorManager : MonoBehaviour
{
    [Header("Tu Personaje Principal")]
    public GameObject Player;

    [Header("Puntos de Salida")]
    public Transform[] exitPoints;

    // --- NUEVO: Sistema de recarga para evitar bucles ---
    private bool puedeTeletransportarse = true;
    public float tiempoDeEspera = 0.5f; // Medio segundo de invulnerabilidad a las puertas

    void Start()
    {
        Collider2D[] childDoors = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D door in childDoors)
        {
            if (door.gameObject != this.gameObject)
            {
                DoorTriggerHelper helper = door.gameObject.AddComponent<DoorTriggerHelper>();
                helper.manager = this;
            }
        }
    }

    public void TeleportPlayer(Transform playerTransform, Transform puertaTocada)
    {
        // Si no ha pasado el medio segundo, ignoramos el choque
        if (!puedeTeletransportarse) return;

        if (exitPoints != null && exitPoints.Length > 1)
        {
            List<Transform> salidasValidas = new List<Transform>();

            // Filtramos la puerta por la que acabas de entrar
            foreach (Transform salida in exitPoints)
            {
                if (Vector2.Distance(puertaTocada.position, salida.position) > 2f)
                {
                    salidasValidas.Add(salida);
                }
            }

            if (salidasValidas.Count == 0)
            {
                salidasValidas.AddRange(exitPoints);
            }

            int randomIndex = Random.Range(0, salidasValidas.Count);

            // 1. Movemos al jugador
            playerTransform.position = salidasValidas[randomIndex].position;

            // 2. Apagamos el teletransporte temporalmente
            puedeTeletransportarse = false;

            // 3. Le decimos a Unity que lo vuelva a encender después del tiempo de espera
            Invoke("ReactivarTeletransporte", tiempoDeEspera);
        }
    }

    // Esta función la llama Unity automáticamente después de medio segundo
    private void ReactivarTeletransporte()
    {
        puedeTeletransportarse = true;
    }
}

// --- SCRIPT VIGILANTE ---
public class DoorTriggerHelper : MonoBehaviour
{
    [HideInInspector] public RandomDoorManager manager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (manager != null && manager.Player != null)
        {
            if (collision.gameObject == manager.Player)
            {
                manager.TeleportPlayer(collision.transform, this.transform);
            }
        }
    }
}