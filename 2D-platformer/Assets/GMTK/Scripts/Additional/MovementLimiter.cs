using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GMTK.PlatformerToolkit {
    public class MovementLimiter : MonoBehaviour {
        public static MovementLimiter instance;

        [SerializeField] bool _initialPlayerCanMove = true;
        public bool playerCanMove;

        private void OnEnable() {
            instance = this;
        }

        private void Start() {
            playerCanMove = _initialPlayerCanMove;
        }
    }
}
