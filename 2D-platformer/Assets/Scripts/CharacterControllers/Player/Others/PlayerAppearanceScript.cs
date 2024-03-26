using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

//This script handles purely aesthetic things like particles, squash & stretch, and tilt

public class PlayerAppearanceScript : MonoBehaviour
{
    [Header("Components")]
    private Animator playerSpineAnimator;
    PlayerMovement moveScript;
    PlayerJump jumpScript;

    [Header("Components - Particles")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;

    [Header("Tilting")]
    [SerializeField, Tooltip("How far should the character tilt?")] public float maxTilt;
    [SerializeField, Tooltip("How fast should the character tilt?")] public float tiltSpeed;

    [Header("Calculations")]
    public float moveSpeed;
    public float maxSpeed;

    [Header("Current State")]
    public bool playerGrounded;

    [Header("Platformer Toolkit Stuff")]
    public bool cameraFalling = false;

    private void Awake()
    {
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
    }

    void Start()
    {
        moveScript = GetComponent<PlayerMovement>();
        jumpScript = GetComponent<PlayerJump>();
    }

    void Update()
    {
        TiltPlayer();

        //We need to change the character's running animation to suit their current speed

    }

    private void TiltPlayer()
    {
        //See which direction the character is currently running towards, and tilt in that direction
        float directionToTilt = 0;
        if (moveScript.tempVelocity.x != 0)
        {
            directionToTilt = Mathf.Sign(moveScript.tempVelocity.x);
        }

        //Create a vector that the character will tilt towards
        Vector3 targetRotVector = new Vector3(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, directionToTilt)));
    }

    private void checkForLanding()
    {
        if (!playerGrounded && jumpScript.onGround)
        {
            //By checking for this, and then immediately setting playerGrounded to true, we only run this code once when the player hits the ground 
            playerGrounded = true;
            cameraFalling = false;

            //Play an animation, some particles, and a sound effect when the player lands
            //playerSpineAnimator.SetTrigger("Landed");
            landParticles.Play();

            AudioManager.Instance.PlayLandSFX();

            moveParticles.Play();
        }
        else if (playerGrounded && !jumpScript.onGround)
        {
            // Player has left the ground, so stop playing the running particles
            playerGrounded = false;
            moveParticles.Stop();
        }
    }

    public void jumpEffects()
    {
        //Play these effects when the player jumps, courtesy of jump script
        AudioManager.Instance.PlayJumpSFX();

        jumpParticles.Play();
    }

}
