using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBreaking : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player") || other.transform.CompareTag("Enemy"))
            Destroy(gameObject);
    }
}
