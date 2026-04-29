using UnityEngine;

public class UISound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip sound;

    public void PlaySound()
    {
        audioSource.PlayOneShot(sound);
    }
}
