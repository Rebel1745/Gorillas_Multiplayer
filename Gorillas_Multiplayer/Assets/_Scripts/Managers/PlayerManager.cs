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
            //Destroy(_playerHolder.GetChild(i).gameObject);
            _playerHolder.GetChild(i).gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    private void PlacePlayers()
    {
        LevelManager.Instance.GetFirstAndLastSpawnPoints(out Vector3 firstSpawnPoint, out Vector3 lastSpawnPoint, out int firstSpawnPointIndex, out int lastSpawnPointIndex);

        NetworkObject newPlayerNO, playerMovementSpriteNO;

        if (GameManager.Instance.CurrentRound == 0)
        {
            // create player
            GameObject newPlayer = Instantiate(Players[0].PlayerPrefab, firstSpawnPoint, Quaternion.identity);
            newPlayerNO = newPlayer.GetComponent<NetworkObject>();
            newPlayerNO.Spawn(true);
            newPlayerNO.TrySetParent(_playerHolder);

            GameObject movementSpriteGO = Instantiate(Players[0].PlayerMovementSpritePrefab);
            playerMovementSpriteNO = movementSpriteGO.GetComponent<NetworkObject>();
            playerMovementSpriteNO.Spawn(true);
            playerMovementSpriteNO.TrySetParent(_playerHolder);

            newPlayer.name = Players[0].Name;
            SetPlayersDetailsRpc(0, newPlayerNO, playerMovementSpriteNO, firstSpawnPointIndex);

            newPlayer = Instantiate(Players[1].PlayerPrefab, lastSpawnPoint, Quaternion.identity);
            newPlayerNO = newPlayer.GetComponent<NetworkObject>();
            newPlayerNO.Spawn(true);
            newPlayerNO.TrySetParent(_playerHolder);

            movementSpriteGO = Instantiate(Players[1].PlayerMovementSpritePrefab);
            playerMovementSpriteNO = movementSpriteGO.GetComponent<NetworkObject>();
            playerMovementSpriteNO.Spawn(true);
            playerMovementSpriteNO.TrySetParent(_playerHolder);

            newPlayer.name = Players[1].Name;
            SetPlayersDetailsRpc(1, newPlayerNO, playerMovementSpriteNO, lastSpawnPointIndex);
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
    private void SetPlayersDetailsRpc(int playerId, NetworkObjectReference player, NetworkObjectReference playerMovementSprite, int spawnPointIndex)
    {
        if (!player.TryGet(out NetworkObject playerNO))
        {
            Debug.Log("Error: Could not retrieve playerNetworkObject");
            return;
        }

        if (!playerMovementSprite.TryGet(out NetworkObject playerMovementSpriteNO))
        {
            Debug.Log("Error: Could not retrieve playerNetworkObject");
            return;
        }

        Players[playerId].PlayerGameObject = playerNO.gameObject;
        Players[playerId].PlayerController = playerNO.gameObject.GetComponent<PlayerController>();
        Players[playerId].PlayerAnimator = playerNO.gameObject.GetComponentInChildren<Animator>();
        Players[playerId].PlayerLineRenderer = playerNO.gameObject.GetComponent<LineRenderer>();
        Players[playerId].PlayerTrajectoryLine = playerNO.gameObject.GetComponent<TrajectoryLine>();
        Players[playerId].PlayerUI = Players[playerId].PlayerUIGO.GetComponent<PlayerUI>();
        Players[playerId].SpawnPointIndex = spawnPointIndex;
        Players[playerId].PlayerController.SetPlayerDetails(playerId);
        Players[playerId].PlayerMovementSpriteGO = playerMovementSpriteNO.gameObject;
        Players[playerId].PlayerMovementSpriteGO.transform.position = Players[playerId].PlayerGameObject.transform.position;
        Players[playerId].PlayerMovementSpriteGO.SetActive(false);

        // set outline colour
        string savedColourString = PlayerPrefs.GetString("PlayerOutlineColour", ColorUtility.ToHtmlStringRGBA(SettingsManager.Instance.DefaultPlayerOutlineColour));
        ColorUtility.TryParseHtmlString("#" + savedColourString, out Color savedColour);
        Material mat = playerNO.gameObject.GetComponentInChildren<SpriteRenderer>().material;
        mat.SetColor("_SolidOutline", savedColour);

        // UI size
        float savedUIScale = PlayerPrefs.GetFloat("UIScale", 1f);
        Players[playerId].PlayerUI.transform.localScale = new Vector3(savedUIScale, savedUIScale, 0);

        if (playerId == 1)
        {
            playerNO.gameObject.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            playerNO.gameObject.transform.GetChild(1).transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        CameraManager.Instance.AddPlayerRpc(Players[playerId].PlayerGameObject.transform.position);

        // for (int i = 0; i < 50; i++)
        // {
        //     PowerupManager.Instance.AddRandomPlayerPowerupRpc(playerId);
        // }
    }

    public int GetOtherPlayerId()
    {
        return (GameManager.Instance.CurrentPlayerId.Value + 1) % 2;
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
        Players[playerId].PlayerMovementSpriteGO.transform.position = position;
        Players[playerId].PlayerMovementSpriteGO.SetActive(false);
        Players[playerId].SpawnPointIndex = spawnPointIndex;
        Players[playerId].PlayerGameObject.SetActive(true);
        StartCoroutine(ResetAnimation(playerId, 0));
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
