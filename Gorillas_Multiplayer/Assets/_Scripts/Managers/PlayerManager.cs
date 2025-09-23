using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform _playerHolder;
    public PlayerDetails[] Players;
    [SerializeField] private GameObject[] _availablePowerups;
    private List<GameObject>[] _playerPowerups;
    private List<string>[] _playerPowerupNames;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetupPlayers()
    {
        if (GameManager.Instance.CurrentRound == 0)
            RemovePlayers();

        PlacePlayers();

        GameManager.Instance.UpdateGameState(GameState.SetupGame);
    }

    private void RemovePlayers()
    {
        for (int i = 0; i < _playerHolder.childCount; i++)
        {
            Destroy(_playerHolder.GetChild(i).gameObject);
        }
    }

    private void PlacePlayers()
    {
        LevelManager.Instance.GetFirstAndLastSpawnPoints(out Vector3 firstSpawnPoint, out Vector3 lastSpawnPoint, out int firstSpawnPointIndex, out int lastSpawnPointIndex);

        NetworkObject newPlayerNO;

        if (GameManager.Instance.CurrentRound == 0)
        {
            _playerPowerups = new List<GameObject>[2];
            _playerPowerups[0] = new();
            _playerPowerups[1] = new();

            _playerPowerupNames = new List<string>[2];
            _playerPowerupNames[0] = new();
            _playerPowerupNames[1] = new();

            // create player
            GameObject newPlayer = Instantiate(Players[0].PlayerPrefab, firstSpawnPoint, Quaternion.identity);
            newPlayerNO = newPlayer.GetComponent<NetworkObject>();
            newPlayerNO.Spawn(true);
            newPlayerNO.TrySetParent(_playerHolder);

            newPlayer.name = Players[0].Name;
            SetPlayersDetailsRpc(0, newPlayerNO, firstSpawnPointIndex);

            newPlayer = Instantiate(Players[1].PlayerPrefab, lastSpawnPoint, Quaternion.identity);
            newPlayerNO = newPlayer.GetComponent<NetworkObject>();
            newPlayerNO.Spawn(true);
            newPlayerNO.TrySetParent(_playerHolder);

            newPlayer.name = Players[1].Name;
            SetPlayersDetailsRpc(1, newPlayerNO, lastSpawnPointIndex);
        }
        else
        {
            PlacePlayerAndEnableRpc(0, firstSpawnPoint, firstSpawnPointIndex);

            PlacePlayerAndEnableRpc(1, lastSpawnPoint, lastSpawnPointIndex);
        }

        CameraManager.Instance.AddPlayerRpc(Players[0].PlayerGameObject.transform.position);
        CameraManager.Instance.AddPlayerRpc(Players[1].PlayerGameObject.transform.position);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayersDetailsRpc(int playerId, NetworkObjectReference player, int spawnPointIndex)
    {
        if (!player.TryGet(out NetworkObject networkObject))
        {
            Debug.Log("Error: Could not retrieve NetworkObject");
            return;
        }

        Players[playerId].PlayerGameObject = networkObject.gameObject;
        Players[playerId].PlayerController = networkObject.gameObject.GetComponent<PlayerController>();
        Players[playerId].PlayerAnimator = networkObject.gameObject.GetComponentInChildren<Animator>();
        Players[playerId].PlayerLineRenderer = networkObject.gameObject.GetComponent<LineRenderer>();
        Players[playerId].PlayerTrajectoryLine = networkObject.gameObject.GetComponent<TrajectoryLine>();
        Players[playerId].PlayerUI = Players[playerId].PlayerUIGO.GetComponent<PlayerUI>();
        Players[playerId].SpawnPointIndex = spawnPointIndex;
        Players[playerId].PlayerController.SetPlayerDetails(playerId);

        for (int i = 0; i < 49; i++)
        {
            AddRandomPlayerPowerupRpc(playerId);
        }

        if (playerId == 1)
        {
            networkObject.gameObject.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            networkObject.gameObject.transform.GetChild(1).transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        CameraManager.Instance.AddPlayerRpc(Players[playerId].PlayerGameObject.transform.position);
    }

    public int GetOtherPlayerId(int playerId)
    {
        return (playerId + 1) % 2;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DestroyPlayerRpc(int playerId)
    {
        Players[playerId].PlayerGameObject.SetActive(false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlacePlayerAndEnableRpc(int playerId, Vector3 position, int spawnPointIndex)
    {
        Players[playerId].PlayerGameObject.transform.position = position;
        Players[playerId].SpawnPointIndex = spawnPointIndex;
        Players[playerId].PlayerGameObject.SetActive(true);
        StartCoroutine(ResetAnimation(playerId, 0));
    }

    // [Rpc(SendTo.ClientsAndHost)]
    // public void ShowPlayerTrajectoryLineRpc(int playerId, bool drawTrajectoryLine)
    // {
    //     Players[playerId].PlayerTrajectoryLine.CalculateTrajectoryLine(_latestPowerValue, _latestAngleValue, Players[playerId].ThrowDirection, Players[playerId].PlayerController.DefaultForceMultiplier);
    //     if (drawTrajectoryLine) Players[playerId].PlayerTrajectoryLine.DrawTrajectoryLine();
    // }

    // [Rpc(SendTo.ClientsAndHost)]
    // public void HidePlayerTrajectoryLineRpc(int playerId)
    // {
    //     if (playerId == GameManager.Instance.CurrentPlayerId.Value)
    //         Players[playerId].PlayerTrajectoryLine.HideTrajectoryLine();
    // }

    public void SetPlayerAnimation(int playerId, string animation)
    {
        Players[playerId].PlayerAnimator.Play(animation);
    }

    public IEnumerator ResetAnimation(int playerId, float delay)
    {
        yield return new WaitForSeconds(delay);

        SetPlayerAnimation(playerId, "Idle");
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
            puNO.TrySetParent(Players[playerId].PlayerUIPowerupHolder);
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
