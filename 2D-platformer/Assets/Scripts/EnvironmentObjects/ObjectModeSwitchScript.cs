using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectModeSwitchScript : MonoBehaviour
{
    public bool isPlayerDependentObject;
    // Start is called before the first frame update
    public int objectCount;
    public bool fadeIn, fadeOut, fadingInProgress;

    public float fadingSpeed;

    SpriteRenderer objectSpriteRenderer;

    void OnEnable()
    {
        objectCount = 0;
        fadeIn = fadeOut = fadingInProgress = false;
        objectSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (fadingInProgress == true) return;

        if (fadeOut == true)
        {
            StartCoroutine("FadeOut");

        }
        if (fadeIn == true)
        {
            StartCoroutine("FadeIn");

        }
    }

    private IEnumerator FadeOut()
    {
        float alphaVal = objectSpriteRenderer.color.a;
        Color tmp = objectSpriteRenderer.color;
        fadingInProgress = true;

        while (objectSpriteRenderer.color.a > 0)
        {
            alphaVal -= 0.01f * fadingSpeed;
            tmp.a = alphaVal;
            objectSpriteRenderer.color = tmp;

            yield return new WaitForSeconds(0.05f); // update interval
        }
        fadeIn = fadingInProgress = false;
    }

    private IEnumerator FadeIn()
    {
        float alphaVal = objectSpriteRenderer.color.a;
        Color tmp = objectSpriteRenderer.color;
        fadingInProgress = true;

        while (objectSpriteRenderer.color.a < 1)
        {
            alphaVal += 0.01f * fadingSpeed;
            tmp.a = alphaVal;
            objectSpriteRenderer.color = tmp;

            yield return new WaitForSeconds(0.05f); // update interval
        }
        fadeOut = fadingInProgress = false;
    }

    private void OnTriggerEnter2D(Collider2D collidingObject)
    {
        if (collidingObject.transform.CompareTag("Player") || collidingObject.transform.CompareTag("Enemy"))
        {

        }
    }

    private void OnTriggerExit2D(Collider2D collidingObject)
    {
        if (collidingObject.transform.CompareTag("Player") || collidingObject.transform.CompareTag("Enemy"))
        {

        }
    }
}
