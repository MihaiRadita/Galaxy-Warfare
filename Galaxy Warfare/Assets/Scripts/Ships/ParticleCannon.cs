using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCannon : MonoBehaviour
{
    private new ParticleSystem particleSystem = null;
    public float damageOnHit = 20f;

    public bool IsFirable { get; private set; } = true;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    public void Fire(float speed, Vector3 target)
    {
        if(IsFirable)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.startSpeed = speed;
            transform.LookAt(target);
            particleSystem.Play();
            StartCoroutine(FireCooldown());
        }
    }

    private IEnumerator FireCooldown()
    {
        IsFirable = false;
        yield return new WaitForSeconds(particleSystem.main.duration);
        IsFirable = true;
    }
}
