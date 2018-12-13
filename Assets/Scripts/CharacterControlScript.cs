using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControlScript : CharMain {

    void Awake() {
    }

    protected override void Update() {

        if (GamepadMap.ButtonB) {
            CharacterCrouch();            
        }

        if (GamepadMap.ButtonBack) {
            ResetCharacter();
        }

        LeftStickAxisAndRightTrigger();

        RightStickAxis();
        Bumpers();

        base.Update();
    }

    /// <summary>
    /// Camera rotation.
    /// </summary>
    private void RightStickAxis() {

        if (GamepadMap.RightStickAxisX != 0 || GamepadMap.RightStickAxisY != 0) {
            MoveCamera(GamepadMap.RightStickAxisX, GamepadMap.RightStickAxisY);
        }
    }

    /// <summary>
    /// Camera zoom.
    /// </summary>
    private void Bumpers() {

        if (GamepadMap.LeftBumper) {
            UpdateCameraDistance(-1.0f);
        } else if (GamepadMap.RightBumper) {
            UpdateCameraDistance(1.0f);
        }
    }

    /// <summary>
    /// Player movement.
    /// </summary>
    private void LeftStickAxisAndRightTrigger() {

        //Debug.Log(GamepadMap.LeftTrigger);
        MoveCharacter(GamepadMap.LeftStickAxisX, GamepadMap.LeftStickAxisY, GamepadMap.RightTrigger, GamepadMap.LeftTrigger);
    }


}
