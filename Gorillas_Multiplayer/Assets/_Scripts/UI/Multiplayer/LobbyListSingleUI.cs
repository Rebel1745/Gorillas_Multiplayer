using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _playersText;
    [SerializeField] private TextMeshProUGUI _usePowerupsText;
    [SerializeField] private TextMeshProUGUI _numberOfRoundsText;

    private Lobby lobby;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinLobby(lobby);
        });
    }

    public void UpdateLobby(Lobby lobby)
    {
        this.lobby = lobby;

        _lobbyNameText.text = lobby.Name;
        _playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        string usePowerupsString = lobby.Data[LobbyManager.KEY_USE_POWERUPS].Value;
        _usePowerupsText.text = usePowerupsString == "True" ? "Yes" : "No";
        _numberOfRoundsText.text = lobby.Data[LobbyManager.KEY_ROUNDS].Value;
    }


}