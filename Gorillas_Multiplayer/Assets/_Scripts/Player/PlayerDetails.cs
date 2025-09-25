using UnityEngine;

[System.Serializable]
public struct PlayerDetails
{
    public string Name;
    public GameObject PlayerUIGO;
    public Transform PlayerUIPowerupHolder;
    public PlayerController PlayerController;
    public GameObject PlayerPrefab;
    public Animator PlayerAnimator;
    public GameObject PlayerGameObject;
    public LineRenderer PlayerLineRenderer;
    public TrajectoryLine PlayerTrajectoryLine;
    public PlayerUI PlayerUI;
    public int ThrowDirection; // 1 for left - right, -1 for right to left
    public int SpawnPointIndex;
    public GameObject PlayerMovementSpritePrefab;
    public GameObject PlayerMovementSpriteGO;
}