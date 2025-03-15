using UnityEngine;
using KCC;

namespace Player
{
    public class PlayerDebugDrawer : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private Mesh debugMesh;
        [field: SerializeField] private KinematicCharacterMotor motor;
        [field: SerializeField] private Transform playerCam;

        [field: Header("General Settings")]
        [field: SerializeField] private Color debugColor = Color.red;
        [field: SerializeField] private float parameter = 1f;

        private void OnDrawGizmos()
        {
            Gizmos.color = debugColor;

            Gizmos.DrawWireMesh(debugMesh, 0, motor.transform.position, motor.transform.rotation, motor.transform.localScale);
        }
    }
}
