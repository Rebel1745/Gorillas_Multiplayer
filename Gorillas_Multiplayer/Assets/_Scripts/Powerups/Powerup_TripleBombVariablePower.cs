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
            SetButtonColourRpc(_inUseColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb", false);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb", false);
        }
        else
        {
            ProjectileManager.Instance.SetProjectileBurstRpc(1);
            ProjectileManager.Instance.ResetVariablePowerRpc();
            SetButtonColourRpc(_defaultColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb", true);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb", true);
        }
    }
}
