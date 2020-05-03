using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : MonoBehaviour
{
    Rigidbody dartBody { get => GetComponent<Rigidbody>(); }
    BoxCollider dartCollider { get => GetComponent<BoxCollider>(); }
    AudioSource audioSource { get => GetComponent<AudioSource>(); }
    ActivePlayer activePlayer;

    public bool Thrown, pegged;

    private void Start()
    {
        dartBody.useGravity = false;
    }

    public float pow;
    float PowerCap = 6;

    Vector3 currentPosition, deltaPosition, lastPosition;
    private void Update()
    {
        if(!pegged && !DartsManager.isActiveDart(this)) { Destroy(gameObject); }
        if (Input.GetMouseButton(0)) { pow += Time.deltaTime * 1.2f; }
        if (Input.GetMouseButtonUp(0)) { Launch(Mathf.Min(pow, PowerCap)); }
        Vector3 rot = transform.eulerAngles;
        currentPosition = Input.mousePosition;
        deltaPosition = currentPosition - lastPosition;
        lastPosition = currentPosition;
        if (Input.GetMouseButton(0))
        {
            transform.eulerAngles = rot + new Vector3(deltaPosition.y / 10, deltaPosition.x / 10, 0);
        }

        transform.position += DrunkTilt() * HelperFunctions.PulseValue(0, 1, .5f, 4, 0); //Mathf.Cos(Time.time);

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red);
    }

    public void SetPlayerToTrack(ActivePlayer p)
    {
        activePlayer = p;
    }

    void Launch(float power)
    {
        Thrown = true;
        dartBody.useGravity = true;
        dartBody.AddForce((transform.forward * 3 + Vector3.up * 0.6f) * power, ForceMode.Impulse);
        transform.SetParent(null);
    }

    Vector3 DrunkTilt()
    {
        float mod = 1f;
        try { mod = activePlayer.ActiveSessionIntoxication.intoxicationLevel/100; } catch (NullReferenceException) { }
        return new Vector3(0.03f, 0.01f, 0) * mod;
    }

    public static event Action<Color, int, Vector3> DartHit;

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<Dart>())
        {
            Physics.IgnoreCollision(dartCollider, collision.collider);
        }

        Vector3 v = collision.GetContact(0).point;
        Texture2D t = (Texture2D)collision.transform.GetComponent<MeshRenderer>().material.mainTexture;
        Color c = Color.clear;
        int pointVal = 0;
        if (t.isReadable)
        {
            c = t.GetPixelBilinear(v.x, v.y);
            pointVal = int.Parse(collision.gameObject.name);
            pegged = true;
        }

        DartHit(c, pointVal, v);
        audioSource.Play();
        Destroy(this);
    }

    private void OnDestroy()
    {
        Destroy(dartBody);
        Destroy(dartCollider);
    }

}
