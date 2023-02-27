using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockDefense : MonoBehaviour
{
    private Animator playerAnimator;

    private void Awake() {
        playerAnimator = GameManager.Instance.playerAnimator;
    }

    
}
