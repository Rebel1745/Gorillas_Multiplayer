using UnityEngine;

public class Powerup_TripleBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(3);
            SetButtonColourRpc(_inUseColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", false);
        }
        else
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(1);
            SetButtonColourRpc(_defaultColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", true);
        }

    }
}
