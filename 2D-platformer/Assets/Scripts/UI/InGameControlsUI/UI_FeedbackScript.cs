using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FeedbackScript : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isVisible;
    public float fadeOutTimer;
    public float fadeOutDelay;

    Text[] feedData = new Text[4];
    int currentFilledIndex;
    CanvasGroup UIRenderer;
    bool fadeOut;
    float lastTimeCalled;

    void Start()
    {
        isVisible = fadeOut = false;
        currentFilledIndex = 0;
        lastTimeCalled = Time.time;

        for (int i = 0; i < 4; i++)
        {
            feedData[i] = transform.GetChild(i).GetComponent<Text>();
            feedData[i].supportRichText = true;
        }
        clearDataStack();

        UIRenderer = GetComponent<CanvasGroup>();
        setCanvasGroup(false);
    }

    // Update is called once per frame
    void Update()
    {
        //Need to handle multiple reward messages slowly when registered rapidly
        if (Time.time - lastTimeCalled > fadeOutTimer) setCanvasGroup(false);

        if (fadeOut == true)
        {
            if (UIRenderer.alpha <= 0.0f)
            {
                fadeOut = false;
                clearDataStack();
            }
            else
            {
                UIRenderer.alpha -= Time.deltaTime / fadeOutDelay;
            }
        }
    }

    public void showKillFeedUI()
    {
        lastTimeCalled = Time.time;

        if (isVisible == false)
        {
            setCanvasGroup(true);
        }
        loadKillData();
    }

    void loadKillData()
    {
        //We fill up feedData[] array with relevant info before displaying it on the screen
    }

    void setCanvasGroup(bool state)
    {
        if (state == true)
        {
            isVisible = true;
            fadeOut = false;
            UIRenderer.alpha = 1.0f;
            UIRenderer.interactable = true;
            UIRenderer.blocksRaycasts = true;
        }
        else
        {
            isVisible = false;
            fadeOut = true;
            UIRenderer.interactable = false;
            UIRenderer.blocksRaycasts = false;
        }

    }

    void clearDataStack()
    {
        currentFilledIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            feedData[i].text = "";
        }
    }
}
