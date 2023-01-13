using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour {
    public float damageAmount = 5f;
    [SerializeField] private float projectileThrowForce = 10f;
    [SerializeField] private float projectileActiveTime = 0.8f;
    [SerializeField] private ParticleSystem projectileParticle;

    private Rigidbody2D projectileRigidbody;
    private Collider2D projectileCollider;
    private float originalGravityScale;

    public bool isGrounded;

    [Header("Collider Settings")]
    [SerializeField][Tooltip("Length of the ground-checking collider")] private float groundLength = 0.95f;
    [SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 colliderOffset;

    [Header("Layer Masks")]
    [SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask groundLayer;

    private void Update() {
        RaycastHit2D ground1 = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer);
        RaycastHit2D ground2 = Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
        isGrounded = ground1 || ground2;

        if (isGrounded) {
            StartCoroutine(DisableProjectile());
        }
    }

    private void OnDrawGizmos() {
        if (isGrounded) {
            Gizmos.color = Color.green;
        } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
    }

    private void Awake() {
        projectileRigidbody = GetComponent<Rigidbody2D>();
        originalGravityScale = projectileRigidbody.gravityScale;
        projectileCollider = GetComponent<Collider2D>();
        var main = projectileParticle.main;
        main.duration = projectileActiveTime;
    }

    public void Deploy(Vector2 direction) {
        var forceDirection = (Vector2.up + direction) * projectileThrowForce;
        projectileRigidbody.AddForce(forceDirection, ForceMode2D.Impulse);
    }

    public void Hit() {
        projectileParticle.Play();
    }

    IEnumerator DisableProjectile() {
        yield return new WaitForSeconds(projectileActiveTime);
        gameObject.SetActive(false);
    }
}
