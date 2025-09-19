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
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.CancelMovementPowerupPosition();
            //InputManager.Instance.EnableDisableGameplayControls(true);
            SetButtonColourRpc(_defaultColour);
        }
    }
}
