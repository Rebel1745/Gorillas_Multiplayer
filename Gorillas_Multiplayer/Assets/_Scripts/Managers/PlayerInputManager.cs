using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInputManager : NetworkBehaviour
{
    public static PlayerInputManager Instance { get; private set; }
    private PlayerInput _inputActions;
    public Button _currentPowerupButton;
    private bool _enableControls;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnCurrentPlayerIdChanged += GameManager_OnCurrentPlayerIdChanged;
    }

    private void OnEnable()
    {
        _inputActions = new PlayerInput();

        _inputActions.Gameplay.Power.started += UpdatePower;
        _inputActions.Gameplay.Power.canceled += UpdatePower;

        _inputActions.Gameplay.Angle.started += UpdateAngle;
        _inputActions.Gameplay.Angle.canceled += UpdateAngle;

        _inputActions.Gameplay.LaunchProjectile.started += LaunchProjectile;

        _inputActions.MovementPowerup.Direction.started += MovementPowerupDirection;
        _inputActions.MovementPowerup.Confirm.started += MovementPowerupConfirm;
        _inputActions.MovementPowerup.Cancel.started += MovementPowerupCancel;
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Power.started -= UpdatePower;
        _inputActions.Gameplay.Power.canceled -= UpdatePower;

        _inputActions.Gameplay.Angle.started -= UpdateAngle;
        _inputActions.Gameplay.Angle.canceled -= UpdateAngle;

        _inputActions.Gameplay.LaunchProjectile.started -= LaunchProjectile;

        _inputActions.MovementPowerup.Direction.started -= MovementPowerupDirection;
        _inputActions.MovementPowerup.Confirm.started -= MovementPowerupConfirm;
        _inputActions.MovementPowerup.Cancel.started -= MovementPowerupCancel;
    }

    private void GameManager_OnCurrentPlayerIdChanged(object sender, EventArgs e)
    {
        if ((int)NetworkManager.Singleton.LocalClientId == GameManager.Instance.CurrentPlayerId.Value)
            _enableControls = true;
        else _enableControls = false;

        _currentPowerupButton = null;
    }

    private void UpdatePower(InputAction.CallbackContext context)
    {
        if (!_enableControls) return;
        //if (GameManager.Instance.State != GameState.WaitingForLaunch || PlayerManager.Instance.IsCurrentPlayerCPU) return;

        // if (context.canceled)
        //     PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StopPowerChange();
        // else
        //     PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartPowerChange(context.ReadValue<float>());
    }

    private void UpdateAngle(InputAction.CallbackContext context)
    {
        if (!_enableControls) return;
        //if (GameManager.Instance.State != GameState.WaitingForLaunch || PlayerManager.Instance.IsCurrentPlayerCPU) return;

        // if (context.canceled)
        //     PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StopAngleChange();
        // else
        //     PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.StartAngleChange(context.ReadValue<float>());
    }

    private void LaunchProjectile(InputAction.CallbackContext context)
    {
        if (!_enableControls) return;
        //if (GameManager.Instance.State != GameState.WaitingForLaunch || PlayerManager.Instance.IsCurrentPlayerCPU) return;

        ProjectileManager.Instance.StartLaunchProjectileRpc();
    }

    private void MovementPowerupConfirm(InputAction.CallbackContext context)
    {
        if (!_enableControls) return;
        PlayerMovementManager.Instance.ConfirmMovementPowerupPositionRpc();
    }

    private void MovementPowerupDirection(InputAction.CallbackContext context)
    {
        if (!_enableControls) return;
        //PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.MovePlayerMovementSpriteWithInput(context.ReadValue<float>());
    }

    private void MovementPowerupCancel(InputAction.CallbackContext context)
    {
        if (!_enableControls) return;
        //PlayerManager.Instance.Players[PlayerManager.Instance.CurrentPlayerId].PlayerController.CancelMovementPowerupPosition();
        EnableDisableGameplayControls(true);
    }

    [Rpc(SendTo.Server)]
    public void SetCurrentPowerupButtonRpc(NetworkObjectReference buttonOR)
    {
        if (!buttonOR.TryGet(out NetworkObject buttonNO))
        {
            Debug.Log("Error: Could not retrieve NetworkObject");
            return;
        }
        _currentPowerupButton = buttonNO.gameObject.GetComponent<Button>();
        SetCurrentPowerupButtonClientsRpc(buttonOR);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetCurrentPowerupButtonClientsRpc(NetworkObjectReference buttonOR)
    {
        if (!buttonOR.TryGet(out NetworkObject buttonNO))
        {
            Debug.Log("Error: Could not retrieve NetworkObject");
            return;
        }
        _currentPowerupButton = buttonNO.gameObject.GetComponent<Button>();
    }

    [Rpc(SendTo.Server)]
    public void NullCurrentPowerupButtonRpc()
    {
        _currentPowerupButton = null;
        NullCurrentPowerupButtonClientsRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void NullCurrentPowerupButtonClientsRpc()
    {
        _currentPowerupButton = null;
    }

    [Rpc(SendTo.Server)]
    public void SetButtonColourRpc(NetworkObjectReference buttonOR, Color colour)
    {
        if (!buttonOR.TryGet(out NetworkObject buttonNO))
        {
            Debug.Log("Error: Could not retrieve NetworkObject");
            return;
        }
        _currentPowerupButton = buttonNO.gameObject.GetComponent<Button>();
        _currentPowerupButton.GetComponent<Powerup>().SetButtonColourRpc(colour);
    }

    public void EnableDisableUIControls(bool enabled)
    {
        if (!_enableControls) return;
        if (enabled)
        {
            _inputActions.Gameplay.Disable();
            _inputActions.MovementPowerup.Disable();
            _inputActions.UI.Enable();
        }
        else
            _inputActions.UI.Disable();
    }

    public void EnableDisableGameplayControls(bool enabled)
    {
        if (!_enableControls) return;
        if (enabled)
        {
            _inputActions.UI.Disable();
            _inputActions.MovementPowerup.Disable();
            _inputActions.Gameplay.Enable();
        }
        else
            _inputActions.Gameplay.Disable();
    }

    public void EnableDisableMovementPowerupControls(bool enabled)
    {
        if (!_enableControls) return;
        if (enabled)
        {
            _inputActions.UI.Disable();
            _inputActions.Gameplay.Disable();
            _inputActions.MovementPowerup.Enable();
        }
        else
            _inputActions.MovementPowerup.Disable();
    }
}
