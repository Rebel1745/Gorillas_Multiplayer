using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    private int _currentRound = 0;
    public int CurrentRound { get { return _currentRound; } }

    #region Events
    public event EventHandler OnNewGame;
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateGameState(GameState newState, float delay = 0f)
    {
        PreviousState = State;
        State = newState;

        switch (newState)
        {
            case GameState.WaitingForClientConnection:
                WaitingForClientConnection();
                break;
            case GameState.InitialiseGame:
                InitialiseGame();
                break;
            case GameState.BuildLevel:
                BuildLevel();
                break;
            case GameState.SetupPlayers:
                SetupPlayers();
                break;
            case GameState.SetupGame:
                SetupGame();
                break;
            case GameState.WaitingForLaunch:
                break;
            case GameState.WaitingForDetonation:
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            UpdateGameState(GameState.WaitingForClientConnection);
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            // Setup game
            UpdateGameState(GameState.InitialiseGame);
        }
    }

    private void WaitingForClientConnection()
    {
        UIManager.Instance.UpdateStatusScreenText("Waiting for client to connect...");
    }

    private void InitialiseGame()
    {
        UIManager.Instance.UpdateStatusScreenText("Initialising game...");
        // in here we initialse the scores (move to a new script - i.e. GameScore.cs and GameScoreUI.cs)
        // reset the current round number and current player id
        OnNewGameEventRpc();
        _currentRound = 0;
        UpdateGameState(GameState.BuildLevel);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnNewGameEventRpc()
    {
        OnNewGame?.Invoke(this, EventArgs.Empty);
    }

    private void BuildLevel()
    {
        UIManager.Instance.UpdateStatusScreenText("Building level...");
        LevelManager.Instance.BuildLevel();
    }

    private void SetupPlayers()
    {
        UIManager.Instance.UpdateStatusScreenText("Setting up players...");
        PlayerManager.Instance.SetupPlayers();
    }

    private void SetupGame()
    {
        UIManager.Instance.UpdateStatusScreenText("Setting up game...");
        UIManager.Instance.ShowHideUIElementRpc(UIManager.Instance.StatusScreenUI, false);
    }
}

public enum GameState
{
    None,
    WaitingForClientConnection,
    StartScreen,
    SettingsScreen,
    GameSetupScreen,
    InitialiseGame,
    BuildLevel,
    SetupPlayers,
    SetupGame,
    WaitingForLaunch,
    WaitingForDetonation,
    WaitingForMovement,
    WaitingForBuildingMovement,
    NextTurn,
    RoundComplete,
    GameOver,
    MultiplayerScreen
}
