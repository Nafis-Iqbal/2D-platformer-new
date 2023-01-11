using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            other.GetComponent<EnemyHealth>().takeDamage(10f);
        }
    }
}
