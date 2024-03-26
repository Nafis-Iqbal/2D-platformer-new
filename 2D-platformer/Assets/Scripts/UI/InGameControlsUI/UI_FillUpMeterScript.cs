using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FillUpMeterScript : MonoBehaviour
{
    CanvasGroup UIRenderer;

    public int targetSliderID;
    public GameObject[] fillUpMeterObjects = new GameObject[2];
    public Slider[] fillUpMeterSliders = new Slider[2];//0 for bar......1 for circle   
    public Image[] objectiveStateImages = new Image[2];//0 for bar......1 for circle 
    public Image[] fillImages = new Image[2];
    public Image[] backgroundImages = new Image[2];

    public Color enemyColor, alliedColor, neutralColor;

    // Start is called before the first frame update
    void Start()
    {
        UIRenderer = GetComponent<CanvasGroup>();

        fillImages[0].color = alliedColor;
        fillImages[1].color = alliedColor;

        fillUpMeterObjects[0].SetActive(false);
        fillUpMeterObjects[1].SetActive(false);

        fillUpMeterObjects[targetSliderID].SetActive(true);
        setFillUpMeter(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateSliders(float totalShares, float currentBlueShares, float currentRedShares, bool objectiveNeutral, bool objectiveUnderBlue = false)
    {

        if (objectiveNeutral == true)
        {
            objectiveStateImages[targetSliderID].color = neutralColor;
        }
        else
        {
            //blue is allied color
            if (objectiveUnderBlue == true) objectiveStateImages[targetSliderID].color = alliedColor;
            else objectiveStateImages[targetSliderID].color = enemyColor;
        }

        //Debug.Log("val: " + currentBlueShares +" . " +totalShares);
        fillUpMeterSliders[targetSliderID].value = currentBlueShares / totalShares;

        if (currentRedShares == 0) backgroundImages[targetSliderID].color = neutralColor;
        else backgroundImages[targetSliderID].color = enemyColor;
    }

    public void setFillUpMeter(bool setActive)
    {
        if (setActive == true) UIRenderer.alpha = 1.0f;
        else UIRenderer.alpha = 0.0f;
    }
}
