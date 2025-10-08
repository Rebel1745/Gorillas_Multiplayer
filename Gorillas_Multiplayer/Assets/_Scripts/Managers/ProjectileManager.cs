using System;
using Unity.Netcode;
using Unity.VisualScripting;
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
    private bool _isBigBomb = false;
    bool _isBurstFiring = false;
    private int _burstCount = 1;
    private int _currentBurstNumber;
    private float _lastLaunchTime;
    [SerializeField] float _timeBetweenBurstFire = 0.25f;
    bool _isVariablePower = false;
    [SerializeField] float _variablePowerAmount = 0.5f;
    private float _variablePowerAmountPerShotOfBurst;
    private float _currentVariablePowerAmount;

    // events
    public event EventHandler OnProjectileLaunched;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _explosionMaskParent = GameObject.Find("ExplosionMasks").transform;
        _brokenWindowParent = GameObject.Find("BrokenWindows").transform;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (_isBurstFiring)
        {
            CheckBurstFire();
        }
    }

    [Rpc(SendTo.Server)]
    public void SetLatestPowerAndAngleValuesRpc(int playerId, float power, float angle)
    {
        _latestPowerValue = power;
        _latestAngleValue = angle;

        HidePlayerTrajectoryLineRpc(playerId);
    }

    public void StartLaunchProjectileRpc()
    {
        StartLaunchProjectileRpc(_latestPowerValue, _latestAngleValue);
    }

    [Rpc(SendTo.Server)]
    public void StartLaunchProjectileRpc(float power, float angle)
    {
        _currentPlayerId = GameManager.Instance.CurrentPlayerId.Value;
        _projectileLaunchPoint = PlayerManager.Instance.Players[_currentPlayerId].PlayerController.ProjectileLaunchPoint;

        PowerupManager.Instance.EnableDisableAllPlayerPowerupButtonsRpc(_currentPlayerId, false, false);

        ShowPlayerTrajectoryLineRpc(_currentPlayerId, false);

        ProjectileLaunchedClientsAndHostRpc();

        if (_burstCount == 1)
        {
            LaunchProjectile(power, angle);
            EndLaunchProjectileRpc();
            return;
        }

        _currentBurstNumber = 0;
        _isBurstFiring = true;
    }

    private void LaunchProjectile(float power, float angle)
    {
        // set animation and return to idle
        PlayerManager.Instance.SetPlayerAnimation(_currentPlayerId, "Throw");
        AudioManager.Instance.PlayAudioClipRpc(AudioClipType.ThrowSFX, 0.95f, 1.05f);

        if (_isBurstFiring)
            StartCoroutine(PlayerManager.Instance.ResetAnimation(_currentPlayerId, _timeBetweenBurstFire - 0.05f));
        else
            StartCoroutine(PlayerManager.Instance.ResetAnimation(_currentPlayerId, _delayBeforeAttackAnimationReset));

        GameObject projectile = Instantiate(_projectilePrefab, _projectileLaunchPoint.position, Quaternion.identity);
        projectile.GetComponent<NetworkObject>().Spawn(true);

        if (_isVariablePower)
        {
            power += _currentVariablePowerAmount;
            _currentVariablePowerAmount -= _variablePowerAmountPerShotOfBurst;
        }

        float angleRad = Mathf.Deg2Rad * angle;
        Vector2 force = new(
            _defaultForceMultiplier * power * Mathf.Cos(angleRad),
            _defaultForceMultiplier * power * Mathf.Sin(angleRad)
        );
        force.x *= PlayerManager.Instance.Players[_currentPlayerId].ThrowDirection;
        projectile.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
        IProjectile iProjectile = projectile.GetComponent<IProjectile>();
        iProjectile.SetProjectileParents(_explosionMaskParent, _brokenWindowParent);

        if (_isBigBomb)
        {
            iProjectile.SetExplosionSizeMultiplier(2f);
            _isBigBomb = false;
            PowerupManager.Instance.RemovePowerupUseRpc(_currentPlayerId, "Powerup_BigBomb");
        }

        if (_isBurstFiring)
        {
            // if this is the first projectile launched we can follow it
            if (_currentBurstNumber == 1)
                CameraManager.Instance.UpdateCameraForProjectileRpc();

            iProjectile.SetProjectileNumber(_currentBurstNumber);

            if (_currentBurstNumber == _burstCount)
            {
                iProjectile.SetLastProjectileInBurstRpc();
                if (_isVariablePower)
                    PowerupManager.Instance.RemovePowerupUseRpc(_currentPlayerId, "Powerup_TripleBombVariablePower");
                else
                    PowerupManager.Instance.RemovePowerupUseRpc(_currentPlayerId, "Powerup_TripleBomb");
            }
        }
        else
        {
            CameraManager.Instance.UpdateCameraForProjectileRpc();
            iProjectile.SetLastProjectileInBurstRpc();
        }

        _lastLaunchTime = Time.time;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndLaunchProjectileRpc()
    {
        _isBurstFiring = false;
        _burstCount = 1;
        _currentBurstNumber = 0;
        _isVariablePower = false;
        _currentVariablePowerAmount = 0f;

        GameManager.Instance.UpdateGameState(GameState.WaitingForDetonation);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ProjectileLaunchedClientsAndHostRpc()
    {
        OnProjectileLaunched?.Invoke(this, EventArgs.Empty);
    }

    #region Powerup functions
    [Rpc(SendTo.Server)]
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

    [Rpc(SendTo.Server)]
    public void SetBigBombRpc(bool enabled)
    {
        _isBigBomb = enabled;
    }

    [Rpc(SendTo.Server)]
    public void SetProjectileBurstRpc(int number)
    {
        _burstCount = number;
    }

    private void CheckBurstFire()
    {
        if (_currentBurstNumber == _burstCount)
        {
            EndLaunchProjectileRpc();
            return;
        }

        if (Time.time >= _lastLaunchTime + _timeBetweenBurstFire)
        {
            _currentBurstNumber++;
            LaunchProjectile(_latestPowerValue, _latestAngleValue);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetVariablePowerRpc()
    {
        _isVariablePower = true;
        _variablePowerAmountPerShotOfBurst = (_burstCount - 1) / 2f * _variablePowerAmount;
        _currentVariablePowerAmount = _variablePowerAmountPerShotOfBurst;
    }

    [Rpc(SendTo.Server)]
    public void ResetVariablePowerRpc()
    {
        _isVariablePower = false;
    }
    #endregion
}
