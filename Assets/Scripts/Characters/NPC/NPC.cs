using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NPC : MonoBehaviour {

    [field: Header("Auto-Assigned Settings")]
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    [field: Header("NPC Settings")]
    [field: SerializeField] public HealthController healthController { get; private set; }

    [field: SerializeField] private Animator anim { get; set; }
    [field: SerializeField] private CapsuleCollider capCol { get; set; }

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
        if (healthController == null) {
            healthController = GetComponent<HealthController>();
        }

        if (anim == null) {
            anim = GetComponent<Animator>();
        }

        if (capCol == null) {
            capCol = GetComponentInChildren<CapsuleCollider>();
        }

        revalidateProperties = false;
    }
#endif

    #region Methods Called from HealthController
    /*
     * Methods that are called from the HealthController using Unity Events 
     * and are set in the Unity Editor (Inspector).
     */

    public void TakeDamage() {
        anim.SetTrigger("Hurt");
    }

    public void Heal() {
        anim.SetTrigger("Heal");
    }

    public void Die() {
        anim.SetBool("Dead", true);
    }

    public void Resurrect() {
        anim.SetBool("Dead", false);
    }

    #endregion
}