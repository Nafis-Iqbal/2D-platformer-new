using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsControllerBase : MonoBehaviour
{
    Animator propsAnimator;
    [SerializeField]
    int currentAnimationID;
    public bool hasInteractiveStates;

    public GameObjectStruct[] objectsToRotate = new GameObjectStruct[0];

    // Start is called before the first frame update
    void Start()
    {
        propsAnimator = GetComponent<Animator>();
        currentAnimationID = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (propsAnimator != null && hasInteractiveStates == false) propsAnimator.SetInteger("AnimationID", currentAnimationID);

        if (objectsToRotate.Length > 0)
        {
            for (int i = 0; i < objectsToRotate.Length; i++)
            {
                objectsToRotate[i].structGameObject.transform.Rotate(Vector3.forward, objectsToRotate[i].floatProperty);
            }
        }
    }

    public void switchCurrentAnimation(int newAnimationID)
    {
        currentAnimationID = newAnimationID;
    }

    public void triggerInteractLeft()
    {
        propsAnimator.SetTrigger("HitLeft");
    }

    public void triggerInteractRight()
    {
        propsAnimator.SetTrigger("HitRight");
    }
}
