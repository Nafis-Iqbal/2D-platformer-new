using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShurikenAttack : MonoBehaviour {
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator playerSpineAnimator;
    [SerializeField] private float timeElapsedSinceAttack = 0f;
    [HideInInspector] public float attackCooldownTime = 1f;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private GameObject shurikenPrefab;

    private PlayerColumn playerColumn;

    private void Awake() {
        playerColumn = GameManager.Instance.playerTransform.GetComponent<PlayerColumn>();
    }

    private void Update() {
        if (isAttacking) {
            timeElapsedSinceAttack += Time.deltaTime;
            if (timeElapsedSinceAttack > attackCooldownTime) {
                timeElapsedSinceAttack = 0f;
                isAttacking = false;
            }
        }
    }

    public void OnShurikenAttack(InputAction.CallbackContext context) {
        if (!isAttacking && !playerColumn.hasGrabbedColumn) {
            playerAnimator.Play("shuriken throw");
            playerSpineAnimator.Play("shuriken throw");
            var shuriken = ObjectPooler.Instance.SpawnFromPool("PlayerShuriken", transform.position, Quaternion.identity);
            shuriken.GetComponent<PlayerShuriken>().Deploy(Vector2.right * transform.localScale.x);
        }
    }
}
