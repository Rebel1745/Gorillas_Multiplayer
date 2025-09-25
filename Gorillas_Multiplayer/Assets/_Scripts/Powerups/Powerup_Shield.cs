using UnityEngine;

public class Powerup_Shield : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        int playerId = GameManager.Instance.CurrentPlayerId.Value;

        ShieldManager.Instance.SetShieldForNextTurnRpc(playerId, _powerupEnabled);

        if (_powerupEnabled) SetButtonColour(_inUseColour);
        else SetButtonColour(_defaultColour);
    }
}
