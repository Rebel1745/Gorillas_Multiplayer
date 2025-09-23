using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using System;

public class Powerup : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int _playerId;
    private PlayerUI _playerUI;
    private int _remainingUses = 1;
    [SerializeField] protected Button _powerupButton;
    [SerializeField] protected TMP_Text _powerupNumberText;
    [SerializeField] protected string _powerupTitle;
    [SerializeField] protected string _powerupText;
    [SerializeField] protected Color _defaultColour = new(169, 169, 169);
    [SerializeField] protected Color _inUseColour = new(100, 200, 100);
    [SerializeField] protected Color _usedColour = new(200, 100, 100);
    protected bool _powerupEnabled = false;

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

        //InputManager.Instance.SetCurrentPowerupButton(_powerupButton);

        if (_powerupEnabled)
            _remainingUses--;
        else
            _remainingUses++;

        UpdatePowerupNumberTextRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableDisableButtonRpc()
    {
        EnableDisableButtonRpc(!_powerupButton.enabled);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableDisableButtonRpc(bool enabled)
    {
        _powerupButton.enabled = enabled;
        _powerupEnabled = !enabled;
        UpdatePowerupNumberTextRpc();

        if (enabled)
        {
            SetButtonColourRpc(_defaultColour);
        }
        else
        {
            SetButtonColourRpc(_usedColour);
        }

        if (_remainingUses == 0 && !_powerupEnabled)
            RemoveButtonRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetButtonColourRpc(Color colour)
    {
        _powerupButton.image.color = colour;
    }

    protected void SetButtonColour(Color colour)
    {
        // same as the above function but it is only set locally
        // this stops the other player knowing when the shield is used
        _powerupButton.image.color = colour;
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void RemoveButtonRpc()
    {
        PlayerManager.Instance.RemovePlayerPowerup(gameObject);
        HideTooltip();
        Destroy(gameObject);
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
