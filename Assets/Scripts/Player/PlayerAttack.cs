using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Reads player attack input and coordinates attack execution through the configured primary attack.
    /// Dedicated strictly to attack coordination (no physics queries, movement, aim, or damage math).
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Equipped Weapon")]
        [Tooltip("The primary attack component (must implement IPrimaryAttack).")]
        [FormerlySerializedAs("equippedWeapon")]
        [SerializeField] private MonoBehaviour primaryWeapon;

        [Header("Input Configuration")]
        [Tooltip("Reference to the project InputActionAsset (InputSystem_Actions).")]
        [SerializeField] private InputActionAsset inputActions;

        [Tooltip("Optional direct action reference for Player/Attack.")]
        [SerializeField] private InputActionReference attackActionReference;

        private IPrimaryAttack primaryAttack;
        private InputAction attackAction;
        private InputActionMap playerActionMap;

        public IPrimaryAttack PrimaryAttack => primaryAttack;
        public MeleeWeapon EquippedWeapon => primaryAttack as MeleeWeapon;

        private void Awake()
        {
            ResolveWeapon();
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

        private void ResolveWeapon()
        {
            if (primaryWeapon != null)
            {
                if (primaryWeapon is IPrimaryAttack attack)
                {
                    primaryAttack = attack;
                }
                else
                {
                    Debug.LogError($"[PlayerAttack] Assigned primaryWeapon '{primaryWeapon.name}' ({primaryWeapon.GetType().Name}) does not implement IPrimaryAttack.", this);
                    primaryAttack = null;
                }
                return;
            }

            // Fallback: check children on this character hierarchy only (no scene-wide search)
            primaryAttack = GetComponentInChildren<IPrimaryAttack>();
            if (primaryAttack is MonoBehaviour mb)
            {
                primaryWeapon = mb;
            }
        }

        private void InitializeInput()
        {
            if (attackActionReference != null)
            {
                attackAction = attackActionReference.action;
                return;
            }

            if (inputActions != null)
            {
                playerActionMap = inputActions.FindActionMap("Player");
                if (playerActionMap != null)
                {
                    attackAction = playerActionMap.FindAction("Attack");
                    return;
                }
            }

            if (TryGetComponent<PlayerInput>(out var playerInput) && playerInput.actions != null)
            {
                playerActionMap = playerInput.actions.FindActionMap("Player");
                if (playerActionMap != null)
                {
                    attackAction = playerActionMap.FindAction("Attack");
                    return;
                }
            }

            // Fallback: search project for input actions
            var loadedActions = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            foreach (var asset in loadedActions)
            {
                if (asset.name == "InputSystem_Actions")
                {
                    inputActions = asset;
                    playerActionMap = inputActions.FindActionMap("Player");
                    if (playerActionMap != null)
                    {
                        attackAction = playerActionMap.FindAction("Attack");
                        return;
                    }
                }
            }
        }

        private void EnableInput()
        {
            if (attackAction != null)
            {
                attackAction.performed += OnAttackPerformed;
                attackAction.Enable();
            }
            else if (playerActionMap != null)
            {
                playerActionMap.Enable();
                attackAction = playerActionMap.FindAction("Attack");
                if (attackAction != null)
                {
                    attackAction.performed += OnAttackPerformed;
                    attackAction.Enable();
                }
            }
        }

        private void DisableInput()
        {
            if (attackAction != null)
            {
                attackAction.performed -= OnAttackPerformed;
                attackAction.Disable();
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            TryAttack();
        }

        /// <summary>
        /// Requests the configured primary weapon to perform an attack.
        /// Part of the standard gameplay API; also used for deterministic verification.
        /// </summary>
        /// <returns>True if the attack succeeded; false if blocked by cooldown, pause, or missing weapon.</returns>
        public bool TryAttack()
        {
            if (Time.timeScale <= 0f)
            {
                return false;
            }

            if (primaryAttack == null)
            {
                ResolveWeapon();
            }

            if (primaryAttack != null)
            {
                return primaryAttack.TryAttack();
            }

            return false;
        }

        /// <summary>
        /// Configures the primary attack component. Retained for setup tooling and test initialization.
        /// </summary>
        public void SetPrimaryWeapon(IPrimaryAttack weapon)
        {
            primaryAttack = weapon;
            primaryWeapon = weapon as MonoBehaviour;
        }

        /// <summary>
        /// Retained backward-compatibility setter for existing setup tooling.
        /// </summary>
        public void SetEquippedWeapon(MeleeWeapon weapon)
        {
            SetPrimaryWeapon(weapon);
        }
    }
}
