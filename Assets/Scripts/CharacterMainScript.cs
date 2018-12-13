using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMain : MonoBehaviour {

    protected CharacterTrajectoryAndAnimScript character;
    protected PFNN_CPU network;
    protected Transform mainCamera;

    [Range(0.0f, 150.0f)]
    public float cameraRotationSensitivity = 90.0f;

    [Range(0.0f, 50.0f)]
    public float cameraZoomSensitivity = 60.0f;

    private float cameraDistance;
    private const float cameraDistanceMax = -40.0f;
    private const float cameraDistanceMin = -2.0f;

    private float cameraAngleX, cameraAngleY;
    private const float cameraAngleMaxX = 80.0f;
    private const float cameraAngleMinX = 5.0f;

    public Vector3 initialWorldPosition;

    // Use this for initialization
    void Start() {

        initialWorldPosition = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z);

        network = new PFNN_CPU();

        character = this.GetComponent<CharacterTrajectoryAndAnimScript>();    

        mainCamera = gameObject.transform.GetChild(0);
        mainCamera.LookAt(transform);

        cameraAngleX = mainCamera.eulerAngles.x;
        cameraAngleY = 0.0f;
        cameraDistance = mainCamera.position.z;
        MoveCamera();

        ResetCharacter();
    }

    // Update is called once per frame
    protected virtual void Update() {

        character.UpdateNetworkInput(ref network.X);
        network.Compute(character.phase);
        character.BuildLocalTransforms(network.Y);

        // display stuff
        character.DisplayTrajectory();
        character.DisplayJoints();

        character.PostVisualisationCalculation(network.Y);
        character.UpdatePhase(network.Y);
    }

    protected void MoveCharacter(float axisX = 0.0f, float axisY = 0.0f, float rightTrigger = 0.0f, float leftTrigger = 0.0f) {

        Vector3 newTargetDirection = Vector3.Normalize(
           new Vector3(mainCamera.forward.x, 0.0f, mainCamera.forward.z));

        //Debug.Log(newTargetDirection);
        Debug.DrawRay(mainCamera.transform.position, newTargetDirection, Color.cyan);        

        character.UpdateStrafe(leftTrigger);
        character.UpdateTargetDirectionAndVelocity(newTargetDirection, axisX, axisY, rightTrigger);
        character.UpdateGait(rightTrigger);
        character.PredictFutureTrajectory();

        //Character.Jumps();
        character.Walls();

        character.UpdateRotation();
        character.UpdateHeights();        
    }

    protected void ResetCharacter() {

        network.Reset();
        character.Reset(initialWorldPosition, network.Y);
    }

    public void UpdateCameraDistance(float value) {

        cameraDistance += value * cameraZoomSensitivity * Time.deltaTime;
        cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMax, cameraDistanceMin);

        MoveCamera();
    }

    protected void MoveCamera(float speedY = 0.0f, float speedX = 0.0f) {

        cameraAngleX += speedX * cameraRotationSensitivity * Time.deltaTime;
        cameraAngleY += speedY * cameraRotationSensitivity * Time.deltaTime;
        cameraAngleX = Mathf.Clamp(cameraAngleX, cameraAngleMinX, cameraAngleMaxX);
        //Debug.Log("X: " + CameraAngleX + " Y: " + CameraAngleY);

        Vector3 dir = new Vector3(0, 0, cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraAngleX, cameraAngleY, 0.0f);

        mainCamera.position = transform.position + (rotation * dir);
        mainCamera.LookAt(transform);

        FixCameraAngles();
    }

    protected void FixCameraAngles() {

        if (cameraAngleY >= 360.0f || cameraAngleY <= -360.0f) {
            cameraAngleY = mainCamera.eulerAngles.y;
        }
    }

    protected void CharacterCrouch() {

        character.Crouch();
    }


}
