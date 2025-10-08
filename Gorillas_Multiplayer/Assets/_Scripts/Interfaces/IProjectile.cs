using UnityEngine;

public interface IProjectile
{
    public void SetProjectileParents(Transform explosionMaskParent, Transform brokenWindowParent);
    public void SetExplosionSizeMultiplier(float multiplier);
    public void SetLastProjectileInBurstRpc();
    public void SetProjectileNumber(int number);
}
