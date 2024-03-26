using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public static ParticleController instance;
    [Header("RainParticles")]
    [SerializeField] private GameObject rainEffect;
    [SerializeField] private ParticleSystem rainBack;
    [SerializeField] private ParticleSystem rainMid;
    [SerializeField] private ParticleSystem rainFront;
    public float rainBackEmit = 60;
    public float rainMidEmit = 30;
    public float rainFrontEmit = 25;
    public bool startRain = false;
    [Header("Player Particles")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;

    [Header("Enemy Particles")]
    [SerializeField] private ParticleSystem moveEnemyParticles;
    [SerializeField] private ParticleSystem jumpEnemyParticles;
    [SerializeField] private ParticleSystem landEnemyParticles;

    [Header("Fog Effect")]
    [SerializeField] private GameObject fogEffect;
    public bool startFog = false;

    private void Awake()
    {
        //Makeing script singeleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (startRain == true)
        {
            rainEffect.SetActive(true);
        }
    }

    private void Update()
    {
        rainBack.Emit((int)(rainBackEmit));
        rainMid.Emit((int)(rainMidEmit));
        rainFront.Emit((int)(rainFrontEmit));
    }

    public void startRaining(bool startRain)
    {
        if (startRain == true)
        {
            rainEffect.SetActive(true);
        }
        else
        {
            rainEffect.SetActive(false);
        }
    }

    public void moveEnemyParti(bool running)
    {
        if (running)
        {
            moveEnemyParticles.Play();
        }
        else
        {
            moveEnemyParticles.Stop();
        }
    }

    public void Fog(bool startFog)
    {
        if (startFog == true)
        {
            fogEffect.SetActive(true);
        }
        else
        {
            fogEffect.SetActive(false);
        }
    }

}
