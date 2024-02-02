using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementLimiter : MonoBehaviour
{
    public static MovementLimiter instance;

    [SerializeField] bool _initialPlayerCanMove = true;
    [SerializeField] bool _initialPlayerCanAttack = true;
    [SerializeField] bool _initialPlayerCanParkour = true;
    public bool playerCanMove, playerCanAttack, playerCanParkour;

    private void OnEnable()
    {
        instance = this;
    }

    private void Start()
    {
        playerCanMove = _initialPlayerCanMove;
        playerCanAttack = _initialPlayerCanAttack;
        playerCanParkour = _initialPlayerCanParkour;
    }
}
