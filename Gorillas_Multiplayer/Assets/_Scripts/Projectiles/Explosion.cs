using Unity.Netcode;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private float _cameraShakeDuration = 0.1f;
    [SerializeField] private float _cameraShakeAmount = 1f;
    void Start()
    {
        CameraShake.Instance.Shake(_cameraShakeDuration, _cameraShakeAmount);
        DestroyExplosionRpc();
    }

    [Rpc(SendTo.Server)]
    private void DestroyExplosionRpc()
    {
        Destroy(gameObject, 2f);
    }
}
