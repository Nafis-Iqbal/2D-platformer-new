using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantTrapAnimationEventHandler : MonoBehaviour
{
    public PlantTrapController plantTrapScript;
    public PlantBudController plantBudScript;
    public void TrapPlayer()
    {
        plantTrapScript.trapPlayer = true;
    }

    public void OnProjectileShot()
    {
        plantBudScript.launchProjectile();
    }
}
