using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectScript : MonoBehaviour
{
    bool playHitEffectOnScale;
    public float charScaleShrinkSpeed = .25f;
    public float shrinkSizeMultiplier = .95f;
    float initialCharacterYScale;
    bool charScaleDecreasing;
    // Start is called before the first frame update
    void OnEnable()
    {
        charScaleDecreasing = false;
        initialCharacterYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (playHitEffectOnScale)
        {
            Vector3 tempScale = transform.localScale;

            if (charScaleDecreasing)
            {
                if ((tempScale.y - Time.deltaTime * charScaleShrinkSpeed) > initialCharacterYScale * shrinkSizeMultiplier)
                {
                    tempScale.y = tempScale.y - (Time.deltaTime * charScaleShrinkSpeed);
                }
                else
                {
                    charScaleDecreasing = false;
                    tempScale.y = initialCharacterYScale * shrinkSizeMultiplier;
                }
            }
            else
            {
                if ((tempScale.y + Time.deltaTime * charScaleShrinkSpeed) < initialCharacterYScale)
                {
                    tempScale.y = tempScale.y + (Time.deltaTime * charScaleShrinkSpeed);
                }
                else
                {
                    tempScale.y = initialCharacterYScale;
                    charScaleDecreasing = true;
                    playHitEffectOnScale = false;
                }
            }
            transform.localScale = tempScale;
        }
    }

    public void PlayOnHitEffect()
    {
        playHitEffectOnScale = true;
        charScaleDecreasing = true;
    }
}
