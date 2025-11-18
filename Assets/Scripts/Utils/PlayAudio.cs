using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private float delayBeforePlay = 0f;

    private void Start()
    {
        // Get AudioSource if not assigned in inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Check if AudioSource exists
        if (audioSource == null)
        {
            DebugTag.LogWarning(nameof(PlayAudio), $"No AudioSource found on {gameObject.name}");
            return;
        }

        // Play audio
        if (playOnAwake)
        {
            if (delayBeforePlay > 0)
            {
                Invoke(nameof(PlayingAudio), delayBeforePlay);
            }
            else
            {
                PlayingAudio();
            }
        }
    }

    private void PlayingAudio()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            DebugTag.Log(nameof(PlayAudio), $"Playing audio: {audioSource.clip.name}");
        }
        else
        {
            DebugTag.LogWarning(nameof(PlayAudio), "AudioSource or AudioClip is missing!");
        }
    }

    // Optional: Call this method if you want to play audio manually
    public void PlaySound()
    {
        PlayingAudio();
    }
}