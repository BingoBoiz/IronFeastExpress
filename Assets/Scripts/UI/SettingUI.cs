using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public static SettingUI Instance { get; private set; }

    [SerializeField] private Button backgroundMusicButtonIncrease;
    [SerializeField] private Button backgroundMusicButtonDecrease;
    [SerializeField] private Button soundEffectsButtonIncrease;
    [SerializeField] private Button soundEffectsButtonDecrease;
    [SerializeField] private TextMeshProUGUI soundEffectsAmountText;
    [SerializeField] private TextMeshProUGUI backgroundMusicAmountText;
    [SerializeField] private Button closeButton;

    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI interactAltText;

    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button interactAltButton;

    [SerializeField] private Transform pressToRebindKeyTransform;

    private void Awake()
    {
        Instance = this;
        soundEffectsButtonIncrease.onClick.AddListener(() =>
        {
            SoundManager.Instance.IncreaseVolumn();
            UpdateVisual();
        });
        soundEffectsButtonDecrease.onClick.AddListener(() =>
        {
            SoundManager.Instance.DecreaseVolumn();
            UpdateVisual();
        });

        backgroundMusicButtonIncrease.onClick.AddListener(() =>
        {
            MusicManager.Instance.IncreaseVolumn();
            UpdateVisual();
        });
        backgroundMusicButtonDecrease.onClick.AddListener(() =>
        {
            MusicManager.Instance.DecreaseVolumn();
            UpdateVisual();
        });

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });

        moveUpButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Up);
        });

        moveDownButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Down);
        });

        moveLeftButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Left);
        });

        moveRightButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_Right);
        });

        interactButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Interact);
        });

        interactAltButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.InteractAlt);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnToggleLocalGamePause += GameManager_OnTogglePauseGame;
        UpdateVisual();
        Hide();
        HidePressToRebindKey();
    }

    private void GameManager_OnTogglePauseGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        soundEffectsAmountText.text = "" + Mathf.Round(SoundManager.Instance.GetVolume() * 10f);
        backgroundMusicAmountText.text = "" + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);

        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAltText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlt);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide() 
    {
        gameObject.SetActive(false);
    }
    private void ShowPressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }
    private void HidePressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }
    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () => 
        {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnToggleLocalGamePause -= GameManager_OnTogglePauseGame;
        }
    }

}
