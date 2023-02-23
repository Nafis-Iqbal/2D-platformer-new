// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerWorldSlowDown : MonoBehaviour {

//     [Range(0f, 1f)]
//     public float slowDownFactor = 0.5f;

//     public bool isSlowMotionActive = false;

//     public float originalFixedDeltaTime;

//     private void Update() {
//         // slow motion logic here
//         if (Input.GetKeyDown(KeyCode.Z)) {
//             if (!isSlowMotionActive) {
//                 ActivateSlowMotion();
//             } else {
//                 DisableSlowMotion();
//             }
//         }
//     }

//     private void DisableSlowMotion() {
//         isSlowMotionActive = false;
//         Time.timeScale = 1f;
//         Time.fixedDeltaTime = originalFixedDeltaTime;
//     }

//     private void ActivateSlowMotion() {
//         isSlowMotionActive = true;
//         Time.timeScale = slowDownFactor;
//         originalFixedDeltaTime = Time.fixedDeltaTime;
//         // Time.fixedDeltaTime = Time.timeScale * 0.02f;
//     }
// }



using System.Collections;
using UnityEngine;

public class PlayerWorldSlowDown : MonoBehaviour {
    private const float DEFAULT_FIXED_DELTA_TIME = 0.02f;
    private const float SLOW_DOWN_RATE = 0.65f;
    private const float SPEED_UP_RATE = 1.5f;

    private Coroutine _timeChange;
    private float _targetTimeScale;
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            SlowDownTime();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            SpeedUpTime();
        }
    }

    private void SlowDownTime() {
        _targetTimeScale = 0.01f;
        if (_timeChange != null) {
            StopCoroutine(_timeChange);
        }
        _timeChange = StartCoroutine(MakeFixedTimeAgreeWithTimeScale());
    }

    private void SpeedUpTime() {
        _targetTimeScale = 1f;
        if (_timeChange != null) {
            StopCoroutine(_timeChange);
        }
        _timeChange = StartCoroutine(MakeFixedTimeAgreeWithTimeScale());
    }

    private IEnumerator MakeFixedTimeAgreeWithTimeScale() {
        while (Time.timeScale > _targetTimeScale && !Mathf.Approximately(Time.timeScale, _targetTimeScale)) {
            Time.timeScale = Mathf.Max(_targetTimeScale, Time.timeScale * SLOW_DOWN_RATE);
            Time.fixedDeltaTime = DEFAULT_FIXED_DELTA_TIME * Time.timeScale;
            Debug.Log($"Reducing TimeScale Time to {Time.timeScale} to match TimeScale of {Time.timeScale} at time {Time.unscaledTime}");
            yield return null;
        }

        while (Time.timeScale < _targetTimeScale && !Mathf.Approximately(Time.timeScale, _targetTimeScale)) {
            Time.timeScale = Mathf.Min(_targetTimeScale, Time.timeScale * SPEED_UP_RATE);
            Time.fixedDeltaTime = DEFAULT_FIXED_DELTA_TIME * Time.timeScale;
            Debug.Log($"Increasing TimeScale to {Time.timeScale} to match TimeScale of {Time.timeScale} at time {Time.unscaledTime}");
            yield return null;
        }
    }
}
