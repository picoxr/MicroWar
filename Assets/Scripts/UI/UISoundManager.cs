using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager instance; // Singleton instance

    public AudioClip clickSound; // Sound for button clicks
    public AudioClip hoverSound; // Sound for button hover events

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void PlayHoverSound()
    {
        audioSource.PlayOneShot(hoverSound);
    }
}
