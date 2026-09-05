using UnityEngine;
using UnityEngine.InputSystem;
using DungeonRoguelite.Weapons;

namespace DungeonRoguelite.Player
{
    /// <summary>
    /// Reads player attack input and coordinates attack execution through the equipped weapon.
    /// Dedicated strictly to attack coordination (no physics queries, movement, aim, or damage math).
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Equipped Weapon")]
        [Tooltip("The melee weapon currently equipped.")]
        [SerializeField] private MeleeWeapon equippedWeapon;

        [Header("Input Configuration")]
        [Tooltip("Reference to the project InputActionAsset (InputSystem_Actions).")]
        [SerializeField] private InputActionAsset inputActions;

        [Tooltip("Optional direct action reference for Player/Attack.")]
        [SerializeField] private InputActionReference attackActionReference;

        private InputAction attackAction;
        private InputActionMap playerActionMap;

        public MeleeWeapon EquippedWeapon => equippedWeapon;

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
            if (equippedWeapon == null)
            {
                equippedWeapon = GetComponentInChildren<MeleeWeapon>();
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
        /// Requests the equipped weapon to perform an attack.
        /// Part of the standard gameplay API; also used for deterministic verification.
        /// </summary>
        /// <returns>True if the attack succeeded; false if blocked by cooldown or missing weapon.</returns>
        public bool TryAttack()
        {
            if (equippedWeapon == null)
            {
                ResolveWeapon();
            }

            if (equippedWeapon != null)
            {
                return equippedWeapon.TryAttack();
            }

            return false;
        }

        /// <summary>
        /// Sets or swaps the currently equipped melee weapon.
        /// </summary>
        public void SetEquippedWeapon(MeleeWeapon weapon)
        {
            equippedWeapon = weapon;
        }
    }
}
