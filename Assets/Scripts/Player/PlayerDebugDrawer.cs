using UnityEngine;
using KCC;

namespace Player
{
    public class PlayerDebugDrawer : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private KinematicCharacterMotor motor;
        [field: SerializeField] private Transform playerCam;
        [field: SerializeField] private Mesh debugMesh;

        [field: Header("General Settings")]
        [field: SerializeField] private Color debugColor = Color.red;

        private void OnDrawGizmos()
        {
            Gizmos.color = debugColor;

            Gizmos.DrawWireMesh(debugMesh, 0, motor.transform.position + Vector3.up * motor.Capsule.center.y, motor.transform.rotation, new Vector3(1f, 1f * motor.Capsule.height * 0.5f, 1f));
            Gizmos.DrawRay(playerCam.position, playerCam.forward);
        }
    }
}
