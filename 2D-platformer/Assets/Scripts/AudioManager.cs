using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    public AudioSource jumpSFX;
    public AudioSource landSFX;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    public void PlayJumpSFX() {
        if (!jumpSFX.isPlaying) {
            jumpSFX.Play();
        }
    }

    public void PlayLandSFX() {
        if (!landSFX.isPlaying) {
            landSFX.Play();
        }
    }

}
