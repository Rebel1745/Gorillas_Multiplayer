using UnityEngine;

public class Powerup_Shield : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        int playerId = GameManager.Instance.CurrentPlayerId.Value;

        //PlayerManager.Instance.Players[playerId].PlayerController.SetShieldForNextTurn(_powerupEnabled);

        if (_powerupEnabled) SetButtonColour(_inUseColour);
        else SetButtonColour(_defaultColour);
    }
}
