using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameNotifications : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isVisible;
    public bool isSpecialMessage;
    public float fadeOutTimer;
    public float fadeOutDelay;

    bool fadeOutMode;
    CanvasGroup UIRenderer;
    bool fadeOut;
    float lastTimeCalled, lastSpecialMessageUsed;
    Text displayText;
    public int currentMessagePriority;

    void Start()
    {
        isVisible = fadeOut = isSpecialMessage = false;
        fadeOutMode = true;
        lastTimeCalled = Time.time;
        lastSpecialMessageUsed = -1.0f;
        currentMessagePriority = 0;

        UIRenderer = GetComponent<CanvasGroup>();
        displayText = transform.GetChild(0).GetComponent<Text>();
        setCanvasGroup(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeOutMode == true && Time.time - lastTimeCalled > fadeOutTimer) setCanvasGroup(false);

        //Turning off static messages after 30 secs in case of unhandled bugs
        if (isSpecialMessage == true && Time.time - lastSpecialMessageUsed > 30.0f)
        {
            lastSpecialMessageUsed = Time.time;
            isSpecialMessage = false;
            setCanvasGroup(false, true);
        }

        if (fadeOutMode == true && fadeOut == true)
        {
            if (UIRenderer.alpha <= 0.0f)
            {
                fadeOut = false;
            }
            else
            {
                UIRenderer.alpha -= Time.deltaTime / fadeOutDelay;
            }
        }
    }

    public void showMessage(string message)
    {
        if (isSpecialMessage == true) return;
        if (isVisible == false)
        {
            setCanvasGroup(true);
        }
        displayText.text = message;
        lastTimeCalled = Time.time;
    }

    public void showStaticMessage(bool setVisibility, string message = "")
    {
        if (setVisibility == true)
        {
            isSpecialMessage = true;
            lastSpecialMessageUsed = Time.time;
            fadeOutMode = false;
            setCanvasGroup(true);
            displayText.text = message;
        }
        else
        {
            isSpecialMessage = false;
            setCanvasGroup(false, true);
            fadeOutMode = true;
        }
    }

    //Called only when player enters/exits capture point.....SOME message should be cached when this method called always  
    public void showCachedMessageOnEnteringObjective(bool setVisibility, int teamID)
    {
        if (setVisibility == true)
        {
            isSpecialMessage = true;
            lastSpecialMessageUsed = Time.time;
            fadeOutMode = false;
            setCanvasGroup(true);
        }
        else
        {
            isSpecialMessage = false;
            setCanvasGroup(false, true);
            fadeOutMode = true;
        }
    }

    public void updateCachedMessage(string message, int priority)
    {
        if (priority >= currentMessagePriority)
        {
            displayText.text = message;
            currentMessagePriority = priority;
        }
    }

    void setCanvasGroup(bool state, bool nrmMessage = false)
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
            UIRenderer.interactable = false;
            UIRenderer.blocksRaycasts = false;
            if (nrmMessage == true)
            {
                UIRenderer.alpha = 0.0f;
                return;
            }
            isVisible = false;
            fadeOut = true;
        }

    }
}

