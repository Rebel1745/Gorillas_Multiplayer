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
        //if (GameManager.Instance.CurrentRound == 0)
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
            Players[0].PlayerGameObject = newPlayer;
            Players[0].PlayerController = newPlayer.GetComponent<PlayerController>();
            Players[0].PlayerAnimator = newPlayer.GetComponentInChildren<Animator>();
            Players[0].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
            Players[0].PlayerUI = Players[0].PlayerUIGO.GetComponent<PlayerUI>();
            Players[0].SpawnPointIndex = firstSpawnPointIndex;
            Players[0].PlayerController.SetPlayerDetails(0, Players[0]);

            newPlayer = Instantiate(Players[1].PlayerPrefab, lastSpawnPoint, Quaternion.identity);
            newPlayerNO = newPlayer.GetComponent<NetworkObject>();
            newPlayerNO.Spawn(true);
            newPlayerNO.TrySetParent(_playerHolder);
            newPlayer.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            newPlayer.transform.GetChild(1).transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            newPlayer.name = Players[1].Name;
            Players[1].PlayerGameObject = newPlayer;
            Players[1].PlayerController = newPlayer.GetComponent<PlayerController>();
            Players[1].PlayerAnimator = newPlayer.GetComponentInChildren<Animator>();
            Players[1].PlayerLineRenderer = newPlayer.GetComponent<LineRenderer>();
            Players[1].PlayerUI = Players[1].PlayerUIGO.GetComponent<PlayerUI>();
            Players[1].SpawnPointIndex = lastSpawnPointIndex;
            Players[1].PlayerController.SetPlayerDetails(1, Players[1]);
        }
        else
        {
            //Players[0].PlayerController.PlacePlayerAndEnable(firstSpawnPoint, firstSpawnPointIndex);

            //Players[1].PlayerController.PlacePlayerAndEnable(lastSpawnPoint, lastSpawnPointIndex);
        }

        CameraManager.Instance.AddPlayerRpc(Players[0].PlayerGameObject.transform.position);

        CameraManager.Instance.AddPlayerRpc(Players[1].PlayerGameObject.transform.position);
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
}
