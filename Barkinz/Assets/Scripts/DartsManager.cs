using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartsManager : MonoBehaviour
{
    private DartsManager darts;
    public GameObject DartPrefab;
    public GameObject Dartboard;
    public int DartsOnBegin;
    public int DartsRemaining;
    bool CanSpawnDart { get => DartsRemaining > 0; }

    public Dart ActiveDart;
    public float ShakeFactor = 0.1f;
    public float ShakeAmount = 0.2f;

    private void Awake()
    {
        if(darts == null) { darts = this; } else { Destroy(this); }
        Dart.DartHit += OnDartHit;
    }

    private void Start()
    {
        InitializeDartGame();
    }

    private void Update()
    {
        transform.position += UnityEngine.Random.insideUnitSphere * ShakeFactor * Mathf.Sin(ShakeAmount);
    }


    void OnDartHit(Vector3 contactPoint)
    {
        Vector3 differenceVector = Dartboard.GetComponent<MeshCollider>().bounds.center - contactPoint;
        Debug.Log(Dartboard.GetComponent<MeshCollider>().bounds.center);
        CreateDart();
    }

    void InitializeDartGame()
    {
        DartsRemaining = DartsOnBegin;
        CreateDart();
    }

    void CreateDart()
    {
        if (CanSpawnDart)
        {
            ActiveDart = Instantiate(DartPrefab, transform).GetComponent<Dart>();
            DartsRemaining--;
        }
    }

    private void OnDestroy()
    {
        Dart.DartHit -= OnDartHit;
    }
}
