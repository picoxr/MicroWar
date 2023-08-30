using UnityEngine;

namespace MicroWar
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager instance;
        public static SoundManager Instance { get => instance; private set => instance = value; }

        public AudioSource audioSourceSFX;
        public AudioSource audioSourcePowerUp;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning("There is already an instance of SFXManager exists in the scene.");
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        public void PlayGenericSFX(AudioClip audioClip)
        {
            audioSourceSFX.PlayOneShot(audioClip);
        }

        public void PlayPowerUpSFX(AudioClip audioClip)
        {
            audioSourcePowerUp.PlayOneShot(audioClip);
        }
    }

}
