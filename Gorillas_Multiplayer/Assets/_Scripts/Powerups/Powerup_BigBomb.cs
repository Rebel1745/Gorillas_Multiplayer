using UnityEngine;

public class Powerup_BigBomb : Powerup
{
    public override void UsePowerup()
    {
        base.UsePowerup();

        //PlayerManager.Instance.Players[GameManager.Instance.CurrentPlayerId.Value].PlayerController.SetBigBomb(_powerupEnabled);
        GameObject scatterBomb = PlayerManager.Instance.GetPlayerPowerup(GameManager.Instance.CurrentPlayerId.Value, "Powerup_TripleBombVariablePower");

        if (_powerupEnabled)
        {
            _powerupButton.image.color = _inUseColour;
            if (scatterBomb) scatterBomb.GetComponent<Powerup>().EnableDisableButton(false);
        }
        else
        {
            _powerupButton.image.color = _defaultColour;
            if (scatterBomb) scatterBomb.GetComponent<Powerup>().EnableDisableButton(true);
        }
    }
}
