using UnityEngine;
using UnityEngine.UI;

public class Powerup_MovePlayer : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _inUseColour);
            GameManager.Instance.UpdateGameState(GameState.WaitingForMovement);
        }
        else
        {
            PlayerMovementManager.Instance.CancelMovementPowerupPositionRpc();
            PlayerInputManager.Instance.EnableDisableGameplayControls(true);
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _defaultColour);
        }
    }
}
