using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    public static CameraManager Instance { get; private set; }

    public List<Vector3> _cameraTargets = new();
    private Camera _camera;
    [SerializeField] private Vector3 _cameraOffset;
    private const float ADDITIONAL_Y_OFFSET = 0.5f; // just to add a little space above the banana trajectory
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 10f;
    private float _zoomTarget = 0f;
    private Vector3 _cameraTargetPosition = Vector3.zero;
    private Vector3 _moveVelocity;
    private float _zoomVelocity;
    [SerializeField] private float _zoomSmoothTime = 0.5f;
    [SerializeField] private float _moveSmoothTime = 0.5f;
    private Bounds _cameraBounds;
    private bool _moveCamera = false;
    private bool _instantCameraMovement;
    private float _screenHeightWidthRatio;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        ResetCameraRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ResetCameraRpc()
    {
        _camera = Camera.main;
        CalculateScreenHeightWidthRatio();

        _cameraTargets.Clear();

        // when we first start, don't move the camera, wait for both players to be loaded
        _moveCamera = false;
        // when the level loads, the camera movement should be instant
        _instantCameraMovement = true;
    }

    private void LateUpdate()
    {
        if (_camera == null) _camera = Camera.main;

        // if there are no targets, bail
        if (_cameraTargets.Count == 0) return;

        // if we are not supposed to move the camera, bail
        if (!_moveCamera) return;

        ZoomCamera(_zoomSmoothTime);
        MoveCamera(_moveSmoothTime);
        CheckIfCameraShouldStillMove();
    }

    private void CheckIfCameraShouldStillMove()
    {
        // if we reach our prefered zoom, stop moving the camera
        if (Mathf.Abs(_camera.orthographicSize - _zoomTarget) < 0.01f && Vector3.Distance(_cameraTargetPosition, transform.position) < 0.1f)
        {
            _moveCamera = false;
            _zoomVelocity = 0F;
            _moveVelocity = Vector3.zero;
        }
    }

    private void ZoomCamera(float smoothTime)
    {
        _zoomTarget = Mathf.Max(GetBoundsSize().x / 2f / _screenHeightWidthRatio, (GetBoundsSize().y + _cameraOffset.y) / 2.0f + ADDITIONAL_Y_OFFSET);
        _zoomTarget = Mathf.Clamp(_zoomTarget, _minZoom, _maxZoom);

        if (_cameraTargets.Count == 1) smoothTime *= 2;

        if (_instantCameraMovement)
        {
            _camera.orthographicSize = _zoomTarget;
            _instantCameraMovement = false;
        }
        else
        {
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _zoomTarget, ref _zoomVelocity, smoothTime);
        }
    }

    // moving the camera doesn't need a smooth time becuase it should remain in the same Y position
    private void MoveCamera(float smoothTime)
    {
        float camXPos = 0f;

        if (_cameraTargets.Count == 1) camXPos = GetCenterPoint().x;
        if (_cameraTargets.Count > 1) camXPos = (_cameraTargets[0].x + _cameraTargets[1].x) / 2f;

        _cameraTargetPosition = new(camXPos, _camera.orthographicSize + GetLowestPlayer() - _cameraOffset.y, _cameraOffset.z);

        _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, _cameraTargetPosition, ref _moveVelocity, smoothTime);
    }

    private Vector3 GetCenterPoint()
    {
        if (_cameraTargets.Count == 1) return _cameraTargets[0];

        return _cameraBounds.center;
    }

    private float GetLowestPlayer()
    {
        if (_cameraTargets.Count == 0) return 0f;
        if (_cameraTargets.Count == 1) return _cameraTargets[0].y;

        float _lowestTarget = Mathf.Infinity;

        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            if (_cameraTargets[i].y < _lowestTarget)
                _lowestTarget = _cameraTargets[i].y;
        }

        return _lowestTarget;
    }

    private Vector3 GetBoundsSize()
    {
        if (_cameraTargets.Count == 1) return _cameraTargets[0];

        // adjust the size to take into account the offset
        Vector3 ajustedSize = new(_cameraBounds.size.x + _cameraOffset.x / 2, _cameraBounds.size.y, _cameraBounds.size.z);

        //return _cameraBounds.size;
        return ajustedSize;
    }

    private void SetBounds()
    {
        CalculateScreenHeightWidthRatio();

        _cameraBounds = new Bounds(_cameraTargets.First(), Vector3.zero);
        for (int i = 0; i < _cameraTargets.Count; i++)
        {
            _cameraBounds.Encapsulate(_cameraTargets[i]);
        }
    }

    // function waits until two players are added to the scene to set the initial camera position and zoom
    [Rpc(SendTo.ClientsAndHost)]
    public void AddPlayerRpc(Vector3 target)
    {
        if (_cameraTargets.Contains(target)) return;

        _cameraTargets.Add(target);

        if (_cameraTargets.Count > 1)
        {
            SetBounds();
            _moveCamera = true;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RemovePlayerRpc(int playerId)
    {
        if (_cameraTargets.Count < 2) Debug.LogError("Both players aren't here, why are we trying to remove one?");
        _cameraTargets.RemoveAt(playerId);

        SetBounds();
        _moveCamera = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetProjectileZenithRpc(Vector3 target)
    {
        if (_cameraTargets.Count == 2)
            _cameraTargets.Add(target);
        else if (_cameraTargets.Count == 3)
            _cameraTargets[2] = target;
        else Debug.LogError("SetProjectileZenith() Why are we here?");
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateCameraForProjectileRpc()
    {
        // we now need to update the bounds with the zenith of the projectiles trajectory
        SetBounds();
        _moveCamera = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RemoveProjectileRpc()
    {
        // ensure we have 3 elements (2 players and a projectile)
        if (_cameraTargets.Count != 3)
        {
            Debug.Log($"We tried to remove a projetile when only {_cameraTargets.Count} are in the target list");
            return;
        }

        // the projectile is always the last element, remove it
        _cameraTargets.RemoveAt(_cameraTargets.Count - 1);

        SetBounds();
        _moveCamera = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    // used for when the player movement powerup is initiated and the camera needs to view possible movement positions
    public void UpdatePlayerPositionRpc(int playerId, Vector3 position)
    {
        _cameraTargets[playerId] = position;
        SetBounds();
        _moveCamera = true;
    }

    private void CalculateScreenHeightWidthRatio()
    {
        _screenHeightWidthRatio = (float)Screen.width / Screen.height;
    }
}
