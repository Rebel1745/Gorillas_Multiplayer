using UnityEngine;

public class Powerup_TripleBombVariablePower : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(3);
            ProjectileManager.Instance.SetVariablePowerRpc();
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _inUseColour);
            PowerupManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb", false, true);
            PowerupManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb", false, true);
        }
        else
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(1);
            ProjectileManager.Instance.ResetVariablePowerRpc();
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _defaultColour);
            PowerupManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb", true, true);
            PowerupManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb", true, true);
        }
    }
}
