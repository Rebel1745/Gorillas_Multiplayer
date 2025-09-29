using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : NetworkBehaviour
{
    public static PlayerMovementManager Instance;

    private int _currentPlayerId;
    [SerializeField] private int _movementDistance = 3;
    private int _currentArrowIndex;
    public bool _isMoving;
    [SerializeField] private LayerMask _whatIsPlayerMovementArrow;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        CheckForActiveMovementRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void CheckForActiveMovementRpc()
    {
        if (GameManager.Instance.CurrentPlayerId.Value == (int)NetworkManager.Singleton.LocalClientId && _isMoving)
            CheckForMovementArrowMouseOver();
    }

    [Rpc(SendTo.Server)]
    public void ShowHideMovementPowerupIndicatorsRpc(int playerId, bool show)
    {
        SetIsMovingRpc(playerId, show);

        float lowestY = 999;
        float currentY;
        float lowestPlayerY;
        Vector3 newCameraPosition;
        int spawnPointIndex = PlayerManager.Instance.Players[_currentPlayerId].SpawnPointIndex;
        int otherPlayerSpawnPointIndex = PlayerManager.Instance.Players[PlayerManager.Instance.GetOtherPlayerId(_currentPlayerId)].SpawnPointIndex;

        // figure out the span of the arrows / spawn points
        int firstIndex = spawnPointIndex - _movementDistance;
        int lastIndex = spawnPointIndex + _movementDistance;

        // check whether player 1's last index is not too close to player 2
        if (_currentPlayerId == 0)
        {
            if (otherPlayerSpawnPointIndex - LevelManager.Instance.MinimumDistanceBetweenPlayers < lastIndex)
                lastIndex = otherPlayerSpawnPointIndex - LevelManager.Instance.MinimumDistanceBetweenPlayers;
        }
        else
        {
            // check whether player 2's first index is not too close to player 1
            if (otherPlayerSpawnPointIndex + LevelManager.Instance.MinimumDistanceBetweenPlayers > firstIndex)
                firstIndex = otherPlayerSpawnPointIndex + LevelManager.Instance.MinimumDistanceBetweenPlayers;
        }

        // show the arrows on screen
        LevelManager.Instance.ShowHideSpawnPointArrowsBetweenIndexesRpc(firstIndex, spawnPointIndex, lastIndex, show);

        if (show)
        {
            // figure out the lowest spawn point (or player if it is lower)
            for (int i = firstIndex; i <= lastIndex; i++)
            {
                currentY = LevelManager.Instance.GetSpawnPointAtIndex(i).y;
                if (currentY < lowestY)
                    lowestY = currentY;
            }

            // we have the lowest Y of the spawn points, now get the lowest player
            lowestPlayerY = Mathf.Min(PlayerManager.Instance.Players[0].PlayerGameObject.transform.position.y, PlayerManager.Instance.Players[1].PlayerGameObject.transform.position.y);
            lowestY = Mathf.Min(lowestY, lowestPlayerY);

            if (_currentPlayerId == 0)
                newCameraPosition = new(LevelManager.Instance.GetSpawnPointAtIndex(firstIndex).x, lowestY);
            else
                newCameraPosition = new(LevelManager.Instance.GetSpawnPointAtIndex(lastIndex).x, lowestY);

            CameraManager.Instance.UpdatePlayerPositionRpc(_currentPlayerId, newCameraPosition);
        }
        else
        {
            CameraManager.Instance.UpdatePlayerPositionRpc(_currentPlayerId, PlayerManager.Instance.Players[_currentPlayerId].PlayerGameObject.transform.position);
            GameManager.Instance.UpdateGameState(GameState.WaitingForLaunch);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetIsMovingRpc(int playerId, bool isMoving)
    {
        _currentPlayerId = playerId;
        _isMoving = isMoving;
    }

    private void CheckForMovementArrowMouseOver()
    {
        int arrowIndex;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Collider2D hit = Physics2D.OverlapPoint(mousePos, _whatIsPlayerMovementArrow);

        if (hit)
        {
            arrowIndex = hit.gameObject.GetComponent<MovePlayerArrow>().ArrowIndex;
            SetPlayerMovementSpriteRpc(arrowIndex);
        }
        else HidePlayerMovementSpriteRpc();
    }

    [Rpc(SendTo.Server)]
    public void SetPlayerMovementSpriteRpc(int arrowIndex)
    {
        if (_currentArrowIndex != arrowIndex)
        {
            _currentArrowIndex = arrowIndex;
            Vector3 spawnPointPosition = LevelManager.Instance.GetSpawnPointAtIndex(arrowIndex);
            PlayerManager.Instance.Players[_currentPlayerId].PlayerMovementSpriteGO.transform.position = spawnPointPosition;
            EnablePlayerMovementSpriteRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnablePlayerMovementSpriteRpc()
    {
        PlayerManager.Instance.Players[_currentPlayerId].PlayerMovementSpriteGO.SetActive(true);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void HidePlayerMovementSpriteRpc()
    {
        PlayerManager.Instance.Players[_currentPlayerId].PlayerMovementSpriteGO.SetActive(false);
        _currentArrowIndex = -1;
    }

    [Rpc(SendTo.Server)]
    public void ConfirmMovementPowerupPositionRpc()
    {
        if (_currentArrowIndex == -1) return;

        Vector3 currentArrowPosition = LevelManager.Instance.GetSpawnPointAtIndex(_currentArrowIndex);

        ShowHideMovementPowerupIndicatorsRpc(_currentPlayerId, false);
        PlayerManager.Instance.Players[_currentPlayerId].PlayerGameObject.transform.position = currentArrowPosition;
        PlayerManager.Instance.Players[_currentPlayerId].SpawnPointIndex = _currentArrowIndex;
        HidePlayerMovementSpriteRpc();
        CameraManager.Instance.UpdatePlayerPositionRpc(_currentPlayerId, currentArrowPosition);

        GameManager.Instance.UpdateGameState(GameState.WaitingForLaunch);
    }
}
