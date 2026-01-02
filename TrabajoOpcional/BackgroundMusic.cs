using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource musicSource;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!musicSource)
            musicSource = GetComponent<AudioSource>();

        if (musicSource && !musicSource.isPlaying)
            musicSource.Play();
    }
}
