using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMovement : MonoBehaviour
{
    public Vector3 HomePosition;
    public float HomeOrthographicSize;
    public bool Zoomed { get; private set; }

    private void Start()
    {
        HomePosition = transform.position;
        HomeOrthographicSize = Camera.main.orthographicSize;
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
        Zoomed = false;
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
    float CameraOrthoSize { get; set; }
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
}
