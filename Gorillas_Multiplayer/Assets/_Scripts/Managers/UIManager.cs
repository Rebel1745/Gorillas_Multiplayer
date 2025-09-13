using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject NetworkManagerUI;
    public GameObject StatusScreenUI;
    public GameObject GameUI;
    public GameObject GameOverUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowHideUIElementRpc(NetworkObjectReference element, bool show)
    {
        if (!element.TryGet(out NetworkObject networkObject))
        {
            Debug.Log("Error: Could not retrieve NetworkObject");
            return;
        }
        networkObject.gameObject.SetActive(show);
    }

    public void UpdateStatusScreenText(string text)
    {
        StatusScreenUI.GetComponent<StatusScreenUI>().UpdateStatusScreenTextRpc(text);
    }
}
