using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;


[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class CreateObject : MonoBehaviour
{
    [Header("Config")]
    public Camera arCamera;
    public LayerMask layerMask;

    [Header("Prefabs")]
    public GameObject prefabOrigin;
    private GameObject instantiatedOrigin;

    public GameObject prefabTarget;
    private GameObject instantiatedTarget;

    //other Definitions
    private bool originSet = false;
    private float targetTime = 0;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private LineRenderer lineRendererLaser;

    EnhancedTouch.Finger _finger;

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        lineRendererLaser = GetComponent<LineRenderer>();
        lineRendererLaser.positionCount = 2;
        lineRendererLaser.enabled = false;
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
        EnhancedTouch.Touch.onFingerUp += FingerUp;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.Touch.onFingerUp -= FingerUp;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;
        _finger = finger;
    }

    private void Update()
    {
        targetTime -= Time.deltaTime;

        if (_finger != null)
        {
            Ray ray = arCamera.ScreenPointToRay(_finger.currentTouch.screenPosition);
            RaycastHit hitObject;

            lineRendererLaser.SetPosition(0, arCamera.transform.position
                + arCamera.transform.right * -0.1f + arCamera.transform.up * -0.1f);

            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (Physics.Raycast(ray, out hitObject, 100f, layerMask))
            {
                lineRendererLaser.SetPosition(1, hitObject.transform.position);
                lineRendererLaser.enabled = true;

                if (originSet)
                {
                    Destroy(instantiatedTarget);
                    instantiatedTarget = null;

                    targetTime = 2;
                }
            }
            else if (raycastManager.Raycast(_finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = hits[0].pose;

                lineRendererLaser.SetPosition(1, pose.position);
                lineRendererLaser.enabled = true;

                if (!originSet)
                {
                    instantiatedOrigin = Instantiate(prefabOrigin, pose.position, pose.rotation);
                    originSet = true;
                    targetTime = 2;
                }
            }
            else
            {
                lineRendererLaser.enabled = false;
            }
        }

        if (originSet && instantiatedTarget == null && targetTime < 0)
        {
            throwTarget();
        }

        if (instantiatedTarget != null && 
            Mathf.Abs(Vector3.Distance(instantiatedTarget.transform.position, arCamera.transform.position)) > 25.0f)
        {
            Destroy(instantiatedTarget);
            instantiatedTarget = null;

            Destroy(instantiatedOrigin);
            originSet = false;
        }
    }

    private void FingerUp(EnhancedTouch.Finger finger)
    {
        _finger = null;
        lineRendererLaser.enabled = false;
    }

    private void throwTarget()
    {
        instantiatedTarget = Instantiate(prefabTarget, instantiatedOrigin.transform.position + new Vector3(0.0f, 0.1f, 0.0f), instantiatedOrigin.transform.rotation);

        Rigidbody rigidbodyTarget = instantiatedTarget.GetComponent<Rigidbody>();

        Vector3 force = new(Random.Range(-4f, 4f), Random.Range(22f, 27f), Random.Range(-4f, 4f));
        Vector3 torque = new(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        rigidbodyTarget.AddForce(force, ForceMode.Impulse);
        rigidbodyTarget.AddTorque(torque, ForceMode.Impulse);
    }
}