using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public ParallaxCamera parallaxCamera;
    private List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    private void OnEnable()
    {
        if (parallaxCamera == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null)
                parallaxCamera = mainCam.GetComponent<ParallaxCamera>();
        }

        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;

        SetLayers();
    }

    private void OnDisable()
    {
        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate -= Move;
    }

    public void SetLayers()
    {
        parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }

    private void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}