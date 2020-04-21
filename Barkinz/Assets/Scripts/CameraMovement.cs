using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class CameraMovement : MonoBehaviour
{
    private static CameraMovement camMovement;
    private static Vector3 HomePosition;
    private static Quaternion HomeRotation;
    public float HomeOrthographicSize;
    public static bool Zoomed { get; private set; }
    private void Start()
    {
        if (camMovement == null) { camMovement = this; } else { Destroy(this); }
        HomePosition = transform.position;
        HomeOrthographicSize = Camera.main.orthographicSize;
        HomeRotation = transform.rotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (!Zoomed)
            {
                SetZoomTargetViaClick();
            } else
            {
                ResetZoom();
            }
        }
    }

    public static void AlignWithTransform()
    {
        camMovement.transform.position = HomePosition;
        camMovement.transform.rotation = HomeRotation;
        Camera.main.orthographic = true;
    }

    public static void AlignWithTransform(Transform t, bool ortho)
    {
        camMovement.transform.position = t.position;
        camMovement.transform.rotation = t.rotation;
        Camera.main.orthographic = ortho;
    }


    public void SetZoomTargetViaClick()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rh = new RaycastHit();
        if (Physics.SphereCast(r, .5F, out rh, Mathf.Infinity))
        {
            Debug.Log(rh.transform.name);
            if (rh.transform.GetComponent<MonoBehaviour>() is IZoomOn)
            {
                IZoomOn zoomOn = (IZoomOn)rh.transform.GetComponent<MonoBehaviour>();
                StartCoroutine(LerpCameraToZoomPosition(zoomOn.ZoomCamPosition(), 10f));
                StartCoroutine(LerpCameraToZoomPosition(zoomOn.CameraOrthoSize, 12f));
                Zoomed = true;
            }
        }
    }

    public void ResetZoom()
    {
        StartCoroutine(LerpCameraToZoomPosition(HomePosition, 10));
        StartCoroutine(LerpCameraToZoomPosition(HomeOrthographicSize, 12f));
        camMovement.transform.rotation = HomeRotation;
        Camera.main.orthographic = true;
        Zoomed = false;
    }

    public static void ResetCameraZoom()
    {
        camMovement.ResetZoom();
    }


    public static event Action<IZoomOn> ZoomedOnObject;
    public static void ZoomOn(IZoomOn zoom, bool adjustForward = false)
    {
        if (!Zoomed) {
            Camera.main.orthographic = false;
            camMovement.StartCoroutine(camMovement.LerpCameraToZoomPosition(zoom.ZoomCamPosition(), 5));
            camMovement.StartCoroutine(camMovement.LerpCameraToZoomPosition(zoom.CameraOrthoSize, 12));
            if (adjustForward) { camMovement.transform.forward = (zoom.ZoomObjectTransform.forward + zoom.ZoomObjectTransform.right); }
            Zoomed = true;
            ZoomedOnObject(zoom);
        }
    }

    public IEnumerator LerpCameraToZoomPosition(Vector3 pos, float speed)
    {
        while (!transform.position.SqueezeVectors(pos))
        {
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * speed);
            yield return null;
        }
        transform.position = pos;
    }

    public IEnumerator LerpCameraToZoomPosition(float ortho, float speed)
    {
        while (!Camera.main.orthographicSize.SqueezeFloats(ortho))
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, ortho, Time.deltaTime * speed);
            yield return null;
        }
        Camera.main.orthographicSize = ortho;
    }

}

public interface IZoomOn
{
    Vector3 ZoomCamPosition();
    Transform ZoomObjectTransform { get; }
    float CameraOrthoSize { get; }
}

public static class Squeeze
{
    public static bool SqueezeVectors(this Vector3 v, Vector3 comp, float threshold = 0.01f)
    {
        return Mathf.Abs(Vector3.Distance(v, comp)) <= threshold;
    }

    public static bool SqueezeFloats(this float f, float comp, float threshold = 0.01f)
    {
        return Mathf.Abs(f - comp) <= threshold;
    }

    public static bool InRange(this float f, float floor, float ceil)
    {
        return f >= floor && f < ceil;
    }
}
