using UnityEngine;

public class MovePlayerArrow : MonoBehaviour
{
    private int _arrowIndex;
    public int ArrowIndex { get { return _arrowIndex; } }

    public void SetArrowIndex(int index)
    {
        _arrowIndex = index;
    }

    /*private void OnMouseEnter()
    {
        Debug.Log("OnMouseEnter");
        PlayerMovementManager.Instance.SetPlayerMovementSpriteRpc(_arrowIndex);
    }

    private void OnMouseExit()
    {
        Debug.Log("OnMouseExit");
        PlayerMovementManager.Instance.HidePlayerMovementSpriteRpc();
    }*/
}
