using UnityEngine;

internal class FallGuy: MonoBehaviour
{
    private const string CoinTag = "Coin";
    private const string ObstacleTag = "Obstacle";
    private const string BombTag = "Bomb";
    private const string PlayerTag = "Player";

    [SerializeField]
    private float explosionForce = 10;
    [SerializeField]
    private Material onSetup;
    [SerializeField]
    private Material onDie;
    [SerializeField]
    private bool autoSetup;

    private bool isGained;
    private bool isDead;

    private SkinnedMeshRenderer skinnedRenderer;
    private ParticleSystem particleEffectSystem;

    private void Awake()
    {
        skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        particleEffectSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (autoSetup)
            GainFallGuyIfNot();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
            return;
        switch (collision.transform.tag)
        {
            case CoinTag:
                CollideWithCoin(collision);
                break;
            case ObstacleTag:
                Die();
                break;
            case BombTag:
                CollideWithBomb(collision);
                break;
            case PlayerTag:
                GainFallGuyIfNot();
                break;
        }
    }

    private void CollideWithCoin(Collision coinCollision)
    {
        coinCollision.collider.enabled = false;
        coinCollision.transform.GetComponent<Animation>().Play();
    }

    private void CollideWithBomb(Collision obstacleCollision)
    {
        Die();
        GetComponent<Rigidbody>().AddForce((Vector3.back + Vector3.up) * explosionForce, ForceMode.Impulse);
        obstacleCollision.transform.GetComponent<ParticleSystem>().Play();
    }

    private void Die()
    {
        isDead = true;
        SetMaterial(onDie);
                
        FallGuysSetup.Instance.Remove(gameObject);
        particleEffectSystem.Play();
    }

    private void GainFallGuyIfNot()
    {
        if (isGained)
            return;
    
        isGained = true;
        SetMaterial(onSetup);
        particleEffectSystem.Play();
        FallGuysSetup.Instance.Add(gameObject);
    }

    private void SetMaterial(Material mat)
    {
        var materials = skinnedRenderer.materials;
        materials[0] = mat;
        skinnedRenderer.materials = materials;
    }
}