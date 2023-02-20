using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShuriken : MonoBehaviour {
    [SerializeField] private float rotationSpeed = 450f;
    [SerializeField] private Transform shurikenSpriteTransform;
    [SerializeField] private float shurikenSpeed = 10f;
    [SerializeField] private float disappearDelay = 0.3f;

    [SerializeField] private float shurikenActiveTime;
    [SerializeField] private bool shouldStop = false;

    private Rigidbody2D shurikenRigidbody;

    public void Stop() {
        shouldStop = true;
    }

    private void Awake() {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.Instance.playerTransform.GetComponent<Collider2D>());
        shurikenRigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable() {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.Instance.playerTransform.GetComponent<Collider2D>());
        shurikenActiveTime = 0f;
        shouldStop = false;
    }

    private void Update() {
        shurikenActiveTime += Time.deltaTime;
        if (shurikenActiveTime > disappearDelay) {
            gameObject.SetActive(false);
        }
        if (!shouldStop) {
            shurikenSpriteTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        } else {
            shurikenRigidbody.velocity = Vector2.zero;
        }
    }

    public void Deploy(Vector2 direction) {
        shurikenRigidbody.velocity = direction * shurikenSpeed;
    }
}
