using UnityEngine;

public class Powerup_Shield : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        int playerId = GameManager.Instance.CurrentPlayerId.Value;

        //PlayerManager.Instance.Players[playerId].PlayerController.SetShieldForNextTurn(_powerupEnabled);

        // if the player is the active, don't change the colour so the player is surprised by the shield being activated when they launch their banana
        /*if (!PlayerManager.Instance.Players[playerId].IsCPU)
        {
            if (_powerupEnabled) _powerupButton.image.color = _inUseColour;
            else _powerupButton.image.color = _defaultColour;
        }*/
    }
}
