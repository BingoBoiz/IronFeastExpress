using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";
    public static SoundManager Instance {  get; private set; }
    [SerializeField] AudioClipRefsSO tutorialSFX;
    private float volume = 1f;

    private void Awake()
    {
        Instance = this;
        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start()
    {
        Player.OnAnyPlayerPickedSomething += Player_OnPickedSomething;
        BaseCabinet.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashHole.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashHole trashCounter = sender as TrashHole;
        PlaySound(tutorialSFX.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, System.EventArgs e)
    {
        BaseCabinet baseCounter = sender as BaseCabinet;
        PlaySound(tutorialSFX.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, System.EventArgs e)
    {
        Player player = sender as Player;
        PlaySound(tutorialSFX.objectPickUp, player.transform.position);
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volumeMultiplier);
    }
    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }

    public void CuttingCabinetOnCut(Transform cabinet)
    {
        PlaySound(tutorialSFX.chop, cabinet.position);
    }

    public void CustomerTableDeliverCorrectDish(Transform cabinet)
    {
        PlaySound(tutorialSFX.deliverySuccess, cabinet.position);
    }

    public void CustomerTableDeliverWrongDish(Transform cabinet)
    {
        PlaySound(tutorialSFX.deliveryFail, cabinet.position);
    }

    public void PlayFootstepsSound(Vector3 position, float volume)
    {
        PlaySound(tutorialSFX.footstep, position, volume);
    }

    public void PlayCountdownSound()
    {
        PlaySound(tutorialSFX.warning, Vector3.zero);
    }

    public void PlayBurnWarningSound(Vector3 position)
    {
        PlaySound(tutorialSFX.warning, position);
    }

    public void IncreaseVolumn()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 1f;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void DecreaseVolumn()
    {
        volume -= .1f;
        if (volume < 0f)
        {
            volume = 0f;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
