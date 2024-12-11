using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCabinetSound : MonoBehaviour
{
    [SerializeField] private Stove stoveCabinet;
    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        stoveCabinet.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCabinet.OnProgressChanged += StoveCounter_OnProgressChanged;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        playWarningSound = stoveCabinet.IsBurned() && e.progressNormalized >= burnShowProgressAmount;
    }

    private void StoveCounter_OnStateChanged(object sender, Stove.OnStateChangedEventArgs e)
    {
        bool playSound = e.state == Stove.State.Frying || e.state == Stove.State.Burned;
        if (playSound) 
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer < 0)
            {
                float warningSoundTimerMax = .2f;
                warningSoundTimer = warningSoundTimerMax;
                SoundManager.Instance.PlayBurnWarningSound(stoveCabinet.transform.position);
            }
        }
        
    }
}
