using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDenialTrapController : MonoBehaviour
{
    public float trapDamage = 20.0f;
    public bool hasMultipleSpawns;
    public Transform primaryRespawnPoint;
    public Transform leftRespawnPoint, rightRespawnPoint;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerCombatSystem>().TakeEnvironmentDamage(trapDamage);
            if (other.GetComponent<PlayerCombatSystem>().isDead == true) return;

            other.gameObject.SetActive(false);
            if (hasMultipleSpawns == false)
            {
                CameraController.Instance.triggerRespawnPlayerCameraTransition(primaryRespawnPoint.position);
            }
            else
            {
                float leftSpawnDistance = Vector3.Distance(leftRespawnPoint.position, other.transform.position);
                float rightSpawnDistance = Vector3.Distance(rightRespawnPoint.position, other.transform.position);

                if (leftSpawnDistance < rightSpawnDistance)
                {
                    CameraController.Instance.triggerRespawnPlayerCameraTransition(leftRespawnPoint.position);
                }
                else CameraController.Instance.triggerRespawnPlayerCameraTransition(rightRespawnPoint.position);
            }
        }
    }
}
