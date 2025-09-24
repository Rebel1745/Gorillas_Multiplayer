using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Banana : NetworkBehaviour, IProjectile
{
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private LayerMask _whatIsPlayer;
    [SerializeField] private float _destroyWhenDistanceOffscreen = -20f;
    [SerializeField] private float _rotationRate = 1f;
    private Rigidbody2D _rb;
    private bool _isLastProjectile;
    private int _projectileNumber;

    // explosion stuff
    [SerializeField] private GameObject _explosionSpriteMask;
    private bool _createExplosionMask;
    private float _explosionRadius;
    private Transform _explosionTransform;
    [SerializeField] private AudioClip _explosionSFX;
    private float _explosionRadiusMultiplier = 1f;
    [SerializeField] private float _explosionRadiusDamageMultiplier = 2;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private LayerMask _whatIsWindow;
    [SerializeField] private GameObject[] _brokenWindowSprites;
    private Transform _brokenWindowParent;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _explosionRadius = _explosionSpriteMask.transform.localScale.x / 2;
    }

    void Update()
    {
        CheckForGroundHit();

        // if the banana goes too far offscreen, destroy it
        if (transform.position.y < _destroyWhenDistanceOffscreen)
        {
            Debug.Log("Banana: AutoDestroy");
            CreateExplosionAndDestroyRpc();
            if (_isLastProjectile)
                GameManager.Instance.UpdateGameState(GameState.NextTurn);
        }

        if (!_createExplosionMask && _rb.linearVelocityY < 0 && _projectileNumber == 1)
        {
            // if we are moving down, change the zoom
            CameraManager.Instance.SetProjectileZenithRpc(transform.position);
            CameraManager.Instance.UpdateCameraForProjectileRpc();
        }

        transform.Rotate(0, 0, -_rotationRate * Time.deltaTime);
    }

    private void CheckForGroundHit()
    {
        _createExplosionMask = true;
        int playerHitId, otherPlayerId;

        // check if we hit a player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsPlayer);
        Collider2D[] hits;

        if (hit)
        {
            Debug.Log("Direct hit");
            // if we hit the shield, bail
            if (hit.collider.gameObject.name == "Shield")
            {
                Debug.Log("HitShield");
                // don't destroy the shield unless it is the end of the round otherwise a triple shot would win despite the shield
                //hit.transform.GetComponentInParent<PlayerController>().HideShield();
                CreateExplosionAndDestroyRpc(false);
                if (_isLastProjectile)
                    GameManager.Instance.UpdateGameState(GameState.NextTurn, 1f);
                return;
            }

            playerHitId = hit.transform.GetComponent<PlayerController>().PlayerId;
            otherPlayerId = PlayerManager.Instance.GetOtherPlayerId(playerHitId);
            CreateExplosionAndDestroyRpc();
            CameraManager.Instance.RemovePlayerRpc(playerHitId);
            //GameManager.Instance.UpdateScore(otherPlayerId);
            PlayerManager.Instance.SetPlayerAnimation(otherPlayerId, "Celebrate");

            // we directly hit a player!!
            PlayerManager.Instance.DestroyPlayerRpc(playerHitId);

            // Game over?
            GameManager.Instance.UpdateGameState(GameState.RoundComplete);
        }
        else
        {
            hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, _whatIsGround);

            if (hit)
            {
                // we hit the ground, did the explosion hit a player?
                hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _whatIsPlayer);
                if (hits.Length > 0)
                {
                    foreach (var h in hits)
                    {
                        // if we hit the shield, bail
                        if (h.gameObject.name == "Shield")
                        {
                            Debug.Log("HitShield2");
                            //h.transform.GetComponentInParent<PlayerController>().HideShield();
                            CreateExplosionAndDestroyRpc(false);
                            if (_isLastProjectile)
                                GameManager.Instance.UpdateGameState(GameState.NextTurn, 1f);
                            return;
                        }
                    }
                    Debug.Log("Indirect hit");

                    playerHitId = hits[0].transform.GetComponent<PlayerController>().PlayerId;
                    otherPlayerId = PlayerManager.Instance.GetOtherPlayerId(playerHitId);
                    PlayerManager.Instance.SetPlayerAnimation(otherPlayerId, "Celebrate");

                    // the explosion hit a player!
                    CreateExplosionAndDestroyRpc();
                    CameraManager.Instance.RemovePlayerRpc(playerHitId);
                    PlayerManager.Instance.DestroyPlayerRpc(playerHitId);

                    GameManager.Instance.UpdateGameState(GameState.RoundComplete);
                }
                else
                {
                    // check to see if there are any explosion masks already at the hit point
                    foreach (var h in Physics2D.OverlapPointAll(transform.position))
                    {
                        // if there is, bail
                        if (h.CompareTag("ExplosionMask")) _createExplosionMask = false;
                    }

                    if (_createExplosionMask)
                    {
                        Debug.Log("Missed");
                        CreateExplosionAndDestroyRpc();

                        // Next Players turn
                        if (_isLastProjectile && GameManager.Instance.State == GameState.WaitingForDetonation)
                            GameManager.Instance.UpdateGameState(GameState.NextTurn, 1f);
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void CreateExplosionAndDestroyRpc(bool createMask = true)
    {
        if (createMask)
        {
            //Debug.Log($"Create Mask {explosionRadiusMultiplier}");
            // create the explosion crater with a mask
            GameObject exGO = Instantiate(_explosionSpriteMask, transform.position, Quaternion.identity);
            exGO.GetComponent<NetworkObject>().Spawn(true);
            exGO.GetComponent<NetworkObject>().TrySetParent(_explosionTransform);
            exGO.transform.localScale *= _explosionRadiusMultiplier;
        }

        // find all of the windows in the blast radius (with multiplier)
        foreach (var h in Physics2D.OverlapCircleAll(transform.position, _explosionRadius * _explosionRadiusMultiplier * _explosionRadiusDamageMultiplier, _whatIsWindow))
        {
            GameObject randomSprite = _brokenWindowSprites[Random.Range(0, _brokenWindowSprites.Length)];
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            GameObject windowGO = Instantiate(randomSprite, h.transform.position, randomRotation);
            windowGO.GetComponent<NetworkObject>().Spawn(true);
            windowGO.GetComponent<NetworkObject>().TrySetParent(_brokenWindowParent);
        }

        if (_isLastProjectile)
            CameraManager.Instance.RemoveProjectileRpc();

        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);
        //AudioManager.Instance.PlayAudioClip(_explosionSFX, 0.95f, 1.05f);
        //Debug.Log("CreateExplosionAndDestroyRpc");
        Destroy(gameObject);
    }

    public void SetLastProjectileInBurst()
    {
        _isLastProjectile = true;
    }

    public void SetProjectileNumber(int number)
    {
        _projectileNumber = number;
    }

    public void SetExplosionRadius(float radius)
    {
        _explosionRadius = radius;
    }

    public void SetProjectileParents(Transform explosionMaskParent, Transform brokenWindowParent)
    {
        _explosionTransform = explosionMaskParent;
        _brokenWindowParent = brokenWindowParent;
    }

    public void SetExplosionSizeMultiplier(float multiplier)
    {
        _explosionRadiusMultiplier = multiplier;
    }
}
