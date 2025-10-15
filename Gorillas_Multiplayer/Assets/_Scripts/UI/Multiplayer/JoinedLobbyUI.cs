using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Transform _playerSingleTemplate;
    [SerializeField] private Transform _container;
    [SerializeField] private TMP_Text _lobbyNameText;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private TMP_Text _numberOfRoundsText;
    [SerializeField] private TMP_Text _usePowerupsText;
    [SerializeField] private Button _leaveLobbyButton;
    [SerializeField] private Button _startGameButton;

    private void Awake()
    {
        Instance = this;

        _playerSingleTemplate.gameObject.SetActive(false);

        _leaveLobbyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
        });

        _startGameButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.StartGame();
            Hide();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();

        if (!LobbyManager.Instance.IsLobbyHost())
            _startGameButton.gameObject.SetActive(false);

        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(_playerSingleTemplate, _container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        _lobbyNameText.text = lobby.Name;
        _playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        string usePowerupsString = lobby.Data[LobbyManager.KEY_USE_POWERUPS].Value;
        _usePowerupsText.text = usePowerupsString == "True" ? "Yes" : "No";
        _numberOfRoundsText.text = lobby.Data[LobbyManager.KEY_ROUNDS].Value;

        if (!gameObject.activeInHierarchy)
            Show();
    }

    private void ClearLobby()
    {
        foreach (Transform child in _container)
        {
            if (child == _playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

}