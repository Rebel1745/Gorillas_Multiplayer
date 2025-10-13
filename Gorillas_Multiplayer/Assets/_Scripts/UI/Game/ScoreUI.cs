using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreUI : NetworkBehaviour
{
    private int[] _playerScores;
    private int[] _gamesLostInARow;
    [SerializeField] private TMP_Text _scoreText;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnNewGame += GameManager_OnOnNewGame;
        GameManager.Instance.OnRoundComplete += GameManager_OnRoundComplete;
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;
    }

    private void GameManager_OnOnNewGame(object sender, EventArgs e)
    {
        _playerScores = new int[2];
        _gamesLostInARow = new int[2];

        Show();

        UpdateScoreText();
    }

    private void GameManager_OnRoundComplete(object sender, GameManager.OnRoundCompleteArgs e)
    {
        AddScore(e.WinningPlayerId);
    }

    private void GameManager_OnGameOver(object sender, EventArgs e)
    {
        UIManager.Instance.GameOverUI.GetComponent<GameOverUI>().SetGameOverDetails(_playerScores);
        Hide();
    }

    private void OnSettingsButtonClicked()
    {
        SettingsManager.Instance.Show();
    }

    private void UpdateScoreText()
    {
        _scoreText.text = _playerScores[0] + " - " + _playerScores[1];
    }

    public void AddScore(int winningPlayerId)
    {
        int otherPlayerId = PlayerManager.Instance.GetOtherPlayerId(winningPlayerId);
        _playerScores[winningPlayerId]++;
        _gamesLostInARow[winningPlayerId] = 0;
        _gamesLostInARow[otherPlayerId]++;

        UpdateScoreText();

        if (!IsServer) return;

        for (int i = 0; i < _gamesLostInARow[otherPlayerId]; i++)
        {
            PowerupManager.Instance.AddRandomPlayerPowerupRpc(otherPlayerId);
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}