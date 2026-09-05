using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Handles player movement on the X/Z plane using a CharacterController and the Unity Input System.
    /// Dedicated strictly to movement logic (no combat, aim, or health responsibilities).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Configuration")]
        [Tooltip("Horizontal movement speed in meters per second.")]
        [SerializeField] private float moveSpeed = 6f;

        [Tooltip("Downward acceleration applied to ensure CharacterController grounding stability.")]
        [SerializeField] private float gravity = 20f;

        [Header("Input Configuration")]
        [Tooltip("Reference to the project InputActionAsset (InputSystem_Actions).")]
        [SerializeField] private InputActionAsset inputActions;

        [Tooltip("Optional direct action reference for Player/Move.")]
        [SerializeField] private InputActionReference moveActionReference;

        private CharacterController characterController;
        private InputAction moveAction;
        private InputActionMap playerActionMap;
        private float verticalVelocity;
        private Vector2 currentInput;
        private bool testInputOverrideActive;
        private Vector2 testInputOverride;

        public float MoveSpeed => moveSpeed;
        public float Gravity => gravity;
        public bool IsGrounded => characterController != null && characterController.isGrounded;
        public Vector2 CurrentInput => testInputOverrideActive ? testInputOverride : currentInput;
        public Vector3 Velocity => characterController != null ? characterController.velocity : Vector3.zero;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            InitializeInput();
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void Update()
        {
            float dt = Time.deltaTime > 0f ? Time.deltaTime : 0.02f;
            StepMovement(dt);
        }

        /// <summary>
        /// Explicitly steps input reading and character movement.
        /// Useful for fixed update loops, deterministic testing, or simulation harnesses.
        /// </summary>
        public void StepMovement(float dt = 0.02f)
        {
            ReadInput();
            MoveCharacter(dt);
        }

        private void InitializeInput()
        {
            if (moveActionReference != null)
            {
                moveAction = moveActionReference.action;
                return;
            }

            if (inputActions != null)
            {
                playerActionMap = inputActions.FindActionMap("Player");
                if (playerActionMap != null)
                {
                    moveAction = playerActionMap.FindAction("Move");
                    return;
                }
            }

            if (TryGetComponent<PlayerInput>(out var playerInput) && playerInput.actions != null)
            {
                playerActionMap = playerInput.actions.FindActionMap("Player");
                if (playerActionMap != null)
                {
                    moveAction = playerActionMap.FindAction("Move");
                    return;
                }
            }

            // Fallback: locate InputSystem_Actions asset in loaded assets
            var loadedAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            foreach (var asset in loadedAssets)
            {
                if (asset.name == "InputSystem_Actions")
                {
                    inputActions = asset;
                    playerActionMap = asset.FindActionMap("Player");
                    if (playerActionMap != null)
                    {
                        moveAction = playerActionMap.FindAction("Move");
                    }
                    break;
                }
            }
        }

        private void EnableInput()
        {
            if (playerActionMap != null && !playerActionMap.enabled)
            {
                playerActionMap.Enable();
            }
            else if (moveAction != null && !moveAction.enabled)
            {
                moveAction.Enable();
            }
        }

        private void DisableInput()
        {
            if (playerActionMap != null && playerActionMap.enabled)
            {
                playerActionMap.Disable();
            }
            else if (moveAction != null && moveAction.enabled)
            {
                moveAction.Disable();
            }
        }

        private void ReadInput()
        {
            if (testInputOverrideActive)
            {
                currentInput = testInputOverride;
                return;
            }

            if (moveAction != null)
            {
                currentInput = moveAction.ReadValue<Vector2>();
            }
            else
            {
                currentInput = Vector2.zero;
            }
        }

        private void MoveCharacter(float dt)
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
                if (characterController == null) return;
            }

            Vector2 input = CurrentInput;

            // Ensure diagonal movement does not exceed cardinal movement speed
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            // WASD movement operates strictly on the X/Z horizontal plane
            Vector3 horizontalMove = new Vector3(input.x, 0f, input.y) * moveSpeed;

            // Vertical grounding & gravity for CharacterController stability
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity -= gravity * dt;
            }

            Vector3 totalMove = horizontalMove;
            totalMove.y = verticalVelocity;

            characterController.Move(totalMove * dt);
        }

        /// <summary>
        /// Configure movement speed from code or testing suites.
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// Sets a simulated movement input for verification and automated tests.
        /// </summary>
        public void SetTestInputOverride(Vector2? inputOverride)
        {
            if (inputOverride.HasValue)
            {
                testInputOverrideActive = true;
                testInputOverride = inputOverride.Value;
            }
            else
            {
                testInputOverrideActive = false;
                testInputOverride = Vector2.zero;
            }
        }
    }
}
