using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleCollider : MonoBehaviour {
    [SerializeField] private SpriteRenderer grappleSpriteRenderer;
    [SerializeField] private Transform grappleSpriteTransform;
    [SerializeField] private Color grappleActiveColor;
    [SerializeField] private Color grappleInactiveColor;
    [SerializeField] private bool grappleActive;
    [SerializeField] private Vector3 grappleActiveScale;
    [SerializeField] private float grappleActiveScaleSpeed;

    [SerializeField] private PlayerGrapplingGun playerGrapplingGun;

    // private void Awake() {
    //     playerGrapplingGun = GameManager.Instance.playerTransform.GetComponent<PlayerGrapplingGun>();
    // }

    private void Update() {
        if (grappleActive) {
            grappleSpriteTransform.localScale = Vector3.Lerp(grappleSpriteTransform.localScale, grappleActiveScale, Time.deltaTime * grappleActiveScaleSpeed);
        } else {
            grappleSpriteTransform.localScale = Vector3.Lerp(grappleSpriteTransform.localScale, Vector3.one, Time.deltaTime * grappleActiveScaleSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            GrappleActivate();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            GrappleInactive();
        }
    }

    private void GrappleInactive() {
        grappleSpriteRenderer.color = grappleInactiveColor;
        grappleActive = false;
        playerGrapplingGun.canGrapple = false;
    }

    private void GrappleActivate() {
        grappleSpriteRenderer.color = grappleActiveColor;
        grappleActive = true;
        playerGrapplingGun.UpdateGrappleTargetPosition(transform.position);
        playerGrapplingGun.canGrapple = true;
    }
}
