using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Límites de la Cámara")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (player != null)
        {
            // Calculamos dónde "quiere" ir la cámara
            float targetX = player.position.x + offset.x;
            float targetY = player.position.y + offset.y;

            // Restringimos esos valores para que no pasen de los límites
            float clampedX = Mathf.Clamp(targetX, minX, maxX);
            float clampedY = Mathf.Clamp(targetY, minY, maxY);

            // Aplicamos la posición restringida
            transform.position = new Vector3(clampedX, clampedY, offset.z);
        }
    }
}