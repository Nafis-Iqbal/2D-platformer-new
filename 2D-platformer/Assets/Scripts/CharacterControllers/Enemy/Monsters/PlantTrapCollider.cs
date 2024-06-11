using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantTrapCollider : MonoBehaviour
{
    public PlantTrapController plantTrapScript;
    public Rigidbody2D plantTrapRB2D;
    // Start is called before the first frame update
    void OnEnable()
    {
        plantTrapRB2D = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            plantTrapScript.isObjectOnTop = true;
            plantTrapScript.objectOnTop = other.gameObject;
            plantTrapScript.objectInitialLocalDistance = Vector2.Distance(other.transform.position, plantTrapScript.trapLengthRefPoint.position);
            plantTrapScript.plantTrapAnimator.SetTrigger("ObjectDrop");
            //other.transform.GetComponent<PlayerMovement>().setParent(transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            plantTrapScript.isObjectOnTop = false;
            plantTrapScript.objectOnTop = null;
            plantTrapScript.objectInitialLocalDistance = 0.0f;
            plantTrapScript.plantTrapAnimator.SetTrigger("ObjectBounce");
            //other.transform.GetComponent<PlayerMovement>().resetParent();
        }
    }
}
