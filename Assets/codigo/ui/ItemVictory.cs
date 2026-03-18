using UnityEngine;

public class VictoryItem : MonoBehaviour
{
    [Header("Configuración")]
    public static int itemsCollected = 0; // Se guarda globalmente
    public int totalRequired = 3; // Cuántos necesitas para ganar

    private void Start()
    {
        // Reiniciamos el contador cada vez que empieza el nivel
        itemsCollected = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            itemsCollected++; // Sumamos 1 al contador
            Debug.Log("Objetos recogidos: " + itemsCollected);

            // Si ya recogimos los 3...
            if (itemsCollected >= totalRequired)
            {
                // Buscamos el UI y lanzamos la Victoria
                UIStatemanager ui = FindObjectOfType<UIStatemanager>();
                if (ui != null)
                {
                    ui.ChangeState(UIStatemanager.UIState.Victory);
                }
            }

            // Destruimos el objeto recogido
            Destroy(gameObject);
        }
    }
}