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
            isGrounded = false;
            collisionFlags = CollisionFlags.None;

            oldPosition = transform.position;

            Vector3 horizontalInput = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Vector3 verticalInput = new Vector3(0f, moveDirection.y, 0f);

            Vector3 horizontalMovement = Collide(horizontalInput, oldPosition, 0, false, horizontalInput);
            Vector3 interimPosition = oldPosition + horizontalMovement;
            Vector3 verticalMovement = Collide(verticalInput, interimPosition, 0, true, verticalInput);
            Vector3 finalMovement = horizontalMovement + verticalMovement;

            transform.position = oldPosition + finalMovement;
            velocity = finalMovement;

            groundCollisions = Physics.OverlapSphere(transform.position + Vector3.up * (radius - (skinWidth + 0.01f)), radius, ~ignoredLayers);

            foreach (Collider collider in groundCollisions)
            {
                if (collider.gameObject != gameObject)
                {
                    isGrounded = true;
                    collisionFlags |= CollisionFlags.Below;
                    break;
                }
            }

            updatedPosition = transform.position;
        }

        private Vector3 Collide(Vector3 velocity, Vector3 startPosition, int depth, bool gravityPass, Vector3 initialDirection)
        {
            if (depth > maxRecursionDepth)
                return Vector3.zero;

            float distance = velocity.magnitude + skinWidth;
            Vector3 direction = velocity.normalized;
            float originalMagnitude = velocity.magnitude;

            Vector3 bottomPoint = startPosition + capsuleCollider.center - Vector3.up * ((height * 0.5f) - radius);
            Vector3 topPoint = startPosition + capsuleCollider.center + Vector3.up * ((height * 0.5f) - radius);

            RaycastHit collisionHit;
            if (Physics.CapsuleCast(bottomPoint, topPoint, capsuleCollider.radius, direction, out collisionHit, distance, ~ignoredLayers))
            {
                Vector3 snapToSurface = direction * Mathf.Max(collisionHit.distance - skinWidth, 0);
                Vector3 leftover = velocity - snapToSurface;
                float angle = Vector3.Angle(collisionHit.normal, Vector3.up);

                if (angle <= maxSlope)
                {
                    if (gravityPass)
                        return snapToSurface;

                    leftover = Vector3.ProjectOnPlane(leftover, collisionHit.normal);

                    leftover = Vector3.ClampMagnitude(leftover, originalMagnitude - snapToSurface.magnitude);
                }
                else
                {
                    if (isGrounded && !gravityPass)
                    {
                        leftover = Vector3.ProjectOnPlane(
                            new Vector3(leftover.x, 0, leftover.z),
                            new Vector3(collisionHit.normal.x, 0, collisionHit.normal.z)
                        );
                    }
                    else
                    {
                        leftover = Vector3.ProjectOnPlane(leftover, collisionHit.normal);
                    }

                    leftover = Vector3.ClampMagnitude(leftover, originalMagnitude - snapToSurface.magnitude);
                }

                if (Vector3.Angle(collisionHit.normal, Vector3.up) < 5f)
                {
                    collisionFlags |= CollisionFlags.Below;
                }
                else if (Vector3.Angle(collisionHit.normal, Vector3.down) < 5f)
                {
                    collisionFlags |= CollisionFlags.Above;
                }
                else
                {
                    collisionFlags |= CollisionFlags.Sides;
                }

                return snapToSurface + Collide(leftover, startPosition + snapToSurface, depth + 1, gravityPass, initialDirection);
            }

            if (!gravityPass && isGrounded && velocity.y == 0)
            {
                RaycastHit groundHit;
                float groundCheckDistance = radius * 2;

                if (Physics.Raycast(startPosition + Vector3.up * (radius * 0.5f), Vector3.down, out groundHit, groundCheckDistance + skinWidth, ~ignoredLayers))
                {
                    float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);

                    if (groundAngle <= maxSlope)
                    {
                        float distanceToGround = groundHit.distance - (radius * 0.5f) - skinWidth;

                        if (distanceToGround > skinWidth && distanceToGround < radius)
                        {
                            Vector3 snapVector = Vector3.down * distanceToGround;
                            return velocity + snapVector;
                        }
                    }
                }
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