using Unity.Netcode;
using UnityEngine;

public class Powerup_BigBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        ProjectileManager.Instance.SetBigBombRpc(_powerupEnabled);

        if (_powerupEnabled)
        {
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", false);
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _inUseColour);
        }
        else
        {
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", true);
            PlayerInputManager.Instance.SetButtonColourRpc(_powerupButtonNO, _defaultColour);
        }
    }
}
