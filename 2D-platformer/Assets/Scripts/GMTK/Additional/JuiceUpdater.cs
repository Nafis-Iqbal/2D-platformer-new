using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerEffectUpdater : MonoBehaviour
{

    [SerializeField] PlayerAppearanceScript playerAppearanceScript;

    [SerializeField] ParticleSystem runParticles;
    [SerializeField] ParticleSystem jumpParticles;
    [SerializeField] ParticleSystem landParticles;

    [SerializeField] TrailRenderer playerTrail;

    [Header("Values")]
    [Range(0, 10)]
    public float runParticlesValue = 0;
    [Range(0, 20)]
    public float jumpParticlesValue = 0;
    [Range(0, 50)]
    public float landParticlesValue = 0;
    [Range(0, 10)]
    public float trailValue = 0;
    [Range(-20, 20)]
    public float tiltAngleValue = 0;
    [Range(0, 60)]
    public float tiltSpeedValue = 0;
    public bool jumpSFXToggleValue = false;
    public bool landSFXToggleValue = false;

    private void OnEnable()
    {
        changeRunParticles(runParticlesValue);
        changeJumpParticles(jumpParticlesValue);
        changeLandParticles(landParticlesValue);
        changeTrail(trailValue);
        changeTiltAmount(tiltAngleValue);
        changeTiltSpeed(tiltSpeedValue);
    }


    public void changeRunParticles(float amount)
    {
        var em = runParticles.emission;
        em.rateOverDistance = amount;
    }



    public void changeJumpParticles(float amount)
    {
        var em = jumpParticles.emission;
        ParticleSystem.Burst newBurst = new ParticleSystem.Burst(0, amount);
        em.SetBurst(0, newBurst);
    }

    public void changeLandParticles(float amount)
    {
        var em = landParticles.emission;
        ParticleSystem.Burst newBurst = new ParticleSystem.Burst(0, amount);
        em.SetBurst(0, newBurst);
    }

    public void changeTrail(float amount)
    {
        playerTrail.time = amount;
    }

    public void changeTiltAmount(float amount)
    {
        playerAppearanceScript.maxTilt = amount;
    }

    public void changeTiltSpeed(float amount)
    {
        playerAppearanceScript.tiltSpeed = amount;
    }
}
