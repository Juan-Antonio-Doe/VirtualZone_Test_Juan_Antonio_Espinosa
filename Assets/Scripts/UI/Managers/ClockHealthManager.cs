using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClockHealthManager : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    [field: Header("__Clock Settings__")]
    [field: SerializeField] private HealthController npcOwner { get; set; }

    [field: Header("_Clock Wheels")]
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

#if UNITY_EDITOR
    /*
     * Suelo usar este método para automatizar la asignación de propiedades en el inspector en tiempo de edición.
     * Este código se ejecuta cuando se modifica un componente en el inspector. La propiedad `revalidateProperties`
     * sirve para evitar que el código se ejecute constantemente. Se podría considerar dicho bool como un trigger.
     */

    void OnValidate() {
        if (!Application.isPlaying) {

            // Código que evita que el OnValidate se ejecute en Prefab Stages provocando bucles en el editor.
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
            bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;

            if (!isValidPrefabStage && prefabConnected) {
                if (revalidateProperties)
                    AssingOnValidate();
            }
        }
    }

    void AssingOnValidate() {
        // Code to execute when revalidating properties
        if (npcOwner == null) {
            npcOwner = GetComponentInParent<HealthController>();
        }

        revalidateProperties = false;
    }
#endif

    void Start() {
        DisplayHealthOnClock();
    }

    #region Display Health Methods

    public void DisplayHealthOnClock() {
        // Update the clock wheels with the current health
        RefreshWheelsDisplay();
        // Update the needle position based on current health
        UpdateNeedleSmooth();
    }

    #region Display Wheel Number Methods
    /// <summary>
    /// Extracts each digit and rotates the corresponding wheel.
    /// </summary>
    private void RefreshWheelsDisplay() {
        // Decompose into hundreds, tens, units
        int[] digits = {
            npcOwner.Health / 100,
            (npcOwner.Health / 10) % 10,
            npcOwner.Health % 10
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

            float targetAngle = digits[i] * angleStep;

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
    private void UpdateNeedleSmooth() {
        // stop any existing spin
        if (smoothNeedleRoutine != null)
            StopCoroutine(smoothNeedleRoutine);

        float t = Mathf.Clamp01(npcOwner.Health / (float)npcOwner.MaxHealth);
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