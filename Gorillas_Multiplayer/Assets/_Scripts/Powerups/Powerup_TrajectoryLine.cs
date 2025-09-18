using UnityEngine;

public class Powerup_TrajectoryLine : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerTrajectoryLine.DrawTrajectoryLine();

        _powerupButton.image.color = _usedColour;

        // the trajectory line can't be turned on and off, only on once per throw so disable it straight after use
        EnableDisableButton(false);
    }
}
