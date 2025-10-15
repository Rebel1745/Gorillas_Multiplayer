using System;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartButton);
        _settingsButton.onClick.AddListener(OnSettingsButton);
        _quitButton.onClick.AddListener(OnQuitButton);
    }

    private void Start()
    {
        AudioManager.Instance.PlayBackgroundMusic(AudioClipType.IntroMusic);
    }

    private void OnStartButton()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.MultiplayerUI, true);
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.StatusScreenUI, true);
        Hide();
    }

    private void OnSettingsButton()
    {
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.MultiplayerUI, false);
        UIManager.Instance.ShowHideUIElement(UIManager.Instance.StatusScreenUI, false);
        SettingsManager.Instance.OnSettingsButton(true);
        Hide();
    }

    private void OnQuitButton()
    {
        Application.Quit();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
