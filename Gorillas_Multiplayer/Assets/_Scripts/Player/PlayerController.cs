using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int _playerId;
    private PlayerDetails _playerDetails;
    public int PlayerId { get { return _playerId; } }

    // Projectile stuff
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileLaunchPoint;
    [SerializeField] private float _defaultForceMultiplier = 0.25f;
    public float DefaultForceMultiplier { get { return _defaultForceMultiplier; } }
    [SerializeField] private float _delayBeforeAttackAnimationReset = 0.5f;
    private int _throwDirection;
    private Transform _explosionMaskParent;

    // powerup stuff
    private bool _isBigBomb = false;
    private int _burstCount = 1;
    private int _currentBurstNumber;
    private float _lastLaunchTime;
    [SerializeField] float _timeBetweenBurstFire = 0.25f;
    bool _isBurstFiring = false;
    bool _isVariablePower = false;
    [SerializeField] float _variablePowerAmount = 0.5f;
    private float _variablePowerAmountPerShotOfBurst;
    private float _currentVariablePowerAmount;
    [SerializeField] private Collider2D _gorillaCollider;
    [SerializeField] private Transform _shieldTransform;
    private bool _showShieldNextTurn = false;
    private bool _isShieldActive = false;
    public bool IsShieldActive { get { return _isShieldActive; } }

    private void Start()
    {
        _explosionMaskParent = GameObject.Find("ExplosionMasks").transform;
    }

    public void SetPlayerDetails(int playerId, PlayerDetails pd)
    {
        _playerId = playerId;
        _playerDetails = pd;
        _throwDirection = pd.ThrowDirection;
    }

    public void StartLaunchProjectile(float power, float angle)
    {
        PlayerManager.Instance.SetPlayerAnimation(_playerId, "Throw");
        StartCoroutine(PlayerManager.Instance.ResetAnimation(_playerId, _delayBeforeAttackAnimationReset));
        LaunchProjectileRpc(power, angle);
    }

    [Rpc(SendTo.Server)]
    private void LaunchProjectileRpc(float power, float angle)
    {
        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);

        float angleRad = Mathf.Deg2Rad * angle;
        Vector2 force = new(
            _defaultForceMultiplier * power * Mathf.Cos(angleRad),
            _defaultForceMultiplier * power * Mathf.Sin(angleRad)
        );
        force.x *= _throwDirection;
        projectile.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        IProjectile iProjectile = projectile.GetComponent<IProjectile>();
        iProjectile.SetProjectileExplosionMaskParent(_explosionMaskParent);

        CameraManager.Instance.UpdateCameraForProjectileRpc();

        if (_isBigBomb)
        {
            Debug.Log("_isBigBomb");
            iProjectile.SetExplosionSizeMultiplier(2f);
            _isBigBomb = false;
        }

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
    }

    #region Powerup Functions
    public void ShowTooltip(string title, string tooltip)
    {
        _playerDetails.PlayerUI.ShowTooltip(title, tooltip);
    }

    public void HideTooltip()
    {
        _playerDetails.PlayerUI.HideTooltip();
    }

    [Rpc(SendTo.Server)]
    public void SetBigBombRpc(bool isBigBomb)
    {
        Debug.Log($"SetBigBomb {isBigBomb}");
        _isBigBomb = isBigBomb;
    }
    #endregion
}
