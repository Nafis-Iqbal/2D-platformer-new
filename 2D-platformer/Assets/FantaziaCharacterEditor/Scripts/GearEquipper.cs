using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;


public class GearEquipper : MonoBehaviour {
    //Script responsible for equipping the gear on the character
    //It must be attached to the parent of the character SkeletionAnimation

    [Header("Need only one reference")]
    public SkeletonAnimation skeletonAnimation;
    public SkeletonMecanim skeletonMecanim;
    public Jobs Job;
    public int Melee;
    public int Shield;
    public int Bow;
    public int Quiver;
    public int Staff;
    public int DuelistOffhand;
    public int Armor;
    public int Helmet;
    public int Shoulder;
    public int Arm;
    public int Feet;
    public int Hair;
    public int Face;
    public bool IsAutoUpdate;

    private void GetCorrectReference() {
        if (transform.GetChild(0).GetComponent<SkeletonAnimation>() != null) {
            Debug.Log("set anim");
            skeletonAnimation = transform.GetChild(0).GetComponent<SkeletonAnimation>();
        } else if (transform.GetChild(0).GetComponent<SkeletonMecanim>() != null) {
            Debug.Log("set meca");
            skeletonMecanim = transform.GetChild(0).GetComponent<SkeletonMecanim>();
        }
    }


    private void Start() {
        GetCorrectReference();
        ApplySkinChanges();
    }

    public void GenerateRandomGear() {
        ApplySkinChanges();
    }

    //Applies the chosen gear on the character
    public void ApplySkinChanges() {
        GetCorrectReference();

        if (skeletonAnimation == null && skeletonMecanim == null) {
            Debug.LogError("Check that the this GameObject's 1st child has SkeletonAnimation or SkeletonMecanim");
            return;
        }

        //Gets the skeleton and its data from the character gameobject
        Spine.Skeleton skeleton = null;
        if (skeletonAnimation != null) {
            skeleton = skeletonAnimation.Skeleton;
        } else if (skeletonMecanim != null) {
            skeleton = skeletonMecanim.Skeleton;
        }
        var skeletonData = skeleton.Data;

        //Creates a new custom skin
        var NewCustomSkin = new Skin("CustomCharacter");


        //Combines the skins based on the gear choices
        if (Job == Jobs.Warrior) {
            NewCustomSkin.AddSkin(skeletonData.FindSkin("MELEE " + Melee.ToString()));
            if (Shield == 0) {
                NewCustomSkin.AddSkin(skeletonData.FindSkin("EMPTY"));

            } else {
                NewCustomSkin.AddSkin(skeletonData.FindSkin("SHIELD " + (Shield - 1).ToString()));
            }
        } else if (Job == Jobs.Archer) {
            NewCustomSkin.AddSkin(skeletonData.FindSkin("BOW " + Bow.ToString()));
            NewCustomSkin.AddSkin(skeletonData.FindSkin("QUIVER " + Quiver.ToString()));
        } else if (Job == Jobs.Elementalist) {
            NewCustomSkin.AddSkin(skeletonData.FindSkin("STAFF " + Staff.ToString()));
        } else if (Job == Jobs.Duelist) {
            NewCustomSkin.AddSkin(skeletonData.FindSkin("OFFHAND " + DuelistOffhand.ToString()));
            NewCustomSkin.AddSkin(skeletonData.FindSkin("MELEE " + Melee.ToString()));
        }

        NewCustomSkin.AddSkin(skeletonData.FindSkin("ARMOR " + Armor.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("ARMOR " + Armor.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("ARMOR " + Armor.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("ARMOR " + Armor.ToString()));
        if (Helmet == 0) {
            NewCustomSkin.AddSkin(skeletonData.FindSkin("EMPTY"));

        } else {
            NewCustomSkin.AddSkin(skeletonData.FindSkin("HELMET " + Helmet.ToString()));
        }
        NewCustomSkin.AddSkin(skeletonData.FindSkin("SHOULDER " + Shoulder.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("ARM " + Arm.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("FEET " + Feet.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("HAIR " + Hair.ToString()));
        NewCustomSkin.AddSkin(skeletonData.FindSkin("EYES " + Face.ToString()));

        //Sets the new created skin on the character SkeletonAnimation
        skeleton.SetSkin(NewCustomSkin);
        skeleton.SetSlotsToSetupPose();
    }

}
public class GearInfo {
    public string SkinName;
    public int MaximumNumber;
    public int CurrentChosen;

    public GearInfo(string skinName, int maximumNumber, int currentChosen) {
        SkinName = skinName;
        MaximumNumber = maximumNumber;
        CurrentChosen = currentChosen;
    }
}
