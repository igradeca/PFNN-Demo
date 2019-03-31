using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTrajectoryAndAnimScript : MonoBehaviour {
    
    private Transform characterBody;
    private Transform trajectoryPath;

    public GameObject framePointPrefab;
    public GameObject jointPrefab;
    public float phase;

    // Character Joints stuff 
    [Header("Joints")]
    public int jointsNumber = 31;
    
    public float strafeAmount;
    public float strafeTarget;
    public float crouchedAmount;
    public float crouchedTarget;
    public float responsive;

    public struct JointsComponents {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;

        public GameObject jointPoint;
    }
    public JointsComponents[] joints;

    // Trajectory values
    [Header("Trajectory")]
    [Range(0.0f, 1.0f)]
    public float scaleFactor = 0.04f; // 0.06f
    private float oppositeScaleFactor;

    public int numberOfTrajectoryProjections = 12;
    public int trajectoryLength;

    [Tooltip("Distance of right and left from middle trajectory point.")]
    public float sidePointsOffset = 25.0f;

    [Tooltip("Layer index which should be ignored when measuring height.")]
    public int layerMask;
    public float heightOrigin;

    private Vector3 targetDirection;
    private Vector3 targetVelocity;    

    public struct TrajectoryComponents {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 direction;
        public float height;
        public float gaitStand;
        public float gaitWalk;
        public float gaitJog;
        public float gaitCrouch;
        public float gaitJump;
        public float gaitBump;

        public GameObject framePoint;
    }

    public TrajectoryComponents[] points;

    private Utils.WallPoints[] terrainWalls;
    public float wallWidth = 1.5f;
    public float wallVal = 1.1f;

    // Use this for initialization
    void Start () {

        GetAllWalls();

        characterBody = gameObject.transform.GetChild(1);
        InitializeJoints();

        trajectoryPath = gameObject.transform.GetChild(2);
        InitializeTrajectory();

        layerMask = 1 << layerMask;
        layerMask = ~layerMask;

        oppositeScaleFactor = 1 / scaleFactor;
    }

    private void GetAllWalls() {

        Transform terrainWalls = GameObject.Find("TerrainWalls").transform;
        this.terrainWalls = new Utils.WallPoints[terrainWalls.childCount];
        //Debug.Log(TerrainWalls.Length);

        for (int i = 0; i < terrainWalls.childCount; i++) {
            this.terrainWalls[i] = terrainWalls.GetChild(i).GetComponent<WallScript>().GetWallPoints();
        }
    }

    private void InitializeJoints() {

        phase = 0.0f;

        strafeAmount = 0.0f;
        strafeTarget = 0.0f;
        crouchedAmount = 0.0f;
        crouchedTarget = 0.0f;
        responsive = 0.0f;

        joints = new JointsComponents[jointsNumber];

        InstantiateJoints();
    }

    private void InstantiateJoints() {

        for (int i = 0; i < jointsNumber; i++) {
            var newJoint = Instantiate(
                jointPrefab,
                new Vector3(
                    transform.position.x,
                    0.5f,
                    transform.position.z),
                Quaternion.identity,
                characterBody);
            newJoint.name = "Joint_" + i;
            
            joints[i].jointPoint = newJoint;
        }
    }

    private void InitializeTrajectory() {

        trajectoryLength = numberOfTrajectoryProjections * 10;
        points = new TrajectoryComponents[trajectoryLength];

        targetDirection = Vector3.forward; //new Vector3(0.0f, 0.0f, 1.0f); 
        targetVelocity = new Vector3();

        InstantiateTrajectoryPoints();
    }

    private void InstantiateTrajectoryPoints() {

        for (int i = 0; i < trajectoryLength; i += 10) {
            var newPoint = Instantiate(
                framePointPrefab,
                new Vector3(
                    transform.position.x,
                    transform.position.y,
                    (transform.position.z + (-6f + (i / 10)))),                      // temporary fake value
                Quaternion.identity,
                trajectoryPath);
            newPoint.name = "FramePoint_" + (i / 10);

            points[i].framePoint = newPoint;
        }
    }

    public void Reset(Vector3 initialPosition, Matrix Y) {

        Vector3 rootPosition = new Vector3(
            initialPosition.x,
            GetHeightSample(initialPosition),
            initialPosition.z);
        Quaternion rootRotation = new Quaternion();

        for (int i = 0; i < jointsNumber; i++) {
            int oPosition = 8 + (((trajectoryLength / 2) / 10) * 4) + (jointsNumber * 3 * 0);
            int oVelocity = 8 + (((trajectoryLength / 2) / 10) * 4) + (jointsNumber * 3 * 1);
            int oRotation = 8 + (((trajectoryLength / 2) / 10) * 4) + (jointsNumber * 3 * 2);

            Vector3 position = rootRotation
                * new Vector3(
                Y[oPosition + i * 3 + 0],
                Y[oPosition + i * 3 + 1],
                Y[oPosition + i * 3 + 2])
                + rootPosition;
            Vector3 velocity = rootRotation
                * new Vector3(
                Y[oVelocity + i * 3 + 0],
                Y[oVelocity + i * 3 + 1],
                Y[oVelocity + i * 3 + 2]);
            Quaternion rotation = rootRotation
                * Utils.QuaternionExponent(new Vector3(                                                            // Possible error (1061)
                    Y[oRotation + i * 3 + 0],
                    Y[oRotation + i * 3 + 1],
                    Y[oRotation + i * 3 + 2]));

            joints[i].position = position;
            joints[i].velocity = velocity;
            joints[i].rotation = rotation;
        }

        for (int i = 0; i < trajectoryLength; i++) {
            points[i].position = rootPosition;
            points[i].rotation = rootRotation;
            points[i].direction = Vector3.forward;//new Vector3(0.0f, 0.0f, 1.0f);
            points[i].height = rootPosition.y;
            points[i].gaitStand = 0.0f;
            points[i].gaitWalk = 0.0f;
            points[i].gaitJog = 0.0f;
            points[i].gaitCrouch = 0.0f;
            points[i].gaitJump = 0.0f;
            points[i].gaitBump = 0.0f;
        }

        phase = 0.0f;
    }

    public void UpdateStrafe(float leftTrigger) {

        strafeTarget = leftTrigger;
        strafeAmount = Mathf.Lerp(strafeAmount, strafeTarget, Utils.extraStrafeSmooth);
    }

    public void UpdateTargetDirectionAndVelocity(Vector3 newTargetDirection, float axisX, float axisY, float rightTrigger) {

        Quaternion newTargetRotation = Quaternion.AngleAxis(
            (Mathf.Atan2(newTargetDirection.x, newTargetDirection.z) * Mathf.Rad2Deg),
            Vector3.up);//new Vector3(0.0f, 1.0f, 0.0f)
        
        float movementSpeed = 2.5f + 2.5f * rightTrigger;

        Vector3 newTargetVelocity = movementSpeed * (newTargetRotation * (new Vector3(axisX, 0.0f, axisY)));
        targetVelocity = Vector3.Lerp(targetVelocity, newTargetVelocity, Utils.extraVelocitySmooth);
        
        Vector3 targetVelocityDirection = targetVelocity.magnitude < 1e-05 ? targetDirection : targetVelocity.normalized;

        newTargetDirection = Utils.MixDirections(targetVelocityDirection, newTargetDirection, strafeAmount);
        targetDirection = Utils.MixDirections(targetDirection, newTargetDirection, Utils.extraDirectionSmooth);

        crouchedAmount = Mathf.Lerp(crouchedAmount, crouchedTarget, Utils.extraCrouchedSmooth);

        Debug.DrawRay(this.transform.position, targetDirection * 10, Color.red);
        Debug.DrawRay(this.transform.position, targetVelocityDirection * 10, Color.green);
    }

    public void UpdateGait(float rightTrigger) {

        if (targetVelocity.magnitude < 0.1f) { // Standing still
            float standAmount = 1.0f - Mathf.Clamp01(targetVelocity.magnitude / 0.1f);
            
            points[trajectoryLength / 2].gaitStand  = Mathf.Lerp(points[trajectoryLength / 2].gaitStand, standAmount, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitWalk   = Mathf.Lerp(points[trajectoryLength / 2].gaitWalk, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJog    = Mathf.Lerp(points[trajectoryLength / 2].gaitJog, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitCrouch = Mathf.Lerp(points[trajectoryLength / 2].gaitCrouch, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJump   = Mathf.Lerp(points[trajectoryLength / 2].gaitJump, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitBump   = Mathf.Lerp(points[trajectoryLength / 2].gaitBump, 0.0f, Utils.extraGaitSmooth);

        } else if (crouchedAmount > 0.1f) { // Crouch
            points[trajectoryLength / 2].gaitStand  = Mathf.Lerp(points[trajectoryLength / 2].gaitStand, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitWalk   = Mathf.Lerp(points[trajectoryLength / 2].gaitWalk, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJog    = Mathf.Lerp(points[trajectoryLength / 2].gaitJog, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitCrouch = Mathf.Lerp(points[trajectoryLength / 2].gaitCrouch, crouchedAmount, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJump   = Mathf.Lerp(points[trajectoryLength / 2].gaitJump, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitBump   = Mathf.Lerp(points[trajectoryLength / 2].gaitBump, 0.0f, Utils.extraGaitSmooth);

        } else if (rightTrigger != 0.0f) { // Jog - 546 wuuuut??
            points[trajectoryLength / 2].gaitStand  = Mathf.Lerp(points[trajectoryLength / 2].gaitStand, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitWalk   = Mathf.Lerp(points[trajectoryLength / 2].gaitWalk, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJog    = Mathf.Lerp(points[trajectoryLength / 2].gaitJog, 1.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitCrouch = Mathf.Lerp(points[trajectoryLength / 2].gaitCrouch, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJump   = Mathf.Lerp(points[trajectoryLength / 2].gaitJump, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitBump   = Mathf.Lerp(points[trajectoryLength / 2].gaitBump, 0.0f, Utils.extraGaitSmooth);

        } else { // Walk
            points[trajectoryLength / 2].gaitStand  = Mathf.Lerp(points[trajectoryLength / 2].gaitStand, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitWalk   = Mathf.Lerp(points[trajectoryLength / 2].gaitWalk, 1.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJog    = Mathf.Lerp(points[trajectoryLength / 2].gaitJog, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitCrouch = Mathf.Lerp(points[trajectoryLength / 2].gaitCrouch, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitJump   = Mathf.Lerp(points[trajectoryLength / 2].gaitJump, 0.0f, Utils.extraGaitSmooth);
            points[trajectoryLength / 2].gaitBump   = Mathf.Lerp(points[trajectoryLength / 2].gaitBump, 0.0f, Utils.extraGaitSmooth);
        }
    }

    public void PredictFutureTrajectory() {

        Vector3[] positionsBlend = new Vector3[trajectoryLength];
        positionsBlend[trajectoryLength / 2] = points[trajectoryLength / 2].position;

        for (int i = ((trajectoryLength / 2) + 1); i < trajectoryLength; i++) {
            float biasPosition = Mathf.Lerp(0.5f, 1.0f, strafeAmount);                                       // On both variables will come character response check (569)
            float biasDirection = Mathf.Lerp(2.0f, 0.5f, strafeAmount);

            float scalePosition = 1.0f - Mathf.Pow((1.0f - ((float)(i - trajectoryLength / 2) / (trajectoryLength / 2))), biasPosition);
            float scaleDirection = 1.0f - Mathf.Pow((1.0f - ((float)(i - trajectoryLength / 2) / (trajectoryLength / 2))), biasDirection);

            positionsBlend[i] = positionsBlend[i - 1] + Vector3.Lerp(
                points[i].position - points[i - 1].position,
                targetVelocity,
                scalePosition);

            // Collide with walls
            for (int j = 0; j < terrainWalls.Length; j++) {
                Vector2 trajectoryPoint = new Vector2(positionsBlend[i].x * scaleFactor, positionsBlend[i].z * scaleFactor);

                if ((trajectoryPoint - ((terrainWalls[j].wallStart + terrainWalls[j].wallEnd) / 2.0f)).magnitude >
                    (terrainWalls[j].wallStart - terrainWalls[j].wallEnd).magnitude) {
                    continue;
                }

                Vector2 segmentPoint = Utils.SegmentNearest(terrainWalls[j].wallStart, terrainWalls[j].wallEnd, trajectoryPoint);
                float segmentDistance = (segmentPoint - trajectoryPoint).magnitude;

                if (segmentDistance < (wallWidth + wallVal)) {
                    Vector2 point0 = (wallWidth + 0.0f) * (trajectoryPoint - segmentPoint).normalized + segmentPoint;
                    Vector2 point1 = (wallWidth + wallVal) * (trajectoryPoint - segmentPoint).normalized + segmentPoint;
                    Vector2 point = Vector2.Lerp(point0, point1, Mathf.Clamp01(segmentDistance - wallWidth) / wallVal);

                    positionsBlend[i].x = point.x * oppositeScaleFactor;
                    positionsBlend[i].z = point.y * oppositeScaleFactor;
                }
            }

            points[i].direction = Utils.MixDirections(points[i].direction, targetDirection, scaleDirection);

            points[i].height = points[trajectoryLength / 2].height;

            points[i].gaitStand  = points[trajectoryLength / 2].gaitStand;
            points[i].gaitWalk   = points[trajectoryLength / 2].gaitWalk;
            points[i].gaitJog    = points[trajectoryLength / 2].gaitJog;
            points[i].gaitCrouch = points[trajectoryLength / 2].gaitCrouch;
            points[i].gaitJump   = points[trajectoryLength / 2].gaitJump;
            points[i].gaitBump   = points[trajectoryLength / 2].gaitBump;
        }

        for (int i = ((trajectoryLength / 2) + 1); i < trajectoryLength; i++) {
            points[i].position = positionsBlend[i];
        }

        // crouch stuff
        
    }

    public void Jumps() {

        for (int i = ((trajectoryLength / 2) + 1); i < trajectoryLength; i++) {
            points[i].gaitJump = 0.0f;

            points[i].gaitJump = Mathf.Max(
                points[i].gaitJump,
                1.0f - Mathf.Clamp01( (3.0f / 5.0f))
                );
        }
    }

    public void Walls() {

        for (int i = 0; i < trajectoryLength; i++) {
            points[i].gaitBump = 0.0f;
            for (int j = 0; j < terrainWalls.Length; j++) {
                Vector2 trajectoryPoint = new Vector2(points[i].position.x * scaleFactor, points[i].position.z * scaleFactor);
                Vector2 segmentPoint = Utils.SegmentNearest(terrainWalls[j].wallStart, terrainWalls[j].wallEnd, trajectoryPoint);

                float segmentDistance = (segmentPoint - trajectoryPoint).magnitude;
                points[i].gaitBump = Mathf.Max(points[i].gaitBump, 1.0f - Mathf.Clamp01((segmentDistance - wallWidth) / wallVal));
            }
        }
    }

    public void UpdateRotation() {

        for (int i = 0; i < trajectoryLength; i++) {
            points[i].rotation = Quaternion.AngleAxis(
                (Mathf.Atan2(points[i].direction.x, points[i].direction.z) * Mathf.Rad2Deg),
                Vector3.up);
            //new Vector3(0.0f, 1.0f, 0.0f));
        }
    }

    public void UpdateHeights() {

        for (int i = (trajectoryLength / 2); i < trajectoryLength; i++) {
            points[i].position.y = GetHeightSample(points[i].position);
        }

        points[trajectoryLength / 2].height = 0.0f;
        for (int i = 0; i < trajectoryLength; i += 10) {
            points[trajectoryLength / 2].height += (points[i].position.y / (trajectoryLength / 10));
        }
    }

    public float GetHeightSample(Vector3 position) {
        
        RaycastHit hit;
        position.Scale(new Vector3(scaleFactor, 0.0f, scaleFactor));    
        position.y = heightOrigin;

        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
            if (hit.transform.tag == "Terrain") {
                //Debug.DrawRay(position, Vector3.down, Color.blue);
                return (heightOrigin - hit.distance) * oppositeScaleFactor;
            }            
        }        
        return 0.0f;
    }

    public void UpdateNetworkInput(ref Matrix X) {

        Vector3 rootPosition = new Vector3(
            points[trajectoryLength / 2].position.x,
            points[trajectoryLength / 2].height,
            points[trajectoryLength / 2].position.z);

        Quaternion rootRotation = points[trajectoryLength / 2].rotation;

        int w = trajectoryLength / 10;

        // Trajectory position and direction
        for (int i = 0; i < trajectoryLength; i += 10) {
            Vector3 position = Quaternion.Inverse(rootRotation) * (points[i].position - rootPosition);
            Vector3 direction = Quaternion.Inverse(rootRotation) * points[i].direction;

            X[(w * 0) + (i / 10)] = position.x;
            X[(w * 1) + (i / 10)] = position.z;

            X[(w * 2) + (i / 10)] = direction.x;
            X[(w * 3) + (i / 10)] = direction.z;
        }

        // Trajectory gaits
        for (int i = 0; i < trajectoryLength; i += 10) {
            X[(w * 4) + (i / 10)] = points[i].gaitStand;
            X[(w * 5) + (i / 10)] = points[i].gaitWalk;
            X[(w * 6) + (i / 10)] = points[i].gaitJog;
            X[(w * 7) + (i / 10)] = points[i].gaitCrouch;
            X[(w * 8) + (i / 10)] = points[i].gaitJump;
            X[(w * 9) + (i / 10)] = 0.0f;
        }

        // Joint previous position, velocity and rotation
        Vector3 previousRootPosition = new Vector3(
            points[(trajectoryLength / 2) - 1].position.x,
            points[(trajectoryLength / 2) - 1].height,
            points[(trajectoryLength / 2) - 1].position.z);

        Quaternion previousRootRotation = points[(trajectoryLength / 2) - 1].rotation;

        int o = (trajectoryLength / 10) * 10;
        for (int i = 0; i < jointsNumber; i++) {
            Vector3 pos = Quaternion.Inverse(previousRootRotation) * (joints[i].position - previousRootPosition);
            Vector3 prv = Quaternion.Inverse(previousRootRotation) * joints[i].velocity;

            X[o + (jointsNumber * 3 * 0) + (i * 3 + 0)] = pos.x;
            X[o + (jointsNumber * 3 * 0) + (i * 3 + 1)] = pos.y;
            X[o + (jointsNumber * 3 * 0) + (i * 3 + 2)] = pos.z;

            X[o + (jointsNumber * 3 * 1) + (i * 3 + 0)] = prv.x;
            X[o + (jointsNumber * 3 * 1) + (i * 3 + 1)] = prv.y;
            X[o + (jointsNumber * 3 * 1) + (i * 3 + 2)] = prv.z;            
        }

        // Trajectory heights
        o += (jointsNumber * 3 * 2);
        for (int i = 0; i < trajectoryLength; i += 10) {
            Vector3 positionRight = points[i].position + (points[i].rotation * new Vector3(sidePointsOffset, 0.0f, 0.0f));
            Vector3 positionLeft  = points[i].position + (points[i].rotation * new Vector3(-sidePointsOffset, 0.0f, 0.0f));

            X[o + (w * 0) + (i / 10)] = GetHeightSample(positionRight) - rootPosition.y;
            X[o + (w * 1) + (i / 10)] = points[i].position.y - rootPosition.y;
            X[o + (w * 2) + (i / 10)] = GetHeightSample(positionLeft) - rootPosition.y;
        }
    }

    public void BuildLocalTransforms(Matrix Y) {

        Vector3 rootPosition = new Vector3(
            points[trajectoryLength / 2].position.x,
            points[trajectoryLength / 2].height,
            points[trajectoryLength / 2].position.z);

        Quaternion rootRotation = points[trajectoryLength / 2].rotation;

        for (int i = 0; i < jointsNumber; i++) {
            int oPosition = 8 + (((trajectoryLength / 2) / 10) * 4) + (jointsNumber * 3 * 0);
            int oVelocity = 8 + (((trajectoryLength / 2) / 10) * 4) + (jointsNumber * 3 * 1);
            int oRotation = 8 + (((trajectoryLength / 2) / 10) * 4) + (jointsNumber * 3 * 2);
            
            Vector3 position = rootRotation
                * new Vector3(
                Y[oPosition + i * 3 + 0],
                Y[oPosition + i * 3 + 1],
                Y[oPosition + i * 3 + 2])
                + rootPosition;
            Vector3 velocity = rootRotation
                * new Vector3(
                Y[oVelocity + i * 3 + 0],
                Y[oVelocity + i * 3 + 1],
                Y[oVelocity + i * 3 + 2]);
            Quaternion rotation = rootRotation
                * Utils.QuaternionExponent(                                                                       // Possible error (1061)
                new Vector3(
                    Y[oRotation + i * 3 + 0],
                    Y[oRotation + i * 3 + 1],
                    Y[oRotation + i * 3 + 2]));

            joints[i].position = Vector3.Lerp((joints[i].position + velocity), position, Utils.extraJointSmooth);
            joints[i].velocity = velocity;
            joints[i].rotation = rotation;

            // code goes here (1705 - 1722)
        }
    }

    public void PostVisualisationCalculation(Matrix Y) {

        // Update past trajectory
        for (int i = 0; i < (trajectoryLength / 2); i++) {
            points[i].position   = points[i + 1].position;
            points[i].rotation   = points[i + 1].rotation;
            points[i].direction  = points[i + 1].direction;
            points[i].height     = points[i + 1].height;
            points[i].gaitStand  = points[i + 1].gaitStand;
            points[i].gaitWalk   = points[i + 1].gaitWalk;
            points[i].gaitJog    = points[i + 1].gaitJog;
            points[i].gaitCrouch = points[i + 1].gaitCrouch;
            points[i].gaitJump   = points[i + 1].gaitJump;
            points[i].gaitBump   = points[i + 1].gaitBump;
        }

        // Update current trajectory
        float standAmount = GetStandAmount();

        Vector3 trajectoryUpdate = points[trajectoryLength / 2].rotation * new Vector3(Y[0], 0.0f, Y[1]);
        points[trajectoryLength / 2].position = points[trajectoryLength / 2].position + standAmount * trajectoryUpdate;
        
        points[trajectoryLength / 2].direction = Quaternion.AngleAxis(
            ((standAmount * -Y[2, 0]) * Mathf.Rad2Deg), Vector3.up) * points[trajectoryLength / 2].direction; // new Vector3(0.0f, 1.0f, 0.0f)

        points[trajectoryLength / 2].rotation = Quaternion.AngleAxis(
                (Mathf.Atan2(points[trajectoryLength / 2].direction.x, points[trajectoryLength / 2].direction.z) * Mathf.Rad2Deg),
                Vector3.up);//new Vector3(0.0f, 1.0f, 0.0f));

        // Collide with walls
        for (int j = 0; j < terrainWalls.Length; j++) {
            Vector2 trajectoryPoint = new Vector2(points[trajectoryLength / 2].position.x * scaleFactor, points[trajectoryLength / 2].position.z * scaleFactor);
            Vector2 segmentPoint = Utils.SegmentNearest(terrainWalls[j].wallStart, terrainWalls[j].wallEnd, trajectoryPoint);

            float segmentDistance = (segmentPoint - trajectoryPoint).magnitude;

            if (segmentDistance < (wallWidth + wallVal)) {
                Vector2 point0 = (wallWidth + 0.0f) * (trajectoryPoint - segmentPoint).normalized + segmentPoint;
                Vector2 point1 = (wallWidth + wallVal) * (trajectoryPoint - segmentPoint).normalized + segmentPoint;
                Vector2 point = Vector2.Lerp(point0, point1, Mathf.Clamp01((segmentDistance - wallWidth) / wallVal));

                points[trajectoryLength / 2].position.x = point.x * oppositeScaleFactor;
                points[trajectoryLength / 2].position.z = point.y * oppositeScaleFactor;
            }
        }

        // Update future trajectory
        int w = (trajectoryLength / 2) / 10;
        for (int i = ((trajectoryLength / 2) + 1); i < trajectoryLength; i++) {
            float m = ((float)i - (float)(trajectoryLength / 2) / 10.0f) % 1.0f;

            points[i].position.x  = (1 - m) * Y[8 + (w * 0) + (i / 10) - w] + m * Y[8 + (w * 0) + (i / 10) - (w + 1)];
            points[i].position.z  = (1 - m) * Y[8 + (w * 1) + (i / 10) - w] + m * Y[8 + (w * 1) + (i / 10) - (w + 1)];
            points[i].direction.x = (1 - m) * Y[8 + (w * 2) + (i / 10) - w] + m * Y[8 + (w * 2) + (i / 10) - (w + 1)];
            points[i].direction.z = (1 - m) * Y[8 + (w * 3) + (i / 10) - w] + m * Y[8 + (w * 3) + (i / 10) - (w + 1)];

            points[i].position = (points[trajectoryLength / 2].rotation * points[i].position) + points[trajectoryLength / 2].position;
            points[i].direction = Vector3.Normalize(points[trajectoryLength / 2].rotation * points[i].direction);
            points[i].rotation = Quaternion.AngleAxis(
                (Mathf.Atan2(points[i].direction.x, points[i].direction.z) * Mathf.Rad2Deg),
                Vector3.up);//new Vector3(0.0f, 1.0f, 0.0f));
        }
    }

    public void UpdatePhase(Matrix Y) {

        phase = (phase + ((GetStandAmount() * 0.9f) + 0.1f) * (2.0f * Mathf.PI) * Y[3, 0]) % (2.0f * Mathf.PI);
        //Debug.Log(Phase);
    }

    private float GetStandAmount() {

        return Mathf.Pow(1.0f - points[trajectoryLength / 2].gaitStand, 0.25f);
    }

    public void DisplayTrajectory() {

        // Middle point
        for (int i = 0; i < trajectoryLength; i += 10) {
            Vector3 posCenter = -points[i].position;
            posCenter.Scale(new Vector3(scaleFactor, scaleFactor, scaleFactor));
            
            points[i].framePoint.transform.localPosition = -this.transform.position - posCenter;

            if ((i / 10) == 6) {
                this.transform.position = -posCenter;
            }
        }
        
        // Left and right point
        for (int i = 0; i < trajectoryLength; i += 10) {
            // left
            Vector3 posLeft = Vector3.up + (points[i].rotation * new Vector3(-sidePointsOffset * scaleFactor, 0.0f, 0.0f));
            points[i].framePoint.transform.GetChild(0).localPosition = posLeft;

            // right
            Vector3 posRight = Vector3.up + (points[i].rotation * new Vector3(sidePointsOffset * scaleFactor, 0.0f, 0.0f));
            points[i].framePoint.transform.GetChild(2).localPosition = posRight;
        }

        // Direction arrow
        for (int i = 0; i < trajectoryLength; i += 10) {
            Quaternion angle = Quaternion.AngleAxis(
                Mathf.Atan2(points[i].direction.x, points[i].direction.z) * Mathf.Rad2Deg,
                Vector3.up);//new Vector3(0.0f, 1.0f, 0.0f));
            angle *= new Quaternion(0.7f, 0.0f, 0.0f, 0.7f);

            points[i].framePoint.transform.GetChild(1).localRotation = angle;
        }
    }
    
    public void DisplayJoints() {
        
        for (int i = 0; i < jointsNumber; i++) {
            Vector3 position = joints[i].position;
            position.Scale(new Vector3(scaleFactor, scaleFactor, scaleFactor));
            //position.Scale(new Vector3(ScaleFactor, 0.0f, ScaleFactor));

            //Joints[i].jointPoint.transform.localPosition = this.transform.position - position;
            joints[i].jointPoint.transform.localPosition = new Vector3(
                this.transform.position.x - position.x,
                -(this.transform.position.y - position.y),
                this.transform.position.z - position.z);
        }        
    }

    public void Crouch() {

        if (crouchedTarget == 0.0f) {
            crouchedTarget = 1.0f;
        } else {
            crouchedTarget = 0.0f;
        }
    }


}
