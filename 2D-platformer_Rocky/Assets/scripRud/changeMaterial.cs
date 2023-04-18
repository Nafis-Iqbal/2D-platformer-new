using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeMaterial : MonoBehaviour
{
    public Material[] materials;
    Renderer rend;
    public ParticleSystem ps;
    public healthofPlayer playerScript;
    void Start()
    {
        playerScript = EnemyManager.Instance.player.GetComponent<healthofPlayer>();
        ps = GetComponent<ParticleSystem>();
        
         rend = GetComponent<Renderer>();
         rend.enabled = true;
        // rend.sharedMaterial = materials[0];
    }

    private void Update()
    {
        var main = ps.main;
        main.startColor = playerScript.platColor;
        //rend.material.SetColor("_BaseColor", playerScript.platColor);
    }


}
