using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteShapeAnchorScript : MonoBehaviour
{
    public bool followWorldTransformOffset;
    public bool isSpriteShapeFlipped;
    Vector3 initialPosition;
    public SpriteShapeController spriteShapeController;
    public bool movePointAlongX, movePointAlongY;
    public int splineAnchoredPointID;
    Vector2 initialAnchoringObjectPosition, initialSplineAnchoredPosition, initialSplineAnchoredLeftPosition, initialSplineAnchoredRightPosition,
    tempVectorCenter, tempVectorLeft, tempVectorRight;
    public float offsetY, offsetX;
    private Spline shapeSpline;

    // Start is called before the first frame update
    void Awake()
    {
        shapeSpline = spriteShapeController.spline;

        initialSplineAnchoredPosition = shapeSpline.GetPosition(splineAnchoredPointID);
        initialSplineAnchoredLeftPosition = shapeSpline.GetPosition(splineAnchoredPointID - 1);
        initialSplineAnchoredRightPosition = shapeSpline.GetPosition(splineAnchoredPointID + 1);

        initialAnchoringObjectPosition = transform.localPosition;

        if (followWorldTransformOffset)
        {
            initialPosition = transform.position;
        }
    }

    void Enable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (movePointAlongX == false && movePointAlongY == false) return;

        if (movePointAlongY)
        {
            if (followWorldTransformOffset)
            {
                offsetY = initialPosition.y - transform.position.y;
            }
            else
            {
                offsetY = initialAnchoringObjectPosition.y - transform.localPosition.y;
            }
        }
        if (movePointAlongX)
        {
            if (followWorldTransformOffset)
            {
                offsetX = initialPosition.x - transform.position.x;
            }
            else
            {
                offsetX = initialAnchoringObjectPosition.x - transform.localPosition.x;
            }
        }

        tempVectorCenter = initialSplineAnchoredPosition;
        tempVectorLeft = initialSplineAnchoredLeftPosition;
        tempVectorRight = initialSplineAnchoredRightPosition;

        if (isSpriteShapeFlipped)
        {
            tempVectorCenter.y = tempVectorCenter.y + offsetY;
            tempVectorCenter.x = tempVectorCenter.x + offsetX;

            tempVectorLeft.y = tempVectorLeft.y + (offsetY / 2.0f);
            tempVectorLeft.x = tempVectorLeft.x + (offsetX / 2.0f);

            tempVectorRight.y = tempVectorRight.y + (offsetY / 2.0f);
            tempVectorRight.x = tempVectorRight.x + (offsetX / 2.0f);

        }
        else
        {
            tempVectorCenter.y = tempVectorCenter.y - offsetY;
            tempVectorCenter.x = tempVectorCenter.x - offsetX;

            tempVectorLeft.y = tempVectorLeft.y - (offsetY / 2.0f);
            tempVectorLeft.x = tempVectorLeft.x - (offsetX / 2.0f);

            tempVectorRight.y = tempVectorRight.y - (offsetY / 2.0f);
            tempVectorRight.x = tempVectorRight.x - (offsetX / 2.0f);
        }

        shapeSpline.SetPosition(splineAnchoredPointID, tempVectorCenter);
        shapeSpline.SetPosition(splineAnchoredPointID - 1, tempVectorLeft);
        shapeSpline.SetPosition(splineAnchoredPointID + 1, tempVectorRight);
    }
}
