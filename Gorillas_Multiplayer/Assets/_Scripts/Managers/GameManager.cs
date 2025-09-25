using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    public NetworkVariable<int> CurrentPlayerId = new();
    [SerializeField] private int _numberOfRounds = 5;
    private int _currentRound = 0;
    public int CurrentRound { get { return _currentRound; } }
    [SerializeField] private float _timeBetweenRounds = 3f;
    private bool _usePowerups = true;
    public bool UsePowerups { get { return _usePowerups; } }

    #region Events
    public event EventHandler OnNewGame;
    public event EventHandler OnCurrentPlayerIdChanged;
    public event EventHandler<OnRoundCompleteArgs> OnRoundComplete;
    public class OnRoundCompleteArgs : EventArgs
    {
        public int WinningPlayerId;
    }
    public event EventHandler OnGameOver;
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
            case GameState.WaitingForMovement:
                //WaitingForMovement();
                break;
            case GameState.NextTurn:
                StartCoroutine(nameof(NextTurn), delay);
                break;
            case GameState.RoundComplete:
                StartCoroutine(RoundComplete(_timeBetweenRounds));
                break;
            case GameState.GameOver:
                StartCoroutine(nameof(GameOver), delay);
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentPlayerId.Value = 3;
            UpdateGameState(GameState.WaitingForClientConnection);
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        CurrentPlayerId.OnValueChanged += (int oldValue, int newValue) =>
        {
            OnCurrentPlayerIdChanged?.Invoke(this, EventArgs.Empty);
        };
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
        CurrentPlayerId.Value = 1;
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
        if (_currentRound == 0)
        {
            CurrentPlayerId.Value = 0;
            UpdateGameState(GameState.WaitingForLaunch);
        }
        else UpdateGameState(GameState.NextTurn);
    }

    private IEnumerator NextTurn(float delay)
    {
        yield return new WaitForSeconds(delay);

        NextTurnRpc();
    }

    [Rpc(SendTo.Server)]
    private void NextTurnRpc()
    {
        // advance player
        CurrentPlayerId.Value = PlayerManager.Instance.GetOtherPlayerId(CurrentPlayerId.Value);

        UpdateGameState(GameState.WaitingForLaunch);
    }

    private IEnumerator RoundComplete(float delay)
    {
        yield return new WaitForSeconds(delay);

        RoundCompleteClientsAndHostRpc();
        RoundCompleteRpc();
    }

    [Rpc(SendTo.Server)]
    private void RoundCompleteRpc()
    {
        _currentRound++;

        if (_currentRound == _numberOfRounds)
            UpdateGameState(GameState.GameOver);
        else
            UpdateGameState(GameState.BuildLevel);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RoundCompleteClientsAndHostRpc()
    {
        OnRoundComplete?.Invoke(this, new OnRoundCompleteArgs
        {
            WinningPlayerId = CurrentPlayerId.Value
        });

        CameraManager.Instance.ResetCameraRpc();
    }

    private IEnumerator GameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameOverClientsAndHostRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void GameOverClientsAndHostRpc()
    {
        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void StartNewGameRpc()
    {
        UpdateGameState(GameState.InitialiseGame);
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
