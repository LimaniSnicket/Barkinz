using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    Rigidbody dartBody { get => GetComponent<Rigidbody>(); }
    BoxCollider dartCollider { get => GetComponent<BoxCollider>(); }

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

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red);
    }

    void Launch(float power)
    {
        Thrown = true;
        dartBody.useGravity = true;
        dartBody.AddForce((transform.forward + Vector3.up) * 1.5f * power, ForceMode.Impulse);
        transform.SetParent(null);
    }

    public static event Action<Color, int> DartHit;

    public void OnCollisionEnter(Collision collision)
    {
        Vector3 v = collision.GetContact(0).point;
        Texture2D t = (Texture2D)collision.transform.GetComponent<MeshRenderer>().material.mainTexture;
        Color c =  t.GetPixelBilinear(v.x, v.y);
        int pointVal = int.Parse(collision.gameObject.name);
        DartHit(c, pointVal);
        Debug.Log(pointVal);
        Destroy(this);
    }

    private void OnDestroy()
    {
        Destroy(dartBody);
        Destroy(dartCollider);
    }

}
