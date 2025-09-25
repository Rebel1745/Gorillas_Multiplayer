using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int _playerId;
    public int PlayerId { get { return _playerId; } }

    // Projectile stuff
    [SerializeField] private Transform _projectileLaunchPoint;
    public Transform ProjectileLaunchPoint { get { return _projectileLaunchPoint; } }

    // powerup stuff
    [SerializeField] private Collider2D _gorillaCollider;
    public Collider2D GorillaCollider { get { return _gorillaCollider; } }
    [SerializeField] private GameObject _shieldGameObject;
    public GameObject ShieldGameObject { get { return _shieldGameObject; } }

    public void SetPlayerDetails(int playerId)
    {
        _playerId = playerId;
    }
}
