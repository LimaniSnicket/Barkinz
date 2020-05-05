using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PlacedObject : MonoBehaviour
{
    public PlaceableObject ObjectInformation;
    public Vector2Int GridPosition;
    protected Tile PlacedOn;
    Renderer objectRenderer { get => GetComponent<Renderer>(); }
    MeshFilter objectFilter { get => GetComponent<MeshFilter>(); }
    public GameObject instantiateParticle;

    public void InitializePlacedObject()
    {
        InitializePlacedObject(Vector2Int.zero);
    }

    public void InitializePlacedObject(Vector2Int gridPosition)
    {
        if (ObjectInformation != null)
        {
            if (ObjectInformation.ObjectTexture != null)
            {
                objectRenderer.material.mainTexture = ObjectInformation.ObjectTexture;
            }
            if (ObjectInformation.ObjectMesh != null) 
            {
                objectFilter.mesh = ObjectInformation.ObjectMesh;
            }
        }
        GridPosition = gridPosition;
        Instantiate(instantiateParticle, transform);
    }
    bool rotating;

    public void RotateObject()
    {
        if (rotating) { return; }
        Vector3 rotationEulers = transform.eulerAngles + new Vector3(0, 90, 0);
        StartCoroutine(Rotate(rotationEulers));
    }

    public void Store()
    {
        Destroy(gameObject);
    }

    IEnumerator Rotate(Vector3 euler)
    {
        rotating = true;
        transform.DOMove(transform.position + Vector3.up, .25f);
        yield return new WaitForSeconds(0.25f);
        transform.DORotate(euler, 0.5f);
        yield return new WaitForSeconds(0.5f);
        transform.DOMove(transform.position - Vector3.up, 0.15f);
        yield return new WaitForSeconds(.15f);
        rotating = false;
    }

    public void InitializePlacedObject(Vector2Int gridPosition, PlaceableObject obj)
    {
        ObjectInformation = obj;
        InitializePlacedObject(gridPosition);
    }
}
