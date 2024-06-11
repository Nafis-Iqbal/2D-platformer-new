using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    public static WeaponsManager Instance;
    [Header("Melee Weapons")]
    public WeaponClassInfo[] meleeWeaponsData = new WeaponClassInfo[3];
    [Header("Projectiles")]
    public WeaponClassInfo[] projectilesData = new WeaponClassInfo[3];
    [Header("Area Denial Weapons")]
    public WeaponClassInfo[] areaDenialWeaponsData = new WeaponClassInfo[3];

    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public WeaponClassInfo getProjectileWeaponData(string weaponName)
    {
        for (int i = 0; i < projectilesData.Length; i++)
        {
            if (projectilesData[i].weaponName == weaponName)
            {
                return projectilesData[i];
            }
        }

        return null;
    }

    public WeaponClassInfo getMeleeWeaponsData(string weaponName)
    {
        for (int i = 0; i < meleeWeaponsData.Length; i++)
        {
            if (meleeWeaponsData[i].weaponName == weaponName)
            {
                return meleeWeaponsData[i];
            }
        }

        return null;
    }

    public WeaponClassInfo getAreaDenialWeaponsData(string weaponName)
    {
        for (int i = 0; i < areaDenialWeaponsData.Length; i++)
        {
            if (areaDenialWeaponsData[i].weaponName == weaponName)
            {
                return areaDenialWeaponsData[i];
            }
        }

        return null;
    }
}
