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

    private void Update() {
        if (grappleActive) {
            grappleSpriteTransform.localScale = Vector3.Lerp(grappleSpriteTransform.localScale, grappleActiveScale, Time.deltaTime * grappleActiveScaleSpeed);
        } else {
            grappleSpriteTransform.localScale = Vector3.Lerp(grappleSpriteTransform.localScale, Vector3.one, Time.deltaTime * grappleActiveScaleSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            GrappleActivate();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            GrappleInactive();
        }
    }

    private void GrappleInactive() {
        grappleSpriteRenderer.color = grappleInactiveColor;
        grappleActive = false;
    }

    private void GrappleActivate() {
        grappleSpriteRenderer.color = grappleActiveColor;
        grappleActive = true;
    }
}
