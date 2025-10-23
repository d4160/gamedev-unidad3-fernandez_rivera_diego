using UnityEngine;

/// <summary>
/// Componente de la puerta, especifica para control de audio, siguiendo el principio de Single Responsibility
/// </summary>
public class DoorAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource _doorAudioSource;
    public AudioClip _openSFX;
    public AudioClip _closeSFX;

    public void PlayOpenSFX()
    {
        _doorAudioSource.Stop();
        _doorAudioSource.minDistance = 1;
        _doorAudioSource.maxDistance = 15;
        _doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
        _doorAudioSource.spatialBlend = 1;
        _doorAudioSource.volume = 0.8f;
        _doorAudioSource.clip = _openSFX;
        _doorAudioSource.Play();
    }

    public void PlayCloseSFX()
    {
        _doorAudioSource.Stop();
        _doorAudioSource.minDistance = 1;
        _doorAudioSource.maxDistance = 18;
        _doorAudioSource.pitch = Random.Range(0.95f, 1.05f);
        _doorAudioSource.spatialBlend = 1;
        _doorAudioSource.volume = 0.9f;
        _doorAudioSource.clip = _closeSFX;
        _doorAudioSource.Play();
    }
}
