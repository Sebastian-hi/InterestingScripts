using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTexture : MonoBehaviour
{
    public float speed;
 
    [SerializeField]
    private Renderer bgRenderer;

    private enum ScrollDirection
    {
        Left, 
        Right,
        Forward,
        Back
    }

    [SerializeField] private ScrollDirection _currentScrollDir = ScrollDirection.Left;
 
    void Update()
    {
        if (_currentScrollDir == ScrollDirection.Left)
        {
            bgRenderer.material.mainTextureOffset += new Vector2(-speed * Time.deltaTime, 0);
        }
        else if (_currentScrollDir == ScrollDirection.Right)
        {
            bgRenderer.material.mainTextureOffset += new Vector2(speed * Time.deltaTime, 0);
        }
        else if (_currentScrollDir == ScrollDirection.Forward)
        {
            bgRenderer.material.mainTextureOffset += new Vector2(0, speed * Time.deltaTime);
        }
        else if (_currentScrollDir == ScrollDirection.Back)
        {
            bgRenderer.material.mainTextureOffset += new Vector2(0, -speed * Time.deltaTime);
        }
    }
}
