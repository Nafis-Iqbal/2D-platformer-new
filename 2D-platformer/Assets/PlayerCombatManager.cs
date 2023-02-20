using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour {
    public static float SwordDamage;
    public static float ShurikenDamage;
    public static float ProjectileDamage;

    [SerializeField] private float swordDamage;
    [SerializeField] private float shurikenDamage;
    [SerializeField] private float projectileDamage;

    private void Update() {
        SwordDamage = swordDamage;
        ShurikenDamage = shurikenDamage;
        ProjectileDamage = projectileDamage;
    }
}
