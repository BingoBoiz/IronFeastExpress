using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
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
        /*DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;*/
        //CuttingCabinet.OnAnyCut += CuttingCounter_OnAnyCut;
        Player.OnAnyPlayerPickedSomething += Player_OnPickedSomething;
        //BaseCabinet.OnDropSomething += BaseCounter_OnDropSomething;
        TrashHole.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashHole trashCounter = sender as TrashHole;
        PlaySound(tutorialSFX.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnDropSomething(object sender, System.EventArgs e)
    {
        BaseCabinet baseCounter = sender as BaseCabinet;
        PlaySound(tutorialSFX.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, System.EventArgs e)
    {
        Player player = sender as Player;
        PlaySound(tutorialSFX.objectPickUp, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCabinet cuttingCounter = sender as CuttingCabinet;
        PlaySound(tutorialSFX.chop, cuttingCounter.transform.position);
    }

    // THIS SHOULD BE FIX FOR CUSTOMER TABLE
    /*private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(tutorialSFX.deliveryFail, deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(tutorialSFX.deliverySuccess, deliveryCounter.transform.position);
    }*/

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volumeMultiplier);
    }
    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
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

    public void ChangeVolumn()
    {
        volume += .1f;
        if (volume > 1f)
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
