using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maximumHealth = 100f;
    [SerializeField][Range(0, 1f)] protected float percentageStartHealth = 1f;
    [SerializeField] protected float damageOnCollision = 50f;
    protected float currentHealth = default;

    [Header("Visual FX")] [Space]
    [SerializeField] protected ParticleSystem explosionParticles = null;
    public Transform shipVisuals = null;
    [SerializeField] protected ParticleSystem hitParticles = null;

    public bool IsAlive => maximumHealth > 0f;

    protected virtual void Awake()
    {
        currentHealth = maximumHealth * percentageStartHealth;
    }

    protected virtual void DamageBy(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            SendMessage($"{nameof(OnDeath)}");
        }
        else
        {
            hitParticles?.Play();
        }
    }

    protected virtual void OnParticleCollision(GameObject other)
    {
        if (IsAlive)
        {
            print($"Particles sent by {other.name} collided with {name}!");

            ParticleCannon particleCannon = other.GetComponent<ParticleCannon>();
            if (particleCannon != null)
            {
                DamageBy(particleCannon.damageOnHit);
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        DamageBy(damageOnCollision);
    }

    protected virtual void OnDeath()
    {
        currentHealth = 0f;
        explosionParticles.transform.position = shipVisuals.position;
        shipVisuals.gameObject.SetActive(false);
        explosionParticles.Play();
        Destroy(gameObject, explosionParticles.main.duration);
    }
}
