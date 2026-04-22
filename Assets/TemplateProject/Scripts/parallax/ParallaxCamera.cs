using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private const float PositionThreshold = 0.0001f;
    private float oldPosition;

    private void OnEnable()
    {
        oldPosition = transform.position.x;
    }

    private void Update()
    {
        float delta = transform.position.x - oldPosition;

        if (Mathf.Abs(delta) > PositionThreshold)
        {
            onCameraTranslate?.Invoke(delta);
            oldPosition = transform.position.x;
        }
    }
}