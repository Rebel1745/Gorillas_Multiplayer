using UnityEngine;

public class Powerup_TrajectoryLine : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        ProjectileManager.Instance.ShowPlayerTrajectoryLineRpc(GameManager.Instance.CurrentPlayerId.Value, true);

        PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _usedColour);

        PowerupManager.Instance.RemovePowerupUseRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TrajectoryLine");

        // the trajectory line can't be turned on and off, only on once per throw so disable it straight after use
        EnableDisableButtonRpc(false, true);
    }
}
