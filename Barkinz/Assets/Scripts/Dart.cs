using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Dart : MonoBehaviour
{
    Rigidbody dartBody { get => GetComponent<Rigidbody>(); }
    public bool Thrown;

    private void Start()
    {
        dartBody.useGravity = false;
    }

    public float pow;
    float PowerCap = 4;

    Vector3 currentPosition, deltaPosition, lastPosition;
    private void Update()
    {
        if (Input.GetKey(KeyCode.U)) { pow += Time.deltaTime * 1.2f; }
        if (Input.GetKeyUp(KeyCode.U)) { Launch(Mathf.Min(pow, PowerCap)); }
        Vector3 rot = transform.eulerAngles;
        currentPosition = Input.mousePosition;
        deltaPosition = currentPosition - lastPosition;
        lastPosition = currentPosition;
        if (Input.GetMouseButton(0))
        {
            transform.eulerAngles = rot + new Vector3(deltaPosition.y / 10, deltaPosition.x / 10, 0);
        }
    }

    void Launch(float power)
    {
        Thrown = true;
        dartBody.useGravity = true;
        dartBody.AddForce((transform.forward + Vector3.up) * 1.5f * power, ForceMode.Impulse);
        transform.SetParent(null);
    }

    public static event Action<Vector3> DartHit;

    public void OnCollisionEnter(Collision collision)
    {
        DartHit(collision.GetContact(0).point);
        Destroy(this);
    }

    private void OnDestroy()
    {
        Destroy(dartBody);
    }

}
