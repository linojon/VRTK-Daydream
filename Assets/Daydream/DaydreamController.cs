using UnityEngine;
using System.Collections;

public class DaydreamController : MonoBehaviour {
    public GameObject daydController;
    //public Arm playerArm;

    //public enum GestureType { FLYINGCARPET, SPRINGYHAND, STATIONARY, FLATHOVER };
    //public GestureType gestureType;

    // 

    public enum TouchControl {  NONE, LEFTRIGHT, FRONTBACK, DOWNUP };
    public TouchControl touchXControl;
    public TouchControl touchYControl;
    public bool transformPlane;

    // input control variables
    public float velocity;
    public bool clampReach;
    public bool resetUntouch;

    // calibration settings of the input device
    public Vector3 restPosition;
    public Vector3 restRotation = new Vector3(30f, -90f, 0f);
    public Vector3 reachMin = new Vector3(-0.67f, 0.9f, -0.42f);
    public Vector3 reachMax = new Vector3(0.08f, 1.1f, 0.37f);
    private Quaternion restRotationQ;

    // feddback options
    public bool laserPointer;
    public bool showTouch;
    public GameObject touchSpot;
    private float touchRadius;


    void Start() {
        restRotationQ = Quaternion.Euler(restRotation);
        restPosition = daydController.transform.position;
        // hack to initiate coroutine
        //if (playerArm != null)
        //    playerArm.GripButton(false);
        if (touchSpot != null)
            StartHighlightTouch();
    }

    void Update() {
        if (GvrController.State != GvrConnectionState.Connected) {
            Debug.Log("GvrController.State: " + GvrController.State);
            daydController.SetActive(false);
            return;
        }
        daydController.SetActive(true);

        //if (playerArm != null)
        //    playerArm.GripButton(GvrController.ClickButton);
        UpdateOrientation();
        UpdatePosition();
    }

    private void UpdateOrientation() {
        Quaternion rotation = restRotationQ * GvrController.Orientation;
        daydController.transform.rotation = rotation;
        //playerArm.RotationalTracking(rotation);

    }

    private void UpdatePosition() {
        if (showTouch) {
            touchSpot.SetActive(GvrController.IsTouching);
        }

        if (!GvrController.IsTouching) {
            if (resetUntouch) {
                daydController.transform.position = restPosition;
            }
            return;
        }

        Vector2 touch = GetTouch();

        if (showTouch) {
            HighlightTouch(touch);
        }

        Vector3 pos = daydController.transform.position;
        Vector3 offset = Vector3.zero;

        switch (touchXControl) {
            case TouchControl.LEFTRIGHT:
                offset.z = touch.x;
                break;
            case TouchControl.FRONTBACK:
                break;
            case TouchControl.DOWNUP:
                break;
        }
        switch (touchYControl) {
            case TouchControl.LEFTRIGHT:
                break;
            case TouchControl.FRONTBACK:
                offset.x = -touch.y;
                break;
            case TouchControl.DOWNUP:
                break;
        }

        if (transformPlane) {
            offset = restRotationQ * GvrController.Orientation * offset;
        }

        pos += offset * velocity;

        if (clampReach) {
            pos = ClampPosition(pos);
        }

        //daydController.transform.position = pos;
    }

    private Vector2 GetTouch() {
        Vector2 touch = GvrController.TouchPos;

        // 0,0 is top left of touch pad, x is left-right, y is front-back
        // make 0,0 the middle, +/- 1.0
        touch.x = touch.x * 2.0f - 1.0f;
        touch.y = 1.0f - touch.y *2.0f;
        return touch;
    }

    private void StartHighlightTouch() {
        Mesh touchPad = touchSpot.transform.parent.gameObject.GetComponent<MeshFilter>().mesh;
        touchRadius = touchPad.bounds.extents.magnitude * 0.5f;
        float spotRadius = touchSpot.GetComponent<Renderer>().bounds.extents.magnitude;
        touchRadius -= spotRadius;
    }

    private void HighlightTouch(Vector2 touch) {
        if (touchSpot == null)
            return;

        // these can be initialized before
        //Vector3 touchCenter = touchPad.bounds.center;
        //Debug.Log("touchCenter: " + touchCenter + " touchRadius: " + touchRadius + " spotRadius: " + spotRadius);

        //Debug.Log("touch: " + touch);
        //Vector3 pos = touchCenter;
        //pos.x += touch.x * touchRadius;
        //pos.z += touch.y * touchRadius;
        //touchSpot.transform.position = pos;

        Vector3 pos = touchSpot.transform.localPosition;
        pos.x = touch.x * touchRadius;
        pos.z = touch.y * touchRadius;
        // preserve default height y
        touchSpot.transform.localPosition = pos;

    }

    private Vector3 ClampPosition(Vector3 pos) {
        Vector3 clamped;

        // clamp postion within users reach
        clamped.x = Mathf.Clamp(pos.x, reachMin.x, reachMax.x);
        clamped.y = Mathf.Clamp(pos.y, reachMin.y, reachMax.y);
        clamped.z = Mathf.Clamp(pos.z, reachMin.z, reachMax.z);
        return clamped;
    }
}
