using System;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;

public class ShieldManager : NetworkBehaviour
{
    public static ShieldManager Instance { get; private set; }

    private bool[] _playerShieldActiveNextTurn;
    private bool[] _isShieldActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _playerShieldActiveNextTurn = new bool[2];
        _isShieldActive = new bool[2];
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnCurrentPlayerIdChanged += GameManager_OnCurrentPlayerIdChanged;
        ProjectileManager.Instance.OnProjectileLaunched += ProjectileManager_OnProjectileLaunched;
    }

    private void GameManager_OnCurrentPlayerIdChanged(object sender, EventArgs e)
    {
        int playerId = GameManager.Instance.CurrentPlayerId.Value;
        if (_isShieldActive[playerId]) EnableDisableShieldRpc(playerId, false);
    }

    private void ProjectileManager_OnProjectileLaunched(object sender, EventArgs e)
    {
        int otherPlayerId = PlayerManager.Instance.GetOtherPlayerId();

        if (_playerShieldActiveNextTurn[otherPlayerId])
        {
            EnableDisableShieldRpc(otherPlayerId, true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnableDisableShieldRpc(int playerId, bool enable)
    {
        _isShieldActive[playerId] = enable;
        PlayerManager.Instance.Players[playerId].PlayerController.ShieldGameObject.SetActive(enable);
        PlayerManager.Instance.Players[playerId].PlayerController.GorillaCollider.enabled = !enable;

        // if the shield is being enabled, then it should not be enable next turn
        if (enable) _playerShieldActiveNextTurn[playerId] = false;
    }

    [Rpc(SendTo.Server)]
    public void SetShieldForNextTurnRpc(int playerId, bool active)
    {
        _playerShieldActiveNextTurn[playerId] = active;
    }
}
