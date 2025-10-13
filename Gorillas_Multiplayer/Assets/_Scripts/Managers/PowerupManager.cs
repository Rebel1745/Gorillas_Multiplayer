using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PowerupManager : NetworkBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [SerializeField] private GameObject[] _availablePowerups;
    private List<GameObject>[] _playerPowerups = new List<GameObject>[2];
    private List<string>[] _playerPowerupNames = new List<string>[2];
    private List<int>[] _playerPowerupCounts = new List<int>[2];

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        _playerPowerups = new List<GameObject>[2];
        _playerPowerupNames = new List<string>[2];
        _playerPowerupCounts = new List<int>[2];

        _playerPowerups[0] = new();
        _playerPowerups[1] = new();

        _playerPowerupNames[0] = new();
        _playerPowerupNames[1] = new();

        _playerPowerupCounts[0] = new();
        _playerPowerupCounts[1] = new();

        GameManager.Instance.OnNewGame += GameManager_OnNewGame;
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;
        GameManager.Instance.OnCurrentPlayerIdChanged += GameManager_OnCurrentPlayerIdChanged;
    }

    private void GameManager_OnCurrentPlayerIdChanged(object sender, EventArgs e)
    {
        // set counts and delete if required
        int otherPlayerId = PlayerManager.Instance.GetOtherPlayerId();
        // player has changed, loop through the counts of the powerups
        for (int i = 0; i < _playerPowerupCounts[otherPlayerId].Count; i++)
        {
            // if we have no uses left, delete the button
            if (_playerPowerupCounts[otherPlayerId][i] == 0)
            {
                RemovePlayerPowerupRpc(otherPlayerId, _playerPowerups[otherPlayerId][i]);
            }
            else
            {
                // otherwise, update the count
                _playerPowerups[otherPlayerId][i].GetComponent<Powerup>().SetPowerupCountTextRpc(_playerPowerupCounts[otherPlayerId][i]);
            }
        }

        // enable all player powerup buttons
        EnableDisableAllPlayerPowerupButtonsRpc(GameManager.Instance.CurrentPlayerId.Value, true, true);
    }

    private void GameManager_OnNewGame(object sender, System.EventArgs e)
    {
        _playerPowerups[0].Clear();
        _playerPowerups[1].Clear();

        _playerPowerupNames[0].Clear();
        _playerPowerupNames[1].Clear();

        _playerPowerupCounts[0].Clear();
        _playerPowerupCounts[1].Clear();
    }

    private void GameManager_OnGameOver(object sender, System.EventArgs e)
    {
        if (!IsServer) return;
        RemoveAllPowerupsRpc();
    }

    [Rpc(SendTo.Server)]
    public void AddRandomPlayerPowerupRpc(int playerId)
    {
        if (!SettingsManager.Instance.UsePowerups) return;

        int randomPowerupIndex = UnityEngine.Random.Range(0, _availablePowerups.Length);
        GameObject powerup = _availablePowerups[randomPowerupIndex];
        string puName = powerup.name + "(Clone)";
        List<GameObject> ppuList = _playerPowerups[playerId];
        List<string> ppuNameList = _playerPowerupNames[playerId];
        List<int> ppuCountList = _playerPowerupCounts[playerId];
        int powerupId;

        if (ppuNameList.Contains(puName))
        {
            powerupId = ppuNameList.IndexOf(puName);
            ppuCountList[powerupId]++;
            ppuList[powerupId].GetComponent<Powerup>().SetPowerupCountTextRpc(ppuCountList[powerupId]);
        }
        else
        {
            GameObject pu = Instantiate(powerup);
            NetworkObject puNO = pu.GetComponent<NetworkObject>();
            puNO.Spawn(true);
            puNO.TrySetParent(PlayerManager.Instance.Players[playerId].PlayerUIPowerupHolder);
            pu.GetComponent<Powerup>().SetPlayerIdRpc(playerId);
            ppuList.Add(pu);
            _playerPowerups[playerId] = ppuList;
            ppuNameList.Add(pu.name);
            _playerPowerupNames[playerId] = ppuNameList;
            ppuCountList.Add(1);
            _playerPowerupCounts[playerId] = ppuCountList;
            powerupId = 1;
            pu.GetComponent<Powerup>().SetPowerupCountTextRpc(1);
        }
    }

    [Rpc(SendTo.Server)]
    public void RemovePowerupUseRpc(int playerId, FixedString64Bytes powerupName, bool instantUpdate = false)
    {
        GameObject pu = GetPowerupFromName(playerId, powerupName);
        int powerupIndex;

        if (pu == null) return;

        powerupIndex = GetPowerupIndexFromPlayerPowerup(playerId, pu);

        if (powerupIndex == -1) return;

        _playerPowerupCounts[playerId][powerupIndex] -= 1;

        if (!instantUpdate) return;

        if (_playerPowerupCounts[playerId][powerupIndex] == 0)
            RemovePlayerPowerupRpc(playerId, _playerPowerups[playerId][powerupIndex]);
        else
            pu.GetComponent<Powerup>().SetPowerupCountTextRpc(_playerPowerupCounts[playerId][powerupIndex]);
    }

    public GameObject GetPowerupFromName(int playerId, FixedString64Bytes powerupName)
    {
        List<GameObject> ppuList = _playerPowerups[playerId];
        List<string> ppuNameList = _playerPowerupNames[playerId];
        string puName = powerupName + "(Clone)";
        GameObject pu;

        if (ppuNameList.Contains(puName))
        {
            pu = ppuList[ppuNameList.IndexOf(puName)];

            return pu;
        }
        else
        {
            //Debug.LogError($"EnableDisablePowerupButtonRpc: Can't find powerup {powerupName} {puName}");
            return null;
        }
    }

    public int GetPowerupIndexFromPlayerPowerup(int playerid, GameObject powerup)
    {
        int powerupIndex = -1;
        List<GameObject> ppuList = _playerPowerups[playerid];

        if (ppuList.Contains(powerup))
            powerupIndex = ppuList.IndexOf(powerup);

        return powerupIndex;
    }

    [Rpc(SendTo.Server)]
    public void EnableDisablePowerupButtonRpc(int playerId, FixedString64Bytes powerupName, bool enable, bool changeColour)
    {
        GameObject pu = GetPowerupFromName(playerId, powerupName);

        if (pu != null) EnableDisablePowerupButtonRpc(pu, enable, changeColour);
    }

    [Rpc(SendTo.Server)]
    public void EnableDisablePowerupButtonRpc(NetworkObjectReference powerup, bool enable, bool changeColour)
    {
        if (!powerup.TryGet(out NetworkObject powerupNO))
        {
            Debug.Log("EnableDisablePowerupButtonRpc Error: Could not retrieve NetworkObject");
            return;
        }
        powerupNO.gameObject.GetComponent<Powerup>().EnableDisableButtonRpc(enable, changeColour);
    }

    [Rpc(SendTo.Server)]
    public void EnableDisableAllPlayerPowerupButtonsRpc(int playerid, bool enable, bool changeColour)
    {
        for (int i = 0; i < _playerPowerups[playerid].Count; i++)
        {
            _playerPowerups[playerid][i].GetComponent<Powerup>().EnableDisableButtonRpc(enabled, changeColour);
        }
    }

    [Rpc(SendTo.Server)]
    public void RemovePlayerPowerupRpc(int playerId, NetworkObjectReference powerup)
    {
        if (!powerup.TryGet(out NetworkObject powerupNO))
        {
            Debug.Log("RemovePlayerPowerupRpc Error: Could not retrieve NetworkObject");
            return;
        }

        string puName = powerupNO.gameObject.name;

        _playerPowerupCounts[playerId].RemoveAt(GetPowerupIndexFromPlayerPowerup(playerId, powerup));
        _playerPowerups[playerId].Remove(powerupNO.gameObject);
        _playerPowerupNames[playerId].Remove(puName);

        //Destroy(powerupNO.gameObject);
        powerupNO.Despawn(true);
    }

    [Rpc(SendTo.Server)]
    public void RemoveAllPowerupsRpc()
    {
        // clear out all of the powerup buttons
        for (int i = 0; i < 2; i++)
        {
            if (PlayerManager.Instance.Players[i].PlayerUIPowerupHolder.childCount == 0) continue;

            for (int j = 0; j < PlayerManager.Instance.Players[i].PlayerUIPowerupHolder.childCount; j++)
            {
                //Destroy(PlayerManager.Instance.Players[i].PlayerUIPowerupHolder.GetChild(j).gameObject);
                PlayerManager.Instance.Players[i].PlayerUIPowerupHolder.GetChild(j).gameObject.GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }
}
