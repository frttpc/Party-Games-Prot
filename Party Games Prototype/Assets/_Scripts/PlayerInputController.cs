using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private InputActionAsset playerInputActions;
    private InputActionMap playerActionMap;
    public InputAction move { get; private set; }
    public InputAction duck { get; private set; }

    public bool dashIsPressed { get; set; } = false;
    public bool jumpIsPressed { get; set; } = false;

    private void Awake()
    {
        playerInputActions = GetComponent<PlayerInput>().actions;
        playerActionMap = playerInputActions.FindActionMap("Player");
    }

    private void OnEnable()
    {
        playerActionMap.Enable();
        move = playerActionMap.FindAction("Movement");
        duck = playerActionMap.FindAction("Duck");
        playerActionMap.FindAction("Jump").performed += JumpIsPressed;
        playerActionMap.FindAction("Dash/Push").performed += DashIsPressed;
    }

    private void OnDisable()
    {
        playerActionMap.Disable();
        playerActionMap.FindAction("Jump").performed -= JumpIsPressed;
        playerActionMap.FindAction("Dash/Push").performed -= DashIsPressed;
    }

    private void JumpIsPressed(InputAction.CallbackContext context) => jumpIsPressed = context.performed;

    private void DashIsPressed(InputAction.CallbackContext context) => dashIsPressed = context.performed;
}
