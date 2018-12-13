using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamepadMap {

    public static float RightStickAxisX { get { return Input.GetAxis("Right Stick X"); } }
    public static float RightStickAxisY { get { return Input.GetAxis("Right Stick Y"); } }
    public static float LeftStickAxisX { get { return Input.GetAxis("Horizontal"); } }
    public static float LeftStickAxisY { get { return Input.GetAxis("Vertical"); } }    
    public static float LeftTrigger { get { return Input.GetAxis("Left Trigger"); } }
    public static float RightTrigger { get { return Input.GetAxis("Right Trigger"); } }
    public static float DPadAxisX { get { return Input.GetAxis("D-Pad X Axis"); } }
    public static float DPadAxisY { get { return Input.GetAxis("D-Pad Y Axis"); } }

    public static bool ButtonA { get { return Input.GetButton("Submit"); } }
    public static bool ButtonB { get { return Input.GetButton("Cancel"); } }
    public static bool ButtonX { get { return Input.GetButton("Fire3"); } }
    public static bool ButtonY { get { return Input.GetButton("Jump"); } }
    public static bool RightBumper { get { return Input.GetButton("Right Bumper"); } }
    public static bool LeftBumper { get { return Input.GetButton("Left Bumper"); } }
    public static bool ButtonBack { get { return Input.GetButton("Back Button"); } }
    public static bool ButtonStart { get { return Input.GetButton("Start Button"); } }
}
