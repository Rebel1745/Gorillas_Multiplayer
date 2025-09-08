using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject NetworkManagerUI;
    public GameObject StatusScreenUI;

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
        Debug.Log($"{networkObject.gameObject.name} - {show}");
        networkObject.gameObject.SetActive(show);
    }

    public void UpdateStatusScreenText(string text)
    {
        StatusScreenUI.GetComponent<StatusScreenUI>().UpdateStatusScreenTextRpc(text);
    }
}
