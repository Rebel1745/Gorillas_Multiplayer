using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PowerupManager : NetworkBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [SerializeField] private GameObject[] _availablePowerups;
    private List<GameObject>[] _playerPowerups;
    private List<string>[] _playerPowerupNames;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnNewGame += GameManager_OnNewGame;
    }

    private void GameManager_OnNewGame(object sender, System.EventArgs e)
    {
        _playerPowerups = new List<GameObject>[2];
        _playerPowerups[0] = new();
        _playerPowerups[1] = new();

        _playerPowerupNames = new List<string>[2];
        _playerPowerupNames[0] = new();
        _playerPowerupNames[1] = new();

        for (int i = 0; i < 49; i++)
        {
            AddRandomPlayerPowerupRpc(0);
            AddRandomPlayerPowerupRpc(1);
        }
    }

    [Rpc(SendTo.Server)]
    public void AddRandomPlayerPowerupRpc(int playerId)
    {
        if (!GameManager.Instance.UsePowerups) return;

        int randomPowerupIndex = Random.Range(0, _availablePowerups.Length);
        GameObject powerup = _availablePowerups[randomPowerupIndex];
        string puName = powerup.name + "(Clone)";
        List<GameObject> ppuList = _playerPowerups[playerId];
        List<string> ppuNameList = _playerPowerupNames[playerId];

        if (ppuNameList.Contains(puName))
        {
            ppuList[ppuNameList.IndexOf(puName)].GetComponent<Powerup>().AddPowerupUseRpc();
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
        }
    }

    [Rpc(SendTo.Server)]
    public void EnableDisablePowerupButtonRpc(int playerId, FixedString64Bytes powerupName, bool enable)
    {
        List<GameObject> ppuList = _playerPowerups[playerId];
        List<string> ppuNameList = _playerPowerupNames[playerId];
        string puName = powerupName + "(Clone)";
        GameObject pu;

        if (ppuNameList.Contains(puName))
        {
            pu = ppuList[ppuNameList.IndexOf(puName)];

            EnableDisablePowerupButtonRpc(pu, enable);
        }
        else Debug.LogError("EnableDisablePowerupButtonRpc: Can't find powerup");
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableDisablePowerupButtonRpc(NetworkObjectReference powerup, bool enable)
    {
        if (!powerup.TryGet(out NetworkObject powerupNO))
        {
            Debug.Log("Error: Could not retrieve NetworkObject");
            return;
        }
        powerupNO.gameObject.GetComponent<Powerup>().EnableDisableButtonRpc(enable);
    }

    public void RemovePlayerPowerup(GameObject powerup)
    {
        string puName = powerup.name;
        List<GameObject> ppuList = _playerPowerups[GameManager.Instance.CurrentPlayerId.Value];
        List<string> ppuNameList = _playerPowerupNames[GameManager.Instance.CurrentPlayerId.Value];

        ppuList.Remove(powerup);
        ppuNameList.Remove(puName);
    }
}
