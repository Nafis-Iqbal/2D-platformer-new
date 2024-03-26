using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearProjectile : ProjectileWeapon
{
    [SerializeField] private Transform projectileSpriteTransform;

    [Header("Projectile Properties")]
    public bool rotatingProjectile;
    private float rotationSpeed = 450f;
    private float horizontalSpeed = 10f;

    protected override void OnEnable()
    {
        base.OnEnable();

        rotationSpeed = projectileInfo.rotationSpeed;
        horizontalSpeed = projectileInfo.horizontalSpeed;
    }

    protected override void Update()
    {
        base.Update();

        if (!projectileHit)
        {
            rb2d.velocity = projectileDirection * horizontalSpeed;
            if (rotatingProjectile) projectileSpriteTransform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
        else
        {
            projectileSpriteTransform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
    }
}
