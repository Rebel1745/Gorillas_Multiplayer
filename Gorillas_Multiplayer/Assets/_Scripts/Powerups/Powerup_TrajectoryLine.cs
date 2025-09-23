using UnityEngine;

public class Powerup_TrajectoryLine : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        ProjectileManager.Instance.ShowPlayerTrajectoryLineRpc(GameManager.Instance.CurrentPlayerId.Value, true);

        SetButtonColourRpc(_usedColour);

        // the trajectory line can't be turned on and off, only on once per throw so disable it straight after use
        EnableDisableButtonRpc(false);
    }
}
