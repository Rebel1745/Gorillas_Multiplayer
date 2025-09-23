using Unity.Netcode;
using UnityEngine;

public class ProjectileManager : NetworkBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    private int _currentPlayerId;

    // Projectile stuff
    [SerializeField] private GameObject _projectilePrefab;
    private Transform _projectileLaunchPoint;
    [SerializeField] private float _defaultForceMultiplier = 0.25f;
    [SerializeField] private float _delayBeforeAttackAnimationReset = 0.5f;

    // explosion stuff
    private Transform _explosionMaskParent;
    private Transform _brokenWindowParent;
    public float _latestPowerValue;
    public float _latestAngleValue;


    // powerup stuff
    // private bool _isBigBomb = false;
    // private int _burstCount = 1;
    // private int _currentBurstNumber;
    // private float _lastLaunchTime;
    // [SerializeField] float _timeBetweenBurstFire = 0.25f;
    // bool _isBurstFiring = false;
    // bool _isVariablePower = false;
    // [SerializeField] float _variablePowerAmount = 0.5f;
    // private float _variablePowerAmountPerShotOfBurst;
    // private float _currentVariablePowerAmount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _explosionMaskParent = GameObject.Find("ExplosionMasks").transform;
        _brokenWindowParent = GameObject.Find("BrokenWindows").transform;
    }

    [Rpc(SendTo.Server)]
    public void StartLaunchProjectileRpc(float power, float angle)
    {
        _currentPlayerId = GameManager.Instance.CurrentPlayerId.Value;
        _projectileLaunchPoint = PlayerManager.Instance.Players[_currentPlayerId].PlayerController.ProjectileLaunchPoint;
        PlayerManager.Instance.SetPlayerAnimation(_currentPlayerId, "Throw");
        StartCoroutine(PlayerManager.Instance.ResetAnimation(_currentPlayerId, _delayBeforeAttackAnimationReset));
        LaunchProjectile(power, angle);
    }

    private void LaunchProjectile(float power, float angle)
    {
        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);

        float angleRad = Mathf.Deg2Rad * angle;
        Vector2 force = new(
            _defaultForceMultiplier * power * Mathf.Cos(angleRad),
            _defaultForceMultiplier * power * Mathf.Sin(angleRad)
        );
        force.x *= PlayerManager.Instance.Players[_currentPlayerId].ThrowDirection;
        projectile.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        IProjectile iProjectile = projectile.GetComponent<IProjectile>();
        iProjectile.SetProjectileParents(_explosionMaskParent, _brokenWindowParent);

        CameraManager.Instance.UpdateCameraForProjectileRpc();

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowPlayerTrajectoryLineRpc(int playerId, bool drawTrajectoryLine)
    {
        ShowPlayerTrajectoryLineRpc(playerId, _latestPowerValue, _latestAngleValue, drawTrajectoryLine);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowPlayerTrajectoryLineRpc(int playerId, float power, float angle, bool drawTrajectoryLine)
    {
        PlayerManager.Instance.Players[playerId].PlayerTrajectoryLine.CalculateTrajectoryLine(power, angle, PlayerManager.Instance.Players[playerId].ThrowDirection, _defaultForceMultiplier);
        if (drawTrajectoryLine) PlayerManager.Instance.Players[playerId].PlayerTrajectoryLine.DrawTrajectoryLine();
        else
            HidePlayerTrajectoryLineRpc(_currentPlayerId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void HidePlayerTrajectoryLineRpc(int playerId)
    {
        if (playerId == GameManager.Instance.CurrentPlayerId.Value)
            PlayerManager.Instance.Players[playerId].PlayerTrajectoryLine.HideTrajectoryLine();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetLatestPowerAndAngleValuesRpc(int playerId, float power, float angle)
    {
        _latestPowerValue = power;
        _latestAngleValue = angle;

        HidePlayerTrajectoryLineRpc(playerId);
    }
}
