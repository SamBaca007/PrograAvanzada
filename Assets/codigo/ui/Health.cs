using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("Cantidad a curar")]
    public float healAmount = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Esto aparecerá en la consola de Unity (abajo) cada vez que algo toque la poción
        Debug.Log("¡Algo tocó la poción! Fue: " + collision.gameObject.name + " con el Tag: " + collision.tag);

        if (collision.CompareTag("Player"))
        {
            PlayerStateManager player = collision.GetComponent<PlayerStateManager>();
            if (player != null)
            {
                player.Heal(healAmount);
                Debug.Log("¡Jugador curado! Destruyendo poción...");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("El objeto tiene el tag Player, pero no tiene el script PlayerStateManager.");
            }
        }
    }
}