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
            OnCollisionEnter(null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
            return;

        if (collision != null && collision.transform.CompareTag(CoinTag))
        {
            collision.collider.enabled = false;
            collision.transform.GetComponent<Animation>().Play();
        }

        if (collision != null && (collision.transform.CompareTag(ObstacleTag) || collision.transform.CompareTag(BombTag)))
        {
            isDead = true;
            SetMaterial(onDie);
                
            FallGuysSetup.Instance.Remove(gameObject);
            particleEffectSystem.Play();

            if (collision.transform.CompareTag(BombTag))
            {
                GetComponent<Rigidbody>().AddForce((Vector3.back + Vector3.up) * explosionForce, ForceMode.Impulse);
                collision.transform.GetComponent<ParticleSystem>().Play();
            }
            return;
        }

        if (isGained)
            return;

        if (collision is null || collision.transform.CompareTag(PlayerTag))
        {
            isGained = true;
            SetMaterial(onSetup);
            particleEffectSystem.Play();
            FallGuysSetup.Instance.Add(gameObject);
        }
    }

    private void SetMaterial(Material mat)
    {
        var materials = skinnedRenderer.materials;
        materials[0] = mat;
        skinnedRenderer.materials = materials;
    }
}