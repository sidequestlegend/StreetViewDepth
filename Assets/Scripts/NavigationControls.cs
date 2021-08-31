using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class NavigationControls : MonoBehaviour
{
    public InputDevice LeftController;
    public InputDevice RightController;
    // Left Inputs 
    public static Vector2 left2DAxis;
    public static Vector2 right2DAxis;

    bool snapTriggered = false;

    public PlotNeighbors PlotNeighbors;

    public bool AutoMove;

    Vector2 Get2DAxis(InputDevice device, InputFeatureUsage<Vector2> axis) {
        if (device == null) return Vector2.zero;
        Vector2 inputValue;
        device.TryGetFeatureValue(axis, out inputValue);
        return inputValue;
    }


    void GetControllers() {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, inputDevices);
        if (inputDevices.Count > 0) {
            LeftController = inputDevices[0];
        }
        inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, inputDevices);
        if (inputDevices.Count > 0) {
            RightController = inputDevices[0];
        }
    }

    void Start() {
    }

    // Update is called once per frame
    void LateUpdate() {
        if (!PlotNeighbors.gameObject.activeSelf || PlotNeighbors.currentWaypoint == null) {
            Debug.Log("No waypoint");
            return;
        }

        GetControllers();

        left2DAxis = Get2DAxis(LeftController, CommonUsages.primary2DAxis);
        right2DAxis = Get2DAxis(RightController, CommonUsages.primary2DAxis);
        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);


        var forwardBack = AutoMove ? 1 : left2DAxis.y;
        var turnLeftRight = right2DAxis.x;

        if (forward) {
            forwardBack = 1;
        } else if (backward) {
            forwardBack = -1;
        }

        if (right) {
            turnLeftRight = 1;
        } else if (left) {
            turnLeftRight = -1;
        }

        if (forwardBack > 0.3f) {
            transform.position = Vector3.MoveTowards(transform.position, PlotNeighbors.currentWaypoint.position, Time.deltaTime * 5 * forwardBack);
        }
        if (turnLeftRight > 0.3f || forwardBack < -0.3f) {
            transform.Rotate(Vector3.up * Time.deltaTime * 50);
        } else if (turnLeftRight < -0.3f) {
            transform.Rotate(Vector3.down * Time.deltaTime * 50);
        } else if (forwardBack > 0.3f) {
            //var thisForward = transform.forward;
            //var thatForward = PlotNeighbors.currentWaypoint.forward;
            var difference = Vector3.Distance(transform.forward, PlotNeighbors.currentWaypoint.forward);  // Mathf.Abs(thisForward.x - thatForward.x) + Mathf.Abs(thisForward.z - thatForward.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, PlotNeighbors.currentWaypoint.rotation, Time.deltaTime * difference * 50);
        }
    }
}
