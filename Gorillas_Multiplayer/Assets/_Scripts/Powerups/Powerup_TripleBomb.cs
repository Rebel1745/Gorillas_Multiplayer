using UnityEngine;

public class Powerup_TripleBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(3);
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _inUseColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", false);
        }
        else
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(1);
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _defaultColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", true);
        }

    }
}
