using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject NetworkManagerUI;
    public GameObject StatusScreenUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowHideUIElement(GameObject element, bool show)
    {
        element.SetActive(show);
    }

    public void UpdateStatusScreenText(string text)
    {
        StatusScreenUI.GetComponent<StatusScreenUI>().UpdateStatusScreenTextRpc(text);
    }
}
