using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class WorldReverse : MonoBehaviour {
    public static WorldReverse Instance;

    public Slider worldReverseSlider;

    public bool isRewinding = false;

    public float recordTime = 5f;
    public float savedTime;

    public int memoryCount;

    private float normalizedSavedTime;
    [SerializeField] private float savedTimeUIUpdateDuration = 0.3f;


    // Use this for initialization
    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        if (worldReverseSlider == null) {
            Debug.LogError("worldReverseSlider is required. Drag and drop world reverse slider ui.");
        }
        savedTime = 0f;
        memoryCount = 0;
    }

    // Update is called once per frame
    void Update() {
        normalizedSavedTime = (float)savedTime / (float)recordTime;
        DOTween.To(() => worldReverseSlider.value, x => worldReverseSlider.value = x, normalizedSavedTime, savedTimeUIUpdateDuration);
        if (Input.GetKeyDown(KeyCode.T)) {
            StartRewind();
        }
        if (Input.GetKeyUp(KeyCode.T)) {
            StopRewind();
        }
    }

    void FixedUpdate() {
        if (isRewinding)
            Rewind();
        else
            Record();
    }

    void Rewind() {
        if (memoryCount > 0) {
            savedTime -= Time.fixedDeltaTime;
            memoryCount -= 1;
        } else {
            savedTime = 0f;
            StopRewind();
        }

    }

    void Record() {
        savedTime += Time.fixedDeltaTime;
        if (memoryCount > Mathf.Round(recordTime / Time.fixedDeltaTime)) {
            savedTime = recordTime;
            memoryCount -= 1;
        }

        memoryCount += 1;
    }

    public void StartRewind() {
        isRewinding = true;
    }

    public void StopRewind() {
        isRewinding = false;
    }
}
