using Unity.Netcode;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private float _cameraShakeDuration = 0.1f;
    [SerializeField] private float _cameraShakeAmount = 1f;
    [SerializeField] private float _destroyAfterTime = 2f;
    private float _startTime;
    private bool _destroying = false;

    void Start()
    {
        _startTime = Time.time;
        CameraShake.Instance.Shake(_cameraShakeDuration, _cameraShakeAmount);
    }

    private void Update()
    {
        if (Time.time > _startTime + _destroyAfterTime && !_destroying)
            DestroyExplosionRpc();
    }

    [Rpc(SendTo.Server)]
    private void DestroyExplosionRpc()
    {
        _destroying = true;
        //Destroy(gameObject, 2f);
        gameObject.GetComponent<NetworkObject>().Despawn(true);
    }
}
