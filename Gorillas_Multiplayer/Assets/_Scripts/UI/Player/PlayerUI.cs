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
    private float _defaultPowerValue = 50f;
    private float _defaultAngleValue = 45f;
    public float _powerValue = 0f;
    public float _angleValue = 0f;
    [SerializeField] private GameObject _powerUpGO;
    [SerializeField] private GameObject _tooltipGO;
    [SerializeField] private TMP_Text _tooltipTitle;
    [SerializeField] private TMP_Text _tooltipText;

    public event EventHandler<OnPowerOrAngleChangedArgs> OnPowerOrAngleChanged;
    public class OnPowerOrAngleChangedArgs : EventArgs
    {
        public int PlayerId;
        public float PowerValue;
        public float AngleValue;
    }

    public override void OnNetworkSpawn()
    {
        SetupListeners();
    }

    private void GameManager_OnOnNewGame(object sender, EventArgs e)
    {
        Hide();
        SetViewMode();
        _powerValue = _defaultPowerValue;
        _angleValue = _defaultAngleValue;
    }

    private void GameManager_OnPlayerIdChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.CurrentPlayerId.Value == _playerId)
        {
            Show();

            if ((int)NetworkManager.Singleton.LocalClientId == _playerId)
            {
                ProjectileManager.Instance.SetLatestPowerAndAngleValuesRpc(_playerId, _powerValue, _angleValue);
            }
        }
        else
        {
            Hide();
        }

        if (_playerId == (int)NetworkManager.Singleton.LocalClientId)
            _launchButton.enabled = true;
        else
            _launchButton.enabled = false;
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
        ProjectileManager.Instance.SetLatestPowerAndAngleValuesRpc(_playerId, power, _angleValue);
        OnPowerOrAngleChanged?.Invoke(this, new OnPowerOrAngleChangedArgs
        {
            PlayerId = _playerId,
            PowerValue = power,
            AngleValue = _angleValue
        });
    }

    private void OnAngleSliderValueChanged(float angle)
    {
        _angleValue = angle;
        ProjectileManager.Instance.SetLatestPowerAndAngleValuesRpc(_playerId, _powerValue, angle);
        OnPowerOrAngleChanged?.Invoke(this, new OnPowerOrAngleChangedArgs
        {
            PlayerId = _playerId,
            PowerValue = _powerValue,
            AngleValue = angle
        });
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
        {
            _powerInput.text = power[..^1];
            return;
        }

        OnPowerSliderValueChanged(powerInput);
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
        {
            _angleInput.text = angle[..^1];
            return;
        }

        OnAngleSliderValueChanged(angleInput);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdatePowerAndAngleTextRpc(int playerId, float power, float angle)
    {
        if (playerId == _playerId)
        {
            _powerText.text = power.ToString("F1");
            _angleText.text = angle.ToString("F1");
        }
    }

    private void OnLaunchButtonClicked()
    {
        _launchButton.enabled = false;
        ProjectileManager.Instance.ShowPlayerTrajectoryLineRpc(_playerId, _powerValue, _angleValue, false);
        ProjectileManager.Instance.StartLaunchProjectileRpc(_powerValue, _angleValue);
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

    #region Tooltip
    public void ShowTooltip(string title, string tooltip)
    {
        _tooltipGO.SetActive(true);
        _tooltipTitle.text = title;
        _tooltipText.text = tooltip;
    }

    public void HideTooltip()
    {
        _tooltipGO.SetActive(false);
    }
    #endregion
}
