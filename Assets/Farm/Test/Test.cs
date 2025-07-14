using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Material material;
    public Vector3 origin;
    public Vector3 currentPosition;
    public float boundarySize = 1.5f;

    void Start()
    {
        origin = transform.position;
    }

    void Update()
    {
        currentPosition = transform.position - origin;
        
        // Smooth wrapping using modulo operation
        var posX = WrapPosition(currentPosition.x, boundarySize);
        var posY = WrapPosition(currentPosition.y, boundarySize);
        
        material.SetVector("_Pos", new Vector2(-posX, -posY));
    }
    
    private float WrapPosition(float value, float boundary)
    {
        float range = boundary * 2f;
        return ((value + boundary) % range) - boundary;
    }
}
