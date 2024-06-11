using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearProjectile : ProjectileWeapon
{
    [SerializeField] private Transform projectileSpriteTransform;

    [Header("Projectile Properties")]
    public bool rotatingProjectile;
    private float rotationSpeed = 450f;
    public float horizontalSpeed = 10f;
    public float appliedForce;
    public bool forceMode = false;
    public bool forceAlreadyApplied;
    public bool lookAtTarget;
    Vector2 tempVelocity;
    float arrowOrientationAngle;

    protected override void OnEnable()
    {
        base.OnEnable();

        rotationSpeed = projectileInfo.rotationSpeed;
        horizontalSpeed = projectileInfo.horizontalSpeed;
        forceAlreadyApplied = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!projectileHit && forceMode == false)
        {
            rb2d.velocity = projectileDirection * horizontalSpeed;
            if (rotatingProjectile) projectileSpriteTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
        else if (forceMode == true && forceAlreadyApplied == false)
        {
            forceAlreadyApplied = true;
            rb2d.AddForce(appliedForce * projectileDirection, ForceMode2D.Impulse);
        }

        tempVelocity = rb2d.velocity;

        if (lookAtTarget == true && projectileHit == false)
        {
            arrowOrientationAngle = Mathf.Atan2(tempVelocity.y, tempVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(arrowOrientationAngle, Vector3.forward);
        }
    }
}
