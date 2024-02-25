using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherAnimationEventHandler : MonoBehaviour
{
    public EnemyArcher enemyArcherScript;

    public void ArmedWeaponsArrowShot()
    {
        Debug.Log("Event fired");
        enemyArcherScript.performArrowShot();
    }

    public void ArmedWeaponsPowerShot()
    {
        Debug.Log("Event fired");
        enemyArcherScript.performPowerShot();
    }

    public void ArmedWeaponsArcShot()
    {
        Debug.Log("Event fired");
        enemyArcherScript.performArcShot();
    }
}
