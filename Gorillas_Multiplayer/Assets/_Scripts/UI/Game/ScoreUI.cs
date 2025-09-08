using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ScoreUI : NetworkBehaviour
{
    private int[] _playerScores;
    private int[] _gamesLostInARow;
    [SerializeField] private TMP_Text _scoreText;

    private void Start()
    {
        GameManager.Instance.OnNewGame += GameManager_OnOnNewGame;
    }

    private void GameManager_OnOnNewGame(object sender, EventArgs e)
    {
        _playerScores = new int[2];
        _gamesLostInARow = new int[2];

        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        _scoreText.text = _playerScores[0] + " - " + _playerScores[1];
    }

    public void AddScore(int winningPlayerId)
    {
        _playerScores[winningPlayerId]++;
        _gamesLostInARow[winningPlayerId] = 0;
        _gamesLostInARow[PlayerManager.Instance.GetOtherPlayerId(winningPlayerId)]++;

        UpdateScoreText();
    }
}