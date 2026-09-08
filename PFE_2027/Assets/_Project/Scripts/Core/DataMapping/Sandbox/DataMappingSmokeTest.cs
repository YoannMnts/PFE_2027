using PFE.Core.DataMapping;
using UnityEngine;

namespace PFE.Core.DataMapping.Sandbox
{
    /// <summary>
    /// Smoke test manuel pour valider la chaîne mapper de bout en bout.
    /// Assigner un asset SandboxData dans l'Inspector, attacher sur un GameObject, lancer la scène.
    /// A supprimer une fois la validation faite.
    /// </summary>
    public class DataMappingSmokeTest : MonoBehaviour
    {
        [SerializeField] private SandboxData data;

        private void Start()
        {
            if (data.TryGet(out ISandboxBehaviourContainer container))
            {
                Debug.Log($"[DataMapping Sandbox] Container resolved for ID={data.ID}");
                container.Execute(data);
            }
            else
            {
                Debug.LogError("[DataMapping Sandbox] No container found for SandboxData - mapper registration failed (bootstrap not run / generator not applied?).");
            }
        }
    }
}
