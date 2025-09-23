using UnityEngine;

public class Powerup_BigBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        //PlayerManager.Instance.SetBigBombRpc(GameManager.Instance.CurrentPlayerId.Value, _powerupEnabled);

        if (_powerupEnabled)
        {
            SetButtonColourRpc(_inUseColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", false);
        }
        else
        {
            SetButtonColourRpc(_defaultColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", true);
        }
    }
}
