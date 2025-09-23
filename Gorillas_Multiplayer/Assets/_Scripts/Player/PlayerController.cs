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
    [SerializeField] private Transform _shieldTransform;
    private bool _showShieldNextTurn = false;
    private bool _isShieldActive = false;
    public bool IsShieldActive { get { return _isShieldActive; } }

    public void SetPlayerDetails(int playerId)
    {
        _playerId = playerId;
    }
}
