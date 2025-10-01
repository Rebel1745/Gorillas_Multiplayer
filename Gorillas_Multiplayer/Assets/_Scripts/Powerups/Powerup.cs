using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Netcode;
using System;

public class Powerup : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int _playerId;
    private PlayerUI _playerUI;
    private int _remainingUses = 1;
    [SerializeField] protected Button _powerupButton;
    protected NetworkObject _powerupButtonNO;
    [SerializeField] protected TMP_Text _powerupNumberText;
    [SerializeField] protected string _powerupTitle;
    [SerializeField] protected string _powerupText;
    [SerializeField] protected Color _defaultColour = new(169, 169, 169);
    [SerializeField] protected Color _inUseColour = new(100, 200, 100);
    [SerializeField] protected Color _usedColour = new(200, 100, 100);
    [SerializeField] protected bool _isCovertUse = false;
    public bool _powerupEnabled = false;

    public override void OnNetworkSpawn()
    {
        _powerupButtonNO = _powerupButton.GetComponent<NetworkObject>();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetPlayerIdRpc(int playerId)
    {
        _playerId = playerId;
        _playerUI = PlayerManager.Instance.Players[playerId].PlayerUI.GetComponent<PlayerUI>();

        UpdatePowerupNumberTextRpc();

        if ((int)NetworkManager.Singleton.LocalClientId == _playerId)
        {
            _powerupButton.onClick.AddListener(UsePowerup);
            GameManager.Instance.OnCurrentPlayerIdChanged += GameManager_OnCurrentPlayerIdChanged;
        }
    }

    private void GameManager_OnCurrentPlayerIdChanged(object sender, EventArgs e)
    {
        // when we change player, disable or enable powerup buttons
        if ((int)NetworkManager.Singleton.LocalClientId == _playerId)
            EnableDisableButtonRpc(true);
        else
            EnableDisableButtonRpc(false);
    }

    public virtual void UsePowerup()
    {
        _powerupEnabled = !_powerupEnabled;

        HideTooltip();

        // if (_powerupEnabled)
        //     PlayerInputManager.Instance.SetCurrentPowerupButtonRpc(_powerupButtonNO);
        // else PlayerInputManager.Instance.NullCurrentPowerupButtonRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableDisableButtonRpc(bool enabled)
    {
        _powerupButton.enabled = enabled;
        _powerupEnabled = !enabled;
        // UpdatePowerupNumberTextRpc();

        if (enabled)
        {
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _defaultColour);
        }
        else
        {
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _usedColour);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetButtonColourRpc(Color colour)
    {
        _powerupButton.image.color = colour;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetButtonColourRpc(POWERUP_BUTTON_COLOUR colour)
    {
        switch (colour)
        {
            case POWERUP_BUTTON_COLOUR.Default:
                _powerupButton.image.color = _defaultColour;
                break;
            case POWERUP_BUTTON_COLOUR.InUse:
                _powerupButton.image.color = _inUseColour;
                break;
            case POWERUP_BUTTON_COLOUR.Used:
                _powerupButton.image.color = _usedColour;
                break;
        }
    }

    protected void SetButtonColour(Color colour)
    {
        _powerupButton.image.color = colour;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AddPowerupUseRpc()
    {
        _remainingUses++;
        UpdatePowerupNumberTextRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePowerupNumberTextRpc()
    {
        if (_remainingUses > 1)
        {
            _powerupNumberText.text = _remainingUses.ToString();
            _powerupNumberText.gameObject.SetActive(true);
        }
        else
            _powerupNumberText.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_powerupButton.enabled)
            _playerUI.ShowTooltip(_powerupTitle, _powerupText);
        else
            HideTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void HideTooltip()
    {
        _playerUI.HideTooltip();
    }
}

public enum POWERUP_BUTTON_COLOUR
{
    Default,
    Used,
    InUse
}
