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
    private float _powerValue;
    private float _angleValue;

    public override void OnNetworkSpawn()
    {
        SetupListeners();
    }

    private void GameManager_OnOnNewGame(object sender, EventArgs e)
    {
        Hide();
        SetViewMode();
        _powerValue = 50f;
        _angleValue = 45f;
        UpdatePowerDetails();
        UpdateAngleDetails();
    }

    private void GameManager_OnPlayerIdChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.CurrentPlayerId.Value == _playerId)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void GameManager_OnGameOver(object sender, EventArgs e)
    {
        Hide();
    }

    private void SetupListeners()
    {
        GameManager.Instance.OnNewGame += GameManager_OnOnNewGame;
        GameManager.Instance.OnCurrentPlayerIdChanged += GameManager_OnPlayerIdChanged;
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;

        _powerSlider.onValueChanged.AddListener(OnPowerSliderValueChanged);
        _angleSlider.onValueChanged.AddListener(OnAngleSliderValueChanged);

        _powerInput.onValueChanged.AddListener(OnPowerInputChanged);
        _angleInput.onValueChanged.AddListener(OnAngleInputChanged);

        _launchButton.onClick.AddListener(OnLaunchButtonClicked);
    }

    #region Power and angle controls
    private void OnPowerSliderValueChanged(float power)
    {
        _powerValue = power;
        UpdatePowerDetails();
    }

    private void UpdatePowerDetails()
    {
        _powerSlider.value = _powerValue;
        _powerInput.text = _powerValue.ToString();
        OnPowerValueChangedRpc(_powerValue);
    }

    private void OnAngleSliderValueChanged(float angle)
    {
        _angleValue = angle;
        UpdateAngleDetails();
    }

    private void UpdateAngleDetails()
    {
        _angleSlider.value = _angleValue;
        _angleInput.text = _angleValue.ToString();
        OnAngleValueChangedRpc(_angleValue);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnPowerValueChangedRpc(float power)
    {
        _powerText.text = power.ToString("F1");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnAngleValueChangedRpc(float angle)
    {
        _angleText.text = angle.ToString("F1");
    }

    private void OnPowerInputChanged(string power)
    {
        if (power == "-" || float.Parse(power) < 0 || power == "")
        {
            _powerInput.text = "";
            return;
        }

        float powerInput = float.Parse(power);
        if (powerInput > 100)
            _powerInput.text = power[..^1];

        _powerValue = powerInput;
        UpdatePowerDetails();
    }

    private void OnAngleInputChanged(string angle)
    {
        if (angle == "-" || float.Parse(angle) < 0 || angle == "")
        {
            _angleInput.text = "";
            return;
        }

        float angleInput = float.Parse(angle);
        if (angleInput > 100)
            _angleInput.text = angle[..^1];

        _angleValue = angleInput;
        UpdateAngleDetails();
    }

    private void OnLaunchButtonClicked()
    {
        PlayerManager.Instance.StartLaunchProjectileForPlayerRpc(_playerId, _powerValue, _angleValue);
    }
    #endregion

    #region View Modes
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
    #endregion
}
