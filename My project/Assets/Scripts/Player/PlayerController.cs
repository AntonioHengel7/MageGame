using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
  [Header("Look")]
  [SerializeField] Transform camRig;
  [SerializeField] float mouseSensitivity = 1.0f;
  [SerializeField] float pitchMin = -80f, pitchMax = 80f;

  [Header("Move")]
  [SerializeField] float moveSpeed = 8f;
  [SerializeField] float jumpSpeed = 7f;
  [SerializeField] float gravity = -20f;

  [Header("Dash")]
  [SerializeField] float dashSpeed = 18f;
  [SerializeField] float dashDuration = 0.15f;
  [SerializeField] float dashCooldown = 0.6f;

  [Header("Input Actions")]
  [SerializeField] InputActionReference moveAction;     // Player/Move (Vector2)
  [SerializeField] InputActionReference lookAction;     // Player/Look (Vector2)
  [SerializeField] InputActionReference jumpAction;     // Player/Jump (Button)
  [SerializeField] InputActionReference dashAction;     // Player/Dash (Button)

  CharacterController cc;
  float yaw, pitch;
  Vector3 velocity;
  bool isDashing;
  float dashTimer, dashCdTimer;

  void Awake() {
    cc = GetComponent<CharacterController>();
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    yaw = transform.eulerAngles.y;
  }

  void OnEnable() {
    moveAction?.action.Enable();
    lookAction?.action.Enable();
    jumpAction?.action.Enable();
    dashAction?.action.Enable();
  }
  void OnDisable() {
    moveAction?.action.Disable();
    lookAction?.action.Disable();
    jumpAction?.action.Disable();
    dashAction?.action.Disable();
  }

  void Update() {
    Look();
    MoveAndDash();
  }

  void Look() {
    Vector2 look = lookAction ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;
    // mouse delta is already per-frame; scale it a bit
    float mx = look.x * mouseSensitivity;
    float my = look.y * mouseSensitivity;

    yaw += mx;
    pitch = Mathf.Clamp(pitch - my, pitchMin, pitchMax);

    transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    if (camRig) camRig.localRotation = Quaternion.Euler(pitch, 0f, 0f);
  }

  void MoveAndDash() {
    Vector2 mv = moveAction ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
    Vector3 fwd = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
    Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
    Vector3 input = (right * mv.x + fwd * mv.y);
    input = Vector3.ClampMagnitude(input, 1f);

    dashCdTimer -= Time.deltaTime;
    Vector3 planar = input * (isDashing ? dashSpeed : moveSpeed);

    if (cc.isGrounded) {
      velocity.y = -2f;
      if (jumpAction && jumpAction.action.triggered) velocity.y = jumpSpeed;
      if (!isDashing && dashAction && dashAction.action.triggered && dashCdTimer <= 0f) StartDash();
    } else {
      velocity.y += gravity * Time.deltaTime;
    }

    cc.Move((planar + new Vector3(0, velocity.y, 0)) * Time.deltaTime);
    if (isDashing) {
      dashTimer -= Time.deltaTime;
      if (dashTimer <= 0f) isDashing = false;
    }
  }

  void StartDash() {
    isDashing = true;
    dashTimer = dashDuration;
    dashCdTimer = dashCooldown;
  }
}
