using UnityEngine;

// Compatibility marker for existing scenes. Lighting uses disjoint serialized
// sorting-layer lists; this component never enables/disables lights.
[DisallowMultipleComponent]
public sealed class SharedGlobalLight2D : MonoBehaviour
{
}
