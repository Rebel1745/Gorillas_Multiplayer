using UnityEngine;

public class Powerup_TripleBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetProjectileBurst(3);
            SetButtonColourRpc(_inUseColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", false);
        }
        else
        {
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetProjectileBurst(1);
            SetButtonColourRpc(_defaultColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower", true);
        }

    }
}
