using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {
    Idle,
    Running,
    Walking,
    Jumping,
    Falling,
    Dashing,
    ColumnGrabbed,
    ColumnWalking,
    CanNotGrabColumn,
    CanGrabColumn
}
public class StateManager : MonoBehaviour {
    public static StateManager Instance;

    public static PlayerState CurrentPlayerState;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }
}
