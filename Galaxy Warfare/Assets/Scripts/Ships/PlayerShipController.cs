using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShipController : ShipController
{
    [Header("Components")]
    [Tooltip("The animator of the ship")] [SerializeField] 
    private Animator animator = null;
    private Rigidbody rb = null;
    [SerializeField] private Slider healthBar = null;

    [Header("Movement")] [Space]
    [Tooltip("In ms^-1")] [SerializeField] 
    private float movementSpeed = 10f;
    [SerializeField]
    private float hitPenaltySeconds = 2f;

    [Header("Input")] [Space]
    [Tooltip("In ms^-1")] [SerializeField] 
    private Vector2 inputRange = new Vector2(4.5f, 2f);
    [Tooltip("The reticle seen on screen")] [SerializeField] 
    private Transform uiReticle = null;
    [Tooltip("How far from the screen edges can the UI reticle go?")] [SerializeField] [Range(0, 100f)] 
    private float reticleEdgeDistPercent = 30f;
    [Tooltip("Used for calculating the reticle world position")] [SerializeField] 
    private LayerMask transparentFXMask = default;
    private Vector2 inputThrow = default;
    private bool isControlEnabled = true;

    [Header("Rotation")] [Space]
    [Tooltip("The rotation spee of the vehicle")] [SerializeField] 
    private float rotationSpeed = 25f;
    private Vector3 reticleWorldPosition = default;

    [Header("Combat")] [Space]
    [Tooltip("The bullet particle system.")] [SerializeField]
    private ParticleCannon[] cannons = null;
    [Tooltip("The speed of the bullets, in meters per second.")] [SerializeField]
    private float bulletSpeed = 500f;
    private Vector3 uiReticleInWorldPos = default;
    [Tooltip("The layers your hittable objects and enemies can be found in.")] [SerializeField]
    private LayerMask hittableObjectsMask = default;

    protected override void Awake()
    {
        base.Awake();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        rb = GetComponent<Rigidbody>();
        healthBar.value = currentHealth / maximumHealth;
    }

    private void Update()
    {
        if(isControlEnabled)
        {
            ProcessTranslation();
            ProcessRotation();
            ProcessAnimations();
            ProcessBullets();
        }
        ProcessReticle();
    }


    private void ProcessTranslation()
    {
        inputThrow = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical"));
        Vector2 offset = inputThrow * movementSpeed * Time.deltaTime;
        Vector2 rawNewPos = shipVisuals.localPosition.ToVector2() + offset;
        Vector2 clampedPos = rawNewPos.Clamp(-inputRange, inputRange);

        shipVisuals.localPosition = new Vector3(clampedPos.x, clampedPos.y, shipVisuals.localPosition.z);
    }

    private void ProcessRotation()
    {
        #region Towards reticle rotation
        Vector3 newDirection = reticleWorldPosition - shipVisuals.position;
        float rotationSpeed = this.rotationSpeed / 4 * Time.deltaTime;
        shipVisuals.rotation = Quaternion.LookRotation(Vector3.RotateTowards(shipVisuals.forward, newDirection, rotationSpeed, 0f));
        #endregion
    }

    private void ProcessAnimations()
    {
        Vector2 animationInput = new Vector2(animator.GetFloat("inputX"), animator.GetFloat("inputY"));
        animationInput = Vector2.Lerp(animationInput, Vector2.ClampMagnitude(inputThrow, 1f), rotationSpeed * Time.deltaTime);

        animator.SetFloat("inputX", animationInput.x);
        animator.SetFloat("inputY", animationInput.y);
    }

    private void ProcessReticle()
    {
        #region UI Reticle
        #region Literally the reticle you see on screen
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 screenCenter = screenSize / 2;
        float uiReticleLerp = reticleEdgeDistPercent / 100f;

        Vector2 uiReticleBottomLeft = Vector2.Lerp(Vector2.zero, screenCenter, uiReticleLerp);
        Vector2 uiReticleTopRight = Vector2.Lerp(screenSize, screenCenter, uiReticleLerp);

        uiReticle.position = CrossPlatformInputManager.mousePosition.ToVector2().Clamp(uiReticleBottomLeft, uiReticleTopRight);
        #endregion

        #region The exact position of the UI reticle in world position. The one towards which your vehicle shoots at.
        // Not to be confused with the World Reticle, which will be affected by a remap that limits the vehicle's on screen rotation capability.
        Ray uiReticleRay = Camera.main.ScreenPointToRay(uiReticle.position);

        if(Physics.Raycast(uiReticleRay, out RaycastHit hitObject, 1000f, hittableObjectsMask, QueryTriggerInteraction.Ignore))
        {
            uiReticleInWorldPos = hitObject.point;
            print(hitObject.transform.name);
        }
        else
        {
            foreach (RaycastHit hitInfo in Physics.RaycastAll(uiReticleRay, 100f, transparentFXMask, QueryTriggerInteraction.Collide))
            {
                if (hitInfo.transform.CompareTag("InputBox"))
                {
                    uiReticleInWorldPos = hitInfo.point;
                    break;
                }
            }
        }
        #endregion
        #endregion

        #region World Reticle, A.K.A. the one your vehicle looks at. Used for vehicle rotation.
        Vector2 worldReticleBottomLeft = Vector2.Lerp(Vector2.zero, screenCenter, 0.85f);
        Vector2 worldReticleTopRight = Vector2.Lerp(screenSize, screenCenter, 0.85f);
        Ray ray = Camera.main.ScreenPointToRay(uiReticle.position.ToVector2().Remap(uiReticleBottomLeft, uiReticleTopRight, worldReticleBottomLeft, worldReticleTopRight));

        foreach(RaycastHit hitInfo in Physics.RaycastAll(ray, 100f, transparentFXMask, QueryTriggerInteraction.Collide))
        {
            if (hitInfo.transform.CompareTag("InputBox"))
            {
                reticleWorldPosition = hitInfo.point;
                break;
            }
        }
        #endregion
    }
   
    private void ProcessBullets()
    {
        if(CrossPlatformInputManager.GetButton("Fire1"))
        {
            foreach(ParticleCannon cannon in cannons)
            {
                cannon.Fire(rb.velocity.magnitude + bulletSpeed, uiReticleInWorldPos);
            }
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        healthBar.value = currentHealth / maximumHealth;
        if (IsAlive)
        {
            StartCoroutine(OnHitCoroutine());
        }
    }

    private IEnumerator OnHitCoroutine()
    {
        isControlEnabled = false;
        yield return new WaitForSeconds(hitPenaltySeconds);
        isControlEnabled = true;
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        isControlEnabled = false;
    }
}