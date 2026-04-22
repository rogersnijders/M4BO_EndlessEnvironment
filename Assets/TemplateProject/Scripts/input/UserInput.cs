using UnityEngine;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    [HideInInspector] public Controls controls;
    [HideInInspector] public Vector2 moveInput;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            controls = new Controls();
            controls.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            controls.Movement.Move.canceled  += ctx => moveInput = Vector2.zero;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            controls?.Dispose();
            instance = null;
        }
    }
}