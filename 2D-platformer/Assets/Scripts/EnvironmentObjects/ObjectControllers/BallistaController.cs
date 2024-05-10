using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallistaController : InteractionBase
{
    public Transform ballistaBow, aimingGear, reloadGear;
    public bool inUse, reloading;
    public float reloadTime, gearRotationSpeed;
    float lastFiredTime;
    public float targetBowAngle, bowRotationSpeed;
    Quaternion targetRotationQuaternion;
    public float bowAngleUpperConstraint, bowAngleLowerConstraint;
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
            playerSpineAnimator.SetInteger("InteractID", targetObjectID);
            playerSpineAnimator.SetBool("InteractB", true);
            PlayerInputManager.Instance.lookDataRequired = true;
            //Enable Mouse/Joystick Input
        }
        else
        {
            inUse = false;
            playerSpineAnimator.SetBool("InteractB", false);
            playerSpineAnimator.SetBool("UsingObject", false);
            PlayerInputManager.Instance.lookDataRequired = false;
            //Disable Joystick Input
        }
    }
}
