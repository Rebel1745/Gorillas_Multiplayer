using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private int _playerId;
    [SerializeField] private TMP_Text _powerText;
    [SerializeField] private Slider _powerSlider;
    [SerializeField] private TMP_Text _angleText;
    [SerializeField] private Slider _angleSlider;
    [SerializeField] private Button _launchButton;
    [SerializeField] private TMP_InputField _powerInput;
    [SerializeField] private TMP_InputField _angleInput;

    private void Start()
    {
        GameManager.Instance.OnNewGame += GameManager_OnOnNewGame;
    }

    private void GameManager_OnOnNewGame(object sender, EventArgs e)
    {
        Show();
        SetViewMode();
    }

    private void SetViewMode()
    {
        if (_playerId == (int)NetworkManager.Singleton.LocalClientId)
        {
            SetViewModeSliders();
        }
        else
        {
            SetViewModeReadOnly();
        }
    }

    private void SetViewModeSliders()
    {
        _powerText.gameObject.SetActive(true);
        _powerSlider.gameObject.SetActive(true);
        _angleText.gameObject.SetActive(true);
        _angleSlider.gameObject.SetActive(true);
        _powerInput.gameObject.SetActive(false);
        _angleInput.gameObject.SetActive(false);
        _launchButton.gameObject.SetActive(true);
    }

    private void SetViewModeReadOnly()
    {
        _powerText.gameObject.SetActive(true);
        _powerSlider.gameObject.SetActive(false);
        _angleText.gameObject.SetActive(true);
        _angleSlider.gameObject.SetActive(false);
        _powerInput.gameObject.SetActive(false);
        _angleInput.gameObject.SetActive(false);
        _launchButton.gameObject.SetActive(false);
    }

    private void SetViewModeInputBoxes()
    {
        _powerText.gameObject.SetActive(false);
        _powerSlider.gameObject.SetActive(false);
        _angleText.gameObject.SetActive(false);
        _angleSlider.gameObject.SetActive(false);
        _powerInput.gameObject.SetActive(true);
        _angleInput.gameObject.SetActive(true);
        _launchButton.gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
