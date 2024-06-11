using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoDetectorObject : MonoBehaviour
{
    public bool isVehicle;
    public InteractionBase cargoHolderParentObject;
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
        if (other.CompareTag("Objects"))
        {
            Debug.Log("object is : " + other.gameObject);
            cargoHolderParentObject.OnUseObjectFunction1(true, other);
        }
        else if (other.CompareTag("Player"))
        {
            cargoHolderParentObject.OnUseObjectFunction2(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Objects"))
        {
            cargoHolderParentObject.OnUseObjectFunction1(false, other);
        }
        else if (other.CompareTag("Player"))
        {
            cargoHolderParentObject.OnUseObjectFunction2(false);
        }
    }
}
