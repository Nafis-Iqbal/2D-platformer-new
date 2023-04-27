using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TestEnemyAttack : MonoBehaviour {
    [SerializeField] private float attackDelay = 1f;
    private void Start() {
        transform.DOMove(new Vector3(transform.position.x, GameManager.Instance.playerTransform.position.y, 0f), attackDelay).SetLoops(-1, LoopType.Yoyo);

    }
}
