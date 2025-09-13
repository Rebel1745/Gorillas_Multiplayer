using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;

public class GameOverUI : NetworkBehaviour
{
    [SerializeField] private Image _player1Image;
    [SerializeField] private Image _player2Image;
    [SerializeField] private Sprite _winningSprite;
    [SerializeField] private Sprite _losingSprite;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _winnerText;
    [SerializeField] private float _winningImageSize = 150f;
    [SerializeField] private float _losingImageSize = 100f;
    [SerializeField] private float _losingImageOffset = -25f;
    [SerializeField] private AudioClip _gameOverAudio;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _mainMenuButton;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnNewGame += GameManager_OnNewGame;
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;

        _playAgainButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartNewGameRpc();
            Hide();
        });

        _mainMenuButton.onClick.AddListener(() =>
        {
            GameManager.Instance.UpdateGameState(GameState.StartScreen);
            Hide();
        });
    }

    private void GameManager_OnNewGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameOver(object sender, EventArgs e)
    {
        Show();
    }

    public void SetGameOverDetails(int[] scores)
    {
        if (scores[0] == scores[1])
        {
            // draw
            _player1Image.sprite = _losingSprite;
            _player1Image.rectTransform.sizeDelta = new(_player1Image.rectTransform.sizeDelta.x, _losingImageSize);
            _player1Image.rectTransform.localPosition = new(_player1Image.rectTransform.localPosition.x, _losingImageOffset);

            _player2Image.sprite = _losingSprite;
            _player2Image.rectTransform.sizeDelta = new(_player2Image.rectTransform.sizeDelta.x, _losingImageSize);
            _player2Image.rectTransform.localPosition = new(_player2Image.rectTransform.localPosition.x, _losingImageOffset);

            _winnerText.text = "DRAW";

        }
        // if player 1 wins
        else if (scores[0] > scores[1])
        {
            _player1Image.sprite = _winningSprite;
            _player1Image.rectTransform.sizeDelta = new(_player1Image.rectTransform.sizeDelta.x, _winningImageSize);
            _player1Image.rectTransform.localPosition = new(_player1Image.rectTransform.localPosition.x, 0f);

            _player2Image.sprite = _losingSprite;
            _player2Image.rectTransform.sizeDelta = new(_player2Image.rectTransform.sizeDelta.x, _losingImageSize);
            _player2Image.rectTransform.localPosition = new(_player2Image.rectTransform.localPosition.x, _losingImageOffset);

            _winnerText.text = "!" + PlayerManager.Instance.Players[0].Name + " WINS!";
        }
        else
        {
            _player1Image.sprite = _losingSprite;
            _player1Image.rectTransform.sizeDelta = new(_player1Image.rectTransform.sizeDelta.x, _losingImageSize);
            _player1Image.rectTransform.localPosition = new(_player1Image.rectTransform.localPosition.x, _losingImageOffset);

            _player2Image.sprite = _winningSprite;
            _player2Image.rectTransform.sizeDelta = new(_player2Image.rectTransform.sizeDelta.x, _winningImageSize);
            _player2Image.rectTransform.localPosition = new(_player2Image.rectTransform.localPosition.x, 0f);

            _winnerText.text = "!" + PlayerManager.Instance.Players[1].Name + " WINS!";
        }

        _scoreText.text = scores[0] + " - " + scores[1];

        //AudioManager.Instance.PlayAudioClip(_gameOverAudio);
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
