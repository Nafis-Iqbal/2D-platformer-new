using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantBudController : MonoBehaviour
{
    public bool isActive;
    public bool tracksPlayerMovement;

    public Animator budAnimator;
    public Transform projectileSpawnPosition, budBasePosition;

    public float trackingRotationSpeed;
    public float shotTimeInterval;
    public float offsetAngle;
    public float upperAngleConstraint, lowerAngleConstraint;
    public float lastShotTime, aimCorrectionAngle;

    Vector2 launchDirection, playerDirection;
    Quaternion targetRotation;
    Transform playerTransform;

    void OnEnable()
    {
        isActive = false;
        lastShotTime = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameManager.Instance.playerTransform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        playerTransform = GameManager.Instance.playerTransform;
        if (tracksPlayerMovement)
        {
            playerDirection = playerTransform.position - budBasePosition.position;
            launchDirection = projectileSpawnPosition.position - budBasePosition.position;

            aimCorrectionAngle = Vector2.SignedAngle(playerDirection, Vector2.right);

            if ((aimCorrectionAngle > 0.0f && aimCorrectionAngle < upperAngleConstraint) || (aimCorrectionAngle <= 0.0f && aimCorrectionAngle > lowerAngleConstraint))
            {
                aimCorrectionAngle += offsetAngle;
                aimCorrectionAngle = -aimCorrectionAngle;
                targetRotation = Quaternion.Euler(0.0f, 0.0f, aimCorrectionAngle);
            }
            // aimCorrectionAngle += offsetAngle;
            // aimCorrectionAngle = -aimCorrectionAngle;
            //targetRotation = Quaternion.Euler(0.0f, 0.0f, aimCorrectionAngle);

            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.deltaTime * trackingRotationSpeed);
        }

        if (Time.time - lastShotTime > shotTimeInterval)
        {
            budAnimator.SetTrigger("Shoot");
            lastShotTime = Time.time;
        }
    }

    public void launchProjectile()
    {
        launchDirection = projectileSpawnPosition.position - budBasePosition.position;

        var projectileObject = ObjectPooler.Instance.SpawnFromPool("PlantBudProjectile", projectileSpawnPosition.position, Quaternion.identity);
        projectileObject.GetComponent<LinearProjectile>().initializeProjectile(projectileSpawnPosition.position, launchDirection.normalized);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isActive = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isActive = false;
        }
    }
}
