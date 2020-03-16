using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMovement : MonoBehaviour
{
    public Vector3 HomePosition;
    public float HomeOrthographicSize;

    private void Start()
    {
        HomePosition = transform.position;
        HomeOrthographicSize = Camera.main.orthographicSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            SetZoomTargetViaClick();
        }
    }

    public void SetZoomTargetViaClick()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rh = new RaycastHit();
        if (Physics.SphereCast(r, 1, out rh, Mathf.Infinity))
        {
            Debug.Log(rh.transform.name);
            if (rh.transform.GetComponent<MonoBehaviour>() is IZoomOn)
            {
                IZoomOn zoomOn = (IZoomOn)rh.transform.GetComponent<MonoBehaviour>();
                StartCoroutine(LerpCameraToZoomPosition(zoomOn.ZoomCamPosition(), .5f));
                StartCoroutine(LerpCameraToZoomPosition(zoomOn.CameraOrthoSize, .3f));
                //transform.position = zoomOn.ZoomCamPosition();
                //Camera.main.orthographicSize = zoomOn.CameraOrthoSize;
            }
        }
    }

    public IEnumerator LerpCameraToZoomPosition(Vector3 pos, float speed)
    {
        while (Vector3.Distance(pos, transform.position) <= 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * speed);
            yield return null;
        }
        transform.position = pos;
    }

    public IEnumerator LerpCameraToZoomPosition(float ortho, float speed)
    {
        while (Mathf.Abs(ortho - Camera.main.orthographicSize) <= 0.001)
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
