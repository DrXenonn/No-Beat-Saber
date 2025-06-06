using System;
using UnityEngine;

public class Box : MonoBehaviour
{
    [SerializeField] private float MovementSpeed = 5f;
    [SerializeField] private ParticleSystem BreakParticle;

    private void Update()
    {
        transform.position += Vector3.left * (MovementSpeed * Time.deltaTime);

        if (transform.position.x < -4f)
            Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Sword")) return;

        BreakParticle.Play();
        BreakParticle.transform.parent = null;
        Destroy(gameObject);
    }
}