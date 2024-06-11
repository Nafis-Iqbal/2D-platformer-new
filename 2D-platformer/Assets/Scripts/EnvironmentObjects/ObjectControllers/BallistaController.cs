using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallistaController : InteractionBase
{
    public bool reloading;
    public Transform ballistaBow, aimingGear, reloadGear, ballistaSpear, projectileSpawnPosition, spearBasePosition;
    public Collider2D[] collidersToIgnore = new Collider2D[3];
    public float reloadTime, gearRotationSpeed, inUseCameraZoomValue;
    float lastFiredTime;
    public float targetBowAngle, bowRotationSpeed;
    public float bowAngleUpperConstraint, bowAngleLowerConstraint;
    Quaternion targetRotationQuaternion;
    Vector2 aimDirectionVector, cachedAimDirectionVector;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        inUse = reloading = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (inUse)
        {
            aimDirectionVector = PlayerInputManager.Instance.mousePosition.normalized;
            if (Vector2.Angle(cachedAimDirectionVector, aimDirectionVector) > .01f)
            {
                playerSpineAnimator.SetBool("UsingObject", true);
                aimingGear.Rotate(new Vector3(0.0f, 0.0f, -1.0f), Time.deltaTime * gearRotationSpeed);
            }
            else
            {
                playerSpineAnimator.SetBool("UsingObject", false);
            }

            targetBowAngle = Vector2.SignedAngle(aimDirectionVector, Vector2.right);

            targetBowAngle = -targetBowAngle;

            if ((targetBowAngle > 0.0f && targetBowAngle < bowAngleUpperConstraint) || (targetBowAngle <= 0.0f && targetBowAngle > bowAngleLowerConstraint))
            {
                //ballistaBow.rotation = Quaternion.Euler(0.0f, 0.0f, targetBowAngle);
                targetRotationQuaternion = Quaternion.Euler(0.0f, 0.0f, targetBowAngle);
            }

            ballistaBow.transform.localRotation = Quaternion.RotateTowards(ballistaBow.transform.localRotation, targetRotationQuaternion, Time.deltaTime * bowRotationSpeed);
            //aimingGear.Rotate(new Vector3(0.0f, 0.0f, -1.0f), Time.deltaTime * gearRotationSpeed);

            cachedAimDirectionVector = aimDirectionVector;
            //Debug.Log("Angle: " + targetBowAngle);
        }

        if (reloading == true)
        {
            reloadGear.Rotate(Vector3.forward, Time.deltaTime * gearRotationSpeed);
            ballistaSpear.gameObject.SetActive(false);
        }
        else
        {
            ballistaSpear.gameObject.SetActive(true);
        }

        if (Time.time - lastFiredTime > reloadTime)
        {
            reloading = false;
        }
    }

    public override void OnInteract()
    {
        if (!inUse)
        {
            inUse = true;
            playerMovement.enterObjectControlState();

            playerSpineAnimator.SetInteger("InteractID", targetObjectID);
            playerSpineAnimator.SetBool("InteractB", true);
            PlayerInputManager.Instance.lookDataRequired = true;
            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction1.Enable();

            CameraController.Instance.objectInUseZoomValue = inUseCameraZoomValue;
            CameraController.Instance.wideAngleObjectInUse = true;
            //Enable Mouse/Joystick Input
        }
        else
        {
            inUse = false;
            playerMovement.exitObjectControlState();

            playerSpineAnimator.SetBool("InteractB", false);
            playerSpineAnimator.SetBool("UsingObject", false);
            PlayerInputManager.Instance.lookDataRequired = false;
            PlayerInputManager.Instance.playerInputActions.Player.UseObjectFunction1.Disable();

            CameraController.Instance.wideAngleObjectInUse = false;
            //Disable Joystick Input
        }
    }

    public override void OnUseObjectFunction1(bool useStatus)
    {
        if (reloading == false)
        {
            lastFiredTime = Time.time;
            reloading = true;

            var spearObject = ObjectPooler.Instance.SpawnFromPool("BallistaSpear", projectileSpawnPosition.position, Quaternion.identity);
            spearObject.GetComponent<LinearProjectile>().initializeProjectile(projectileSpawnPosition.position, (projectileSpawnPosition.position - spearBasePosition.position).normalized, true);
            spearObject.GetComponent<LinearProjectile>().forceMode = true;

            for (int i = 0; i < collidersToIgnore.Length; i++)
            {
                Physics2D.IgnoreCollision(spearObject.GetComponent<Collider2D>(), collidersToIgnore[i]);
            }

            float spearAngle = Vector2.SignedAngle(projectileSpawnPosition.position - spearBasePosition.position, Vector2.right);

            spearObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -spearAngle);
            spearObject.SetActive(true);
        }
    }
}
