using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Esta variable estática guardará la "instancia" para que no haya dos músicas sonando
    private static MusicManager instance;

    void Awake()
    {
        // Lógica de Singleton: Si ya existe una música, destruye la nueva para que no se dupliquen
        if (instance == null)
        {
            instance = this;
            // ¡ESTO ES LO IMPORTANTE! Hace que el objeto sobreviva entre escenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        // Configuramos que siempre se repita
        if (audioSource != null)
        {
            audioSource.loop = true;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}