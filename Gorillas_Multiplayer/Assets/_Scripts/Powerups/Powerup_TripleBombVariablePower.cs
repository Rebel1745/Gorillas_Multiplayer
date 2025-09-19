using UnityEngine;

public class Powerup_TripleBombVariablePower : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        if (_powerupEnabled)
        {
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetProjectileBurst(3);
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetVariablePower();
            SetButtonColourRpc(_inUseColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb", false);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb", false);
        }
        else
        {
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetProjectileBurst(1);
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.ResetVariablePower();
            SetButtonColourRpc(_defaultColour);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb", true);
            PlayerManager.Instance.EnableDisablePowerupButtonRpc(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb", true);
        }
    }
}
