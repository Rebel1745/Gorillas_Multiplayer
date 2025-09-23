using UnityEngine;

public interface IProjectile
{
    public void SetProjectileParents(Transform explosionMaskParent, Transform brokenWindowParent);
    public void SetExplosionSizeMultiplier(float multiplier);
    public void SetLastProjectileInBurst();
}
