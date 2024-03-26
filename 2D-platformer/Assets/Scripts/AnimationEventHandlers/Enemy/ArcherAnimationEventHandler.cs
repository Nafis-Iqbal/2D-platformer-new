using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherAnimationEventHandler : MonoBehaviour
{
    public EnemyArcher enemyArcherScript;

    public void ArmedWeaponsArrowNormalShot()
    {
        Debug.Log("Event fired");
        enemyArcherScript.performArrowShot();
    }

    public void ArmedWeaponsArrowPowerShot()
    {
        Debug.Log("Event fired");
        enemyArcherScript.performPowerShot();
    }

    public void ArmedWeaponsArrowArcShot()
    {
        Debug.Log("Event fired");
        enemyArcherScript.performArcShot();
    }
}
