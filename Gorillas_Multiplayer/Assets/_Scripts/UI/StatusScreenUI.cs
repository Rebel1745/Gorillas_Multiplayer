using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class StatusScreenUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text _statusText;

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateStatusScreenTextRpc(FixedString64Bytes text)
    {
        _statusText.text = text.ToString();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
