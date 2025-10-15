using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    public static CreateLobbyUI Instance { get; private set; }

    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField _lobbyNameInput;
    [SerializeField] private TMP_Dropdown _numberOfRoundsDropdown;
    [SerializeField] private Toggle _usePowerupsToggle;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        createButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(
                _lobbyNameInput.text,
                false,
                _numberOfRoundsDropdown.options[_numberOfRoundsDropdown.value].text,
                _usePowerupsToggle.isOn
            );
            Hide();
        });

        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }


}
