using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Netcode;
using System;
using Unity.Services.Matchmaker.Models;

public class Powerup : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int _playerId;
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

        if ((int)NetworkManager.Singleton.LocalClientId == _playerId)
        {
            _powerupButton.onClick.AddListener(UsePowerup);
        }
    }

    public virtual void UsePowerup()
    {
        _powerupEnabled = !_powerupEnabled;

        HideTooltip();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableDisableButtonRpc(bool enabled, bool changeColour)
    {
        _powerupButton.enabled = enabled;
        _powerupEnabled = !enabled;

        if (!changeColour) return;

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

    protected void SetButtonColour(Color colour)
    {
        _powerupButton.image.color = colour;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetPowerupCountTextRpc(int count)
    {
        if (count > 1)
        {
            _powerupNumberText.text = count.ToString();
            _powerupNumberText.gameObject.SetActive(true);
        }
        else
            _powerupNumberText.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_powerupButton.enabled)
            PlayerManager.Instance.Players[_playerId].PlayerUI.ShowTooltip(_powerupTitle, _powerupText);
        else
            HideTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void HideTooltip()
    {
        PlayerManager.Instance.Players[_playerId].PlayerUI.HideTooltip();
    }
}

public enum POWERUP_BUTTON_COLOUR
{
    Default,
    Used,
    InUse
}
