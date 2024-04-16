using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class ShipControl : MonoBehaviour
{
    static ShipControl instance = null;
    public const string RamField = "ramming";
    public const string HorizontalField = "horizontal";
    public const string VerticalField = "vertical";
    public const string HitTrigger = "hit";
    public const string KilledTrigger = "kill";
    public const string FlightTowardsTarget = "Forward";
    public const string FlightAwayFromTarget = "Reverse";

    public enum FlightMode
    {
        ToTheTarget,
        AwayFromTheTarget
    }

    [SerializeField]
    Transform camera = null;
    [SerializeField]
    Collider hitCollider = null;
    [SerializeField]
    Collider ramCollider = null;

    [Header("Movement stats")]
    [SerializeField]
    [Range(0, 1000)]
    float forceTowardsTarget = 100f;
    [SerializeField]
    [Range(0, 100)]
    float forceRamming = 2000f;
    [SerializeField]
    [Range(0, 50)]
    float forceSidewaysSpeed = 10f;
    [SerializeField]
    [Range(0, 30)]
    float rotateLerp = 1f;

    [Header("Conditions")]
    [SerializeField]
    bool rammingOn = false;
    [SerializeField]
    [Range(0, 5)]
    float reverseFor = 2f;
    [SerializeField]
    [Range(0, 5)]
    float invincibleFor = 2f;
    [SerializeField]
    [Range(1, 100)]
    int maxHealth = 10;
    [SerializeField]
    [Range(1, 50)]
    int displayDangerBelow = 3;
    [SerializeField]
    [Range(0, 20)]
    int enemyHitDamage = 5;
    [SerializeField]
    float predictiveMultiplierNormal = 10f;
    [SerializeField]
    float predictiveMultiplierRam = 10f;
    [SerializeField]
    [Range(0, 1)]
    float rammingDefenseMultiplier = 0.5f;

    [Header("Drill Stats")]
    [SerializeField]
    [Range(0, 10)]
    float drillMax = 10f;
    [SerializeField]
    [Range(0, 3)]
    float drillDepletionRate = 1f;
    [SerializeField]
    [Range(0, 3)]
    float drillCooldownSmall = 1f;
    [SerializeField]
    [Range(0, 3)]
    float drillCooldownLong = 3f;
    [SerializeField]
    [Range(0, 3)]
    float drillRecoverRate = 1f;
    [SerializeField]
    [Range(0, 0.5f)]
    float pauseKillLength = 0.1f;
    [SerializeField]
    [Range(0, 0.5f)]
    float pauseHurtLength = 0.05f;
    [SerializeField]
    [Range(0, 3)]
    float drillRecoverAfterKill = 0.25f;

    [Header("Menus")]
    [SerializeField]
    Text flightModeLabel;
    [SerializeField]
    Slider lifeBar;
    [SerializeField]
    Slider drillBar;
    [SerializeField]
    Text emptyDrill;
    [SerializeField]
    Text dangerHealth;
    [SerializeField]
    LevelCompleteMenu completeMenu;
    [SerializeField]
    LevelFailedMenu deadMenu;

    [Header("Target")]
    [SerializeField]
    GameObject targetReticle = null;
    [SerializeField]
    Text distanceLabel = null;
    [SerializeField]
    Text enemyNameLabel = null;
    [SerializeField]
    Text enemyNumbersLabel = null;

    [Header("Sound")]
    [SerializeField]
    AudioMutator jetSound = null;
    [SerializeField]
    AudioMutator hitSound = null;
    [SerializeField]
    AudioMutator emptySound = null;
    [SerializeField]
    AudioMutator refillSound = null;
    [SerializeField]
    AudioMutator dangerSound = null;
    [SerializeField]
    AudioSource successSound = null;
    [SerializeField]
    AudioSource failSound = null;

    [Header("Particles")]
    [SerializeField]
    ParticleSystem ramParticles = null;
    [SerializeField]
    Animator cameraAnimation = null;
    [SerializeField]
    PooledExplosion hitExplosion = null;

    [Header("Credits")]
    [SerializeField]
    bool allowChangingTargets = true;

    Rigidbody bodyCache = null;
    Animator animatorCache = null;
    Vector2 controlInput = Vector2.zero;
    Vector3 targetToShip = Vector3.zero,
        moveDirection = Vector3.zero,
        forceCache = Vector3.zero;
    Quaternion currentRotation = Quaternion.identity;
    Quaternion lookRotation = Quaternion.identity;
    FlightMode direction = FlightMode.ToTheTarget;
    float timeCollisionStarted = -1f,
        timeInvincible = -1f,
        drillCurrent = 0,
        timeLastDrilled = 0,
        pauseStartedRealTime = -1f,
        pauseFor = 1f;
    int currentHealth = 0;
    
    public static ShipControl Instance
    {
        get
        {
            return instance;
        }
    }

    public static Transform TransformInfo
    {
        get
        {
            return instance.transform;
        }
    }

    Rigidbody Body
    {
        get
        {
            if(bodyCache == null)
            {
                bodyCache = GetComponent<Rigidbody>();
            }
            return bodyCache;
        }
    }

    Animator Animate
    {
        get
        {
            if (animatorCache == null)
            {
                animatorCache = GetComponent<Animator>();
            }
            return animatorCache;
        }
    }

    public bool IsRamming
    {
        get
        {
            return rammingOn;
        }
        private set
        {
            if (rammingOn != value)
            {
                rammingOn = value;
                Animate.SetBool(RamField, rammingOn);
                cameraAnimation.SetBool(RamField, rammingOn);
                hitCollider.gameObject.SetActive(rammingOn == false);
                ramCollider.gameObject.SetActive(rammingOn == true);
                if(rammingOn == true)
                {
                    jetSound.Play();
                    ramParticles.Play();
                }
                else
                {
                    jetSound.Stop();
                    ramParticles.Stop();
                }
            }
        }
    }

    public Vector3 TargetToShip
    {
        get
        {
            return targetToShip;
        }
    }

    public int CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            // Check invincibility frames
            if ((Time.time - timeInvincible) < invincibleFor)
            {
                return;
            }

            // Update health
            int newHealth = Mathf.Clamp(value, 0, maxHealth);
            if(currentHealth != newHealth)
            {
                // If decreasing for health, flag for invincibility
                if(newHealth < currentHealth)
                {
                    timeInvincible = Time.time;
                    hitSound.Play();
                    cameraAnimation.SetTrigger(HitTrigger);
                }

                // Setup health
                currentHealth = newHealth;

                // Setup UI
                lifeBar.value = currentHealth;
                if(currentHealth <= displayDangerBelow)
                {
                    dangerHealth.enabled = true;
                    dangerSound.Play();
                }
                else
                {
                    dangerHealth.enabled = false;
                }

                // Check for death
                if (currentHealth <= 0)
                {
                    // FIXME: do something on death!
                    Finish(false);
                }
            }
        }
    }

    public FlightMode FlightDirection
    {
        get
        {
            return direction;
        }
        set
        {
            if(direction != value)
            {
                direction = value;
                if(direction == FlightMode.ToTheTarget)
                {
                    flightModeLabel.text = FlightTowardsTarget;
                }
                else
                {
                    flightModeLabel.text = FlightAwayFromTarget;
                }
            }
        }
    }

    float CurrentDrill
    {
        get
        {
            return drillCurrent;
        }
        set
        {
            drillCurrent = value;
            emptyDrill.enabled = (drillCurrent < 0);
            drillBar.value = Mathf.Clamp(value, 0, drillMax);
            if(drillCurrent < 0)
            {
                if (emptySound.Audio.isPlaying == false)
                {
                    emptySound.Play();
                }
            }
        }
    }

    void Start()
    {
        instance = this;
        Time.timeScale = 0;

        // Setup stats
        currentHealth = maxHealth;
        drillCurrent = drillMax;
        timeLastDrilled = -1;
        timeCollisionStarted = -1f;
        timeInvincible = -1f;

        // Setup UI
        lifeBar.wholeNumbers = true;
        lifeBar.minValue = 0;
        lifeBar.maxValue = maxHealth;
        lifeBar.value = currentHealth;

        drillBar.wholeNumbers = false;
        drillBar.minValue = 0;
        drillBar.maxValue = drillMax;
        drillBar.value = currentHealth;

        flightModeLabel.text = FlightTowardsTarget;
        
        dangerHealth.enabled = false;
        emptyDrill.enabled = false;
        ramParticles.Stop();
    }

    void Update ()
    {
        // Grab controls
        controlInput.x = CrossPlatformInputManager.GetAxis("Horizontal");
        controlInput.y = CrossPlatformInputManager.GetAxis("Vertical");
        if(FlightDirection == FlightMode.AwayFromTheTarget)
        {
            controlInput.x *= -1f;
        }
        IsRamming = CheckIfRamming();

        // Figure out the direction to look at
        targetToShip = (transform.position + (transform.forward * 10f));
        moveDirection = targetToShip - transform.position;
        lookRotation = Quaternion.LookRotation(moveDirection);

        if((pauseStartedRealTime > 0) && ((Time.unscaledTime - pauseStartedRealTime) > pauseFor))
        {
            Time.timeScale = 1;
            pauseStartedRealTime = -1f;
        }
    }

    void FixedUpdate()
    {
        // Add rotation
        Body.rotation = Quaternion.Slerp(Body.rotation, lookRotation, (Time.deltaTime * rotateLerp));

        // Add controls force
        forceCache.x = controlInput.x * forceSidewaysSpeed;
        forceCache.y = controlInput.y * forceSidewaysSpeed;
        forceCache.z = 0;
        Body.AddRelativeForce(forceCache, ForceMode.Impulse);

        // Add forward force
        if (IsRamming == true)
        {
            forceCache = transform.forward * forceRamming;
            Body.AddForce(forceCache, ForceMode.Impulse);
        }
        else
        {
            forceCache = transform.forward * forceTowardsTarget;
            Body.AddForce(forceCache, ForceMode.Force);
        }
    }

    void OnCollisionEnter(Collision info)
    {
        // Handle collision logic here
    }

    void Finish(bool complete)
    {
        // Handle finish logic here
    }

    bool CheckIfRamming()
    {
        // Implement ramming logic here
        return false;
    }

    private void Pause(float length)
    {
        pauseFor = length;

        // Pause for a short bit
        Time.timeScale = 0;
        pauseStartedRealTime = Time.unscaledTime;
    }
}
