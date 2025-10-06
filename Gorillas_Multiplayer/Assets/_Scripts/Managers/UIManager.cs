using System;
using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject NetworkManagerUI;
    public GameObject StatusScreenUI;
    public GameObject GameUI;
    public GameObject GameOverUI;
    public GameObject SettingsUI;
    [SerializeField] PlayerUI[] _playerUIs;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        _playerUIs[0].OnPowerOrAngleChanged += PlayerUI_OnPowerOrAngleChanged;
        _playerUIs[1].OnPowerOrAngleChanged += PlayerUI_OnPowerOrAngleChanged;
    }

    private void PlayerUI_OnPowerOrAngleChanged(object sender, PlayerUI.OnPowerOrAngleChangedArgs e)
    {
        UpdatePowerAndAngleValuesRpc(e.PlayerId, e.PowerValue, e.AngleValue);
    }

    [Rpc(SendTo.Server)]
    public void UpdatePowerAndAngleValuesRpc(int playerId, float power, float angle)
    {
        _playerUIs[playerId].UpdatePowerAndAngleTextRpc(playerId, power, angle);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowHideUIElementRpc(NetworkObjectReference element, bool show)
    {
        if (!element.TryGet(out NetworkObject networkObject))
        {
            Debug.Log("ShowHideUIElementRpc Error: Could not retrieve NetworkObject");
            return;
        }
        networkObject.gameObject.SetActive(show);
    }

    public void ShowHideUIElement(GameObject element, bool show)
    {
        element.SetActive(show);
    }

    public void UpdateStatusScreenText(string text)
    {
        StatusScreenUI.GetComponent<StatusScreenUI>().UpdateStatusScreenTextRpc(text);
    }
}
