using UnityEngine;
using PolarBond.Core;

namespace PolarBond.Views
{
    // Note: TargetView does not inherit from EntityView because targets don't move
    // and aren't strictly GridEntities in the same way (entities can walk over them).
    public class TargetView : MonoBehaviour
    {
        [Header("Target Settings")]
        public TargetType Type;


        
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                transform.position = new Vector3(
                    Mathf.Round(transform.position.x),
                    Mathf.Round(transform.position.y),
                    0
                );
            }
        }
    }
}
