using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private Transform _playerHolder;
    public PlayerDetails[] Players;

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
        Players[playerId].PlayerController.SetPlayerDetails(playerId, Players[playerId]);

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

    [Rpc(SendTo.Server)]
    public void StartLaunchProjectileForPlayerRpc(int playerId, float power, float angle)
    {
        Players[playerId].PlayerController.StartLaunchProjectile(power, angle);
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

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowPlayerTrajectoryLineRpc(int playerId, float power, float angle, bool drawTrajectoryLine)
    {
        Players[playerId].PlayerTrajectoryLine.CalculateTrajectoryLine(power, angle, Players[playerId].ThrowDirection, Players[playerId].PlayerController.DefaultForceMultiplier);
        if (drawTrajectoryLine) Players[playerId].PlayerTrajectoryLine.DrawTrajectoryLine();
    }


    public void SetPlayerAnimation(int playerId, string animation)
    {
        Players[playerId].PlayerAnimator.Play(animation);
    }

    public IEnumerator ResetAnimation(int playerId, float delay)
    {
        yield return new WaitForSeconds(delay);

        SetPlayerAnimation(playerId, "Idle");
    }
}
