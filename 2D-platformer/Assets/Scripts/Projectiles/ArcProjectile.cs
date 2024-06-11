﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

public class ArcProjectile : ProjectileWeapon
{
    Vector2 tempVelocity;
    public float projectileLaunchAngle, projectileLaunchVelocity;
    public bool lookAtTarget;
    public bool rotateDuringFlight;
    public float rotateSpeed;
    private float arrowOrientationAngle;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        float timeElapsed = Time.time - spawnTime;

        //ARC Trajectory
        tempVelocity.x = projectileLaunchVelocity * Mathf.Cos(projectileLaunchAngle);
        //tempVelocity.x = projectileLaunchVelocity * Mathf.Cos(projectileLaunchAngle) * 1.5f;//Don't know why multiplying with 1.5 makes it work
        tempVelocity.y = (projectileLaunchVelocity * Mathf.Sin(projectileLaunchAngle)) - (9.8f * timeElapsed);
        rb2d.velocity = tempVelocity;

        if (lookAtTarget == true && projectileHit == false)
        {
            arrowOrientationAngle = Mathf.Atan2(tempVelocity.y, tempVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(arrowOrientationAngle, Vector3.forward);
        }
        else if (rotateDuringFlight && lookAtTarget == false && projectileHit == false)
        {
            transform.Rotate(Vector3.forward, rotateSpeed);
        }
    }

    public override void initializeProjectile(Vector3 startPosition, Vector3 aimPosition, float projectileMaxHeight, float projectileFlightTime, GameObject projectileOwner, bool shooterFacingRight = true, bool playerProjectile = false)
    {
        base.initializeProjectile(startPosition, aimPosition, projectileMaxHeight, projectileFlightTime, projectileOwner, shooterFacingRight, playerProjectile);

        float flightPathDistance = Vector3.Distance(startPosition, aimPosition);
        if (shooterFacingRight == false) flightPathDistance = -flightPathDistance;

        float sqrtValue = Mathf.Sqrt(2.0f * 9.8f * projectileMaxHeight);

        //V = sqrt(2gh)/sin(r) AND S = vcos(r) * T
        projectileLaunchAngle = Mathf.Atan((sqrtValue * projectileFlightTime) / flightPathDistance);
        projectileLaunchVelocity = flightPathDistance / (projectileFlightTime * Mathf.Cos(projectileLaunchAngle));
        //Debug.Log("dist " + flightPathDistance + " ini vel: " + projectileLaunchVelocity + " time: " + projectileFlightTime);
    }
}