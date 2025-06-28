using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthController : MonoBehaviour {

    [field: Header("__Health Settings__")]
    [field: SerializeField, Range(0, 999)] private int maxHealth { get; set; } = 999;
    [field: SerializeField, ReadOnlyField] private int health { get; set; } = 100;
    [field: SerializeField] public bool isDead { get; private set; } = false;

    [field: Header("__Clock Settings__")]
    [field: Header ("_Clock Wheels")]
    [field: Header("Digit Wheels (0–9)")]
    // Assign exactly three Transforms: hundreds, tens, units
    [field: SerializeField] private Transform[] digitWheels { get; set; } = new Transform[3];

    [field: Header("Rotation Settings")]
    // Angle to rotate per digit step (e.g. –36° between 0 and 1)
    [field: SerializeField] private float angleStep { get; set; } = -36f;

    [field: Header("Rotation Settings"), 
        Tooltip("How fast the wheel spins (degrees per second)")]
    [field: SerializeField] private float wheelRotationSpeed { get; set; } = 180f;

    private int[] lastDigits { get; set; } = new int[3] { -1, -1, -1 };
    private Coroutine[] wheelRoutines { get; set; } = new Coroutine[3];

    [field: Header("_Clock Needle")]
    [field: SerializeField] private Transform needle { get; set; }

    [field: Tooltip("Min and max Y-angles for the needle")]
    [field: SerializeField] private float minY = -90f;
    [field: SerializeField] private float maxY = 90f;

    [field: Header("Smooth Rotation"),
        Tooltip("Degrees per second when spinning")]
    [field: SerializeField] private float needleRotationSpeed { get; set; } = 180f;

    private Coroutine smoothNeedleRoutine { get; set; }

    [field: Header("Events")]
    [field: SerializeField] private UnityEvent onTakeDamage { get; set; }
    [field: SerializeField] private UnityEvent onHeal { get; set; }
    [field: SerializeField] private UnityEvent onDeath { get; set; }
    [field: SerializeField] private UnityEvent onResurrect { get; set; }

    void Start() {
        health = maxHealth;
        DisplayHealthOnClock();
    }

    #region Damage Methods

    public void TakeDamage(InputField damageInput) {
        if (damageInput != null) {
            if (string.IsNullOrEmpty(damageInput.text)) {
                Debug.LogWarning("Damage input field is empty");
                return;
            }
        }
        else {
            Debug.LogWarning("Damage input field is not assigned");
            return;
        }


        if (int.TryParse(damageInput.text, out int damage)) {
            if (damage < 0) {
                Debug.LogWarning("Damage cannot be negative");
                return;
            }

            TakeDamage(damage);
        } else {
            Debug.LogWarning("Invalid damage input");
        }
    }

    public void TakeDamage(int damage) {
        if (health > 0) {
            health -= damage;

            if (health <= 0) {
                Die();
                return;
            }

            onTakeDamage?.Invoke();
            DisplayHealthOnClock();
            Debug.Log($"<color=red>Took {damage} damage</color>, current health: {health}");
        }
    }

    #endregion

    #region Heal Methods

    public void Heal(InputField healInput) {
        if (healInput != null) {
            if (string.IsNullOrEmpty(healInput.text)) {
                Debug.LogWarning("Heal input field is empty");
                return;
            }
        }
        else {
            Debug.LogWarning("Heal input field is not assigned");
            return;
        }

        if (int.TryParse(healInput.text, out int healAmount)) {
            if (healAmount < 0) {
                Debug.LogWarning("Heal amount cannot be negative");
                return;
            }
            Heal(healAmount);
        } else {
            Debug.LogWarning("Invalid heal input");
        }
    }

    public void Heal(int amount) {
        if (health < maxHealth) {
            if (isDead) {
                Resurrect(amount);
                return;
            }

            health += amount;
            onHeal?.Invoke();
            DisplayHealthOnClock();

            if (health > maxHealth) {
                health = maxHealth;
            }
            Debug.Log($"<color=green>Healed {amount} health</color>, current health: {health}");
        }
    }

    #endregion

    #region Death Methods

    void Resurrect(int amount) {
        isDead = false;
        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }

        onResurrect?.Invoke();
        DisplayHealthOnClock();
        Debug.Log("<color=green>Resurrected</color>, current health: " + health);
    }

    void Die() {
        isDead = true;
        health = 0;

        onDeath?.Invoke();
        DisplayHealthOnClock();
        Debug.Log("<color=red>Character is dead</color>");
    }

    #endregion

    #region Display Health Methods

    private void DisplayHealthOnClock() {
        // Update the clock wheels with the current health
        RefreshDisplay();
        // Update the needle position based on current health
        UpdateNeedleSmooth();
    }

    #region Display Wheel Number Methods
    /// <summary>
    /// Extracts each digit and rotates the corresponding wheel.
    /// </summary>
    public void RefreshDisplay() {
        // Decompose into hundreds, tens, units
        int[] digits = {
            health / 100,
            (health / 10) % 10,
            health % 10
        };

        // for each wheel, stop any prior spin and start a new smooth spin
        for (int i = 0; i < digitWheels.Length; i++) {
            var wheel = digitWheels[i];
            if (wheel == null) continue;

            if (digits[i] == lastDigits[i])
                continue;   // same digit, no need to rotate

            float currentAngle = wheel.localEulerAngles.x;
            // convert euler 0–360 to a signed angle if >180
            if (currentAngle > 180f) currentAngle -= 360f;

            float targetAngle = digits[i] * -36f;

            // stop old coroutine
            if (wheelRoutines[i] != null)
                StopCoroutine(wheelRoutines[i]);

            // start new one
            wheelRoutines[i] = StartCoroutine(
                SmoothRotate(wheel, currentAngle, targetAngle, wheelRotationSpeed));
        }

        digits.CopyTo(lastDigits, 0);
    }

    IEnumerator SmoothRotate(Transform wheel, float from, float to, float speed) {
        // Spin until we reach exactly 'to'
        float angle = from;
        while (!Mathf.Approximately(angle, to)) {
            // MoveTowardsAngle chooses the shortest path
            angle = Mathf.MoveTowardsAngle(
                angle,
                to,
                wheelRotationSpeed * Time.deltaTime
            );
            wheel.localRotation = Quaternion.Euler(angle, 0f, 0f);
            yield return null;
        }
        // Guarantee exact final orientation
        wheel.localRotation = Quaternion.Euler(to, 0f, 0f);
    }

    #endregion

    #region Display Clock Needle Methods

    /// <summary>
    /// Smoothly spins the needle to the correct angle.
    /// </summary>
    public void UpdateNeedleSmooth() {
        // stop any existing spin
        if (smoothNeedleRoutine != null)
            StopCoroutine(smoothNeedleRoutine);

        float t = Mathf.Clamp01(health / (float)maxHealth);
        float targetY = Mathf.Lerp(minY, maxY, t);
        smoothNeedleRoutine = StartCoroutine(RotateYAxis(needle, targetY, needleRotationSpeed));
    }

    IEnumerator RotateYAxis(Transform t, float targetAngle, float speed) {
        // get current Y in –180..+180 range
        float current = t.localEulerAngles.y;
        if (current > 180f) current -= 360f;

        // spin until we hit the target
        while (!Mathf.Approximately(current, targetAngle)) {
            current = Mathf.MoveTowardsAngle(
                current,
                targetAngle,
                speed * Time.deltaTime
            );
            Vector3 e = t.localEulerAngles;
            e.y = current;
            t.localEulerAngles = e;
            yield return null;
        }

        // ensure exact final
        Vector3 end = t.localEulerAngles;
        end.y = targetAngle;
        t.localEulerAngles = end;
    }

    #endregion

    #endregion
}