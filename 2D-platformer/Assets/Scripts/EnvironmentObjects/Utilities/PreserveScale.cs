using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveScale : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 tempScale = transform.localScale;
        tempScale = new Vector3(tempScale.x / transform.parent.localScale.x, tempScale.y / transform.parent.localScale.y, tempScale.z / transform.parent.localScale.z);
        transform.localScale = tempScale;
    }
}
