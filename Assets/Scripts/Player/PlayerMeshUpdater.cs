using UnityEngine;
using KCC;

namespace Player
{
    [ExecuteAlways]
    public class PlayerMeshUpdater : MonoBehaviour
    {
        [field: Header("Dependencies")]
        [field: SerializeField] private KinematicCharacterMotor motor;

        private void Update()
        {
            if (motor == null) return;

            transform.position = motor.transform.position + transform.up * motor.Capsule.center.y;
            transform.localScale = new Vector3(1, motor.Capsule.height * 0.5f, 1);
        }
    }
}