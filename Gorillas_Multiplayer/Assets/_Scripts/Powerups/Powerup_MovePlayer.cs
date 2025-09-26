using UnityEngine;
using UnityEngine.UI;

public class Powerup_MovePlayer : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            SetButtonColourRpc(_inUseColour);
            GameManager.Instance.UpdateGameState(GameState.WaitingForMovement);
        }
        else
        {
            PlayerMovementManager.Instance.ShowHideMovementPowerupIndicatorsRpc(GameManager.Instance.CurrentPlayerId.Value, false);
            //InputManager.Instance.EnableDisableGameplayControls(true);
            SetButtonColourRpc(_defaultColour);
        }
    }
}
