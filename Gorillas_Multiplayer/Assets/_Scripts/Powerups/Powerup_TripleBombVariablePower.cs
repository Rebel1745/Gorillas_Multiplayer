using UnityEngine;

public class Powerup_TripleBombVariablePower : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        GameObject tripleBomb = PlayerManager.Instance.GetPlayerPowerup(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBomb");
        GameObject bigBomb = PlayerManager.Instance.GetPlayerPowerup(GameManager.Instance.CurrentPlayerId.Value, "Powerup_BigBomb");

        if (_powerupEnabled)
        {
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetProjectileBurst(3);
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetVariablePower();
            _powerupButton.image.color = _inUseColour;
            if (tripleBomb) tripleBomb.GetComponent<Powerup>().EnableDisableButton(false);
            if (bigBomb) bigBomb.GetComponent<Powerup>().EnableDisableButton(false);
        }
        else
        {
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetProjectileBurst(1);
            //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.ResetVariablePower();
            _powerupButton.image.color = _defaultColour;
            if (tripleBomb) tripleBomb.GetComponent<Powerup>().EnableDisableButton(true);
            if (bigBomb) bigBomb.GetComponent<Powerup>().EnableDisableButton(true);
        }
    }
}
