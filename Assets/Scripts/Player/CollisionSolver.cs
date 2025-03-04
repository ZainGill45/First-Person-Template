using UnityEngine;

namespace Player
{
    public enum CollisionFlags
    {
        None = 0,
        Above = 1,
        Below = 2,
        Sides = 4
    }

    public class CollisionSolver : MonoBehaviour
    {
        public CollisionFlags collisionFlags { get; private set; }

        public Vector3 velocity { get; private set; }

        public float height { get; private set; }
        public float radius { get; private set; }

        public bool isGrounded { get; private set; }
        
        public void Move(Vector3 moveDirection)
        {
        }
    }
}