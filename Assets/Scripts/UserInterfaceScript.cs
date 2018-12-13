using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceScript : MonoBehaviour {

    private Text testText;

    public enum DebugType {
        Gamepad,
        Trajectory,
        Joint
    }
    public DebugType debug;

    void Awake() {

        testText = GameObject.Find("DebugText").GetComponent<Text>();        
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

        if (debug == DebugType.Gamepad) {
            PrintGamepadInputs();

        } else if (debug == DebugType.Trajectory) {
            PrintTrajectoryData();

        } else if (debug == DebugType.Joint) {


        }
        
    }

    private void PrintGamepadInputs() {

        string output = "";

        output += "Left stick Axis X: " + GamepadMap.LeftStickAxisX + "\n";
        output += "Left stick Axis Y: " + GamepadMap.LeftStickAxisY + "\n";

        output += "Right stick Axis X: " + GamepadMap.RightStickAxisX + "\n";
        output += "Right stick Axis Y: " + GamepadMap.RightStickAxisY + "\n";

        output += "Button A: " + GamepadMap.ButtonA + "\n";
        output += "Button B: " + GamepadMap.ButtonB + "\n";

        output += "Button X: " + GamepadMap.ButtonX + "\n";
        output += "Button Y: " + GamepadMap.ButtonY + "\n";

        output += "Left trigger: " + GamepadMap.LeftTrigger + "\n";
        output += "Right trigger: " + GamepadMap.RightTrigger + "\n";

        output += "Left Bumper: " + GamepadMap.LeftBumper + "\n";
        output += "Right Bumper: " + GamepadMap.RightBumper + "\n";

        output += "Button Back: " + GamepadMap.ButtonBack + "\n";
        output += "Button Start: " + GamepadMap.ButtonStart + "\n";

        output += "D-pad Axis X: " + GamepadMap.DPadAxisX + "\n";
        output += "D-pad Axis Y: " + GamepadMap.DPadAxisY + "\n";

        testText.text = output;
    }

    private void PrintTrajectoryData() {

        string output = "";



        testText.text = output;

    }

}
