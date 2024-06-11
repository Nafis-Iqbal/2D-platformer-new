using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalToWorldTranslate : MonoBehaviour
{
    public Vector3 outputOffset;
    Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        outputOffset = transform.position - initialPosition;
    }
}
