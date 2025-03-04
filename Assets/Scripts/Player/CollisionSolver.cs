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

    [RequireComponent(typeof(CapsuleCollider))]
    public class CollisionSolver : MonoBehaviour
    {
        [field: Header("General Settings")]
        [field: SerializeField] private float maxSlope = 55f;

        [field: Header("Collider Settings")]
        [field: SerializeField, Range(1f, 5f)] public float height { get; private set; } = 2.25f;
        [field: SerializeField, Range(0.1f, 1f)] public float radius { get; private set; } = 0.45f;

        [field: Header("Collision Settings")]
        [field: SerializeField] private LayerMask ignoredLayers;
        [field: SerializeField] private int maxRecursionDepth = 5;
        [field: SerializeField] private float skinWidth = 0.001f;

        public CollisionFlags collisionFlags { get; private set; }

        public Vector3 velocity { get; private set; }

        public bool isGrounded { get; private set; }

        private CapsuleCollider capsuleCollider;
        private Collider[] groundCollisions;
        private Bounds capsuleBounds;

        private Vector3 updatedPosition;
        private Vector3 oldPosition;

        private void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }
        private void Start()
        {
            capsuleCollider.height = height;
            capsuleCollider.radius = radius;
        }

        private void Update()
        {
            ResizeCollider();
        }

        public void Move(Vector3 moveDirection)
        {
            oldPosition = transform.position;

            Vector3 horizontalInput = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Vector3 verticalInput = new Vector3(0f, moveDirection.y, 0f);

            Vector3 horizontalMovement = Collide(horizontalInput, oldPosition, 0, false, horizontalInput);
            Vector3 interimPosition = oldPosition + horizontalMovement;
            Vector3 verticalMovement = Collide(verticalInput, interimPosition, 0, true, verticalInput);
            Vector3 finalMovement = horizontalMovement + verticalMovement;

            transform.position = oldPosition + finalMovement;
            velocity = finalMovement;

            groundCollisions = Physics.OverlapSphere(transform.position + Vector3.up * (radius - (skinWidth + 0.01f)), radius);

            foreach (Collider collider in groundCollisions)
            {
                if (collider.gameObject != gameObject)
                {
                    isGrounded = true;
                    collisionFlags |= CollisionFlags.Below;
                }
            }

            updatedPosition = transform.position;
        }

        private Vector3 Collide(Vector3 velocity, Vector3 startPosition, int depth, bool gravityPass, Vector3 initialDirection)
        {
            Vector3 ProjectAndScale(Vector3 vector, Vector3 normal)
            {
                float magnitude = vector.magnitude;
                vector = Vector3.ProjectOnPlane(vector, normal).normalized;
                vector *= magnitude;
                return vector;
            }

            if (depth > maxRecursionDepth)
                return Vector3.zero;

            float distance = velocity.magnitude + skinWidth;

            Vector3 bottomPoint = startPosition + capsuleCollider.center - Vector3.up * ((height * 0.5f) - radius);
            Vector3 topPoint = startPosition + capsuleCollider.center + Vector3.up * ((height * 0.5f) - radius);

            RaycastHit collisionHit;
            if (Physics.CapsuleCast(bottomPoint, topPoint, capsuleCollider.radius, velocity.normalized, out collisionHit, distance, ~ignoredLayers))
            {
                Vector3 snapToSurface = velocity.normalized * (collisionHit.distance - skinWidth);
                Vector3 leftover = velocity - snapToSurface;
                float angle = Vector3.Angle(collisionHit.normal, Vector3.up);

                if (snapToSurface.magnitude <= skinWidth)
                    snapToSurface = Vector3.zero;

                if (angle <= maxSlope)
                {
                    if (gravityPass)
                        return snapToSurface;

                    leftover = ProjectAndScale(leftover, collisionHit.normal);
                }
                else
                {
                    float planerScale = 1 - Vector3.Dot(
                        new Vector3(collisionHit.normal.x, 0, collisionHit.normal.z).normalized,
                        -new Vector3(initialDirection.x, 0, initialDirection.z).normalized
                    );

                    if (isGrounded && !gravityPass)
                    {
                        leftover = ProjectAndScale(
                            new Vector3(leftover.x, 0, leftover.z),
                            new Vector3(collisionHit.normal.x, 0, collisionHit.normal.z)
                        ).normalized;
                        leftover *= planerScale;
                    }
                    else
                    {
                        leftover = ProjectAndScale(leftover, collisionHit.normal) * planerScale;
                    }
                }

                return snapToSurface + Collide(leftover, startPosition + snapToSurface, depth + 1, gravityPass, initialDirection);
            }

            return velocity;
        }

        private void OnDrawGizmosSelected()
        {
            ResizeCollider();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * (radius - (skinWidth + 0.01f)), radius);
        }

        private void ResizeCollider()
        {
            if (capsuleCollider == null)
                capsuleCollider = GetComponent<CapsuleCollider>();

            capsuleCollider.height = height;
            capsuleCollider.radius = radius;
            capsuleCollider.center = new Vector3(0f, height * 0.5f, 0f);

            capsuleBounds = capsuleCollider.bounds;
            capsuleBounds.Expand(-2 * skinWidth);
        }
    }
}