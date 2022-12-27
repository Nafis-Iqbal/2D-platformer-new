using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;


namespace GMTK.PlatformerToolkit {
    public class JumpTester : MonoBehaviour {
        [SerializeField] CinemachineVirtualCamera theCamera;



        public float characterY = -3.89f;
        [SerializeField] Transform characterTransform;

        [SerializeField] CharacterJump jumpScript;

        [SerializeField] float offset;

        public bool ignoringJump = false;

        [SerializeField] BoxCollider2D squareBox;
        [SerializeField] BoxCollider2D roundedBox;

        [SerializeField] GameObject squareBoxGO;
        [SerializeField] GameObject roundedBoxGO;

        [SerializeField] public AudioSource jumpSFX;
        [SerializeField] public AudioSource landSFX;
        public bool jumpSFXisOn;
        public bool landSFXisOn;


        public float runParticles;
        public float jumpParticles;
        public float landParticles;

        public bool shouldShowPresets = false;


        void Start() {

            if (ignoringJump) {
                ignoreCharacterJump();

            } else {
                followCharacterJump();
            }
        }
        [ContextMenu("Flip")]
        void flip() {
            if (ignoringJump) {
                ignoringJump = false;
                followCharacterJump();
            } else {
                ignoringJump = true;
                ignoreCharacterJump();

            }
        }

        void Update() {
            transform.position = new Vector3(characterTransform.position.x, characterY);
        }

        public void toggleFollowJump(bool turnOn) {
            if (!turnOn) {
                ignoringJump = false;
                followCharacterJump();
            } else {
                ignoringJump = true;
                ignoreCharacterJump();

            }
        }

        public void followCharacterJump() {
            theCamera.Follow = characterTransform;
        }

        public void ignoreCharacterJump() {
            theCamera.Follow = transform;
        }


        public void toggleJumpSFX(bool turnOn) {
            if (!turnOn) {
                jumpSFX.enabled = false;
            } else {
                jumpSFX.enabled = true;
            }
        }

        public void toggleLandSFX(bool turnOn) {
            if (!turnOn) {
                landSFX.enabled = false;
            } else {
                landSFX.enabled = true;
            }
        }
    }
}
