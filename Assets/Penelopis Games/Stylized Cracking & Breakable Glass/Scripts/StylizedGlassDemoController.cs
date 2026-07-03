using UnityEngine;

namespace PenelopisGames.StylizedGlass
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class StylizedGlassDemoController : MonoBehaviour
    {
        private const string GameUiCanvasName = "PlayerHeartsCanvas";

        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckDistance = 0.12f;

        private Rigidbody body;
        private CapsuleCollider capsule;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            DestroyGameUiCanvas();
        }

        private void FixedUpdate()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 input = new Vector3(horizontal, 0f, vertical);
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            Vector3 velocity = input * moveSpeed;
            velocity.y = body.linearVelocity.y;
            body.linearVelocity = velocity;

            if (Input.GetKey(KeyCode.Space) && IsGrounded())
                body.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        private void LateUpdate()
        {
            DestroyGameUiCanvas();
        }

        private bool IsGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * 0.05f;
            float radius = Mathf.Max(0.05f, capsule.radius * 0.9f);
            float distance = capsule.height * 0.5f - capsule.radius + groundCheckDistance;
            return Physics.SphereCast(origin, radius, Vector3.down, out _, distance, groundMask, QueryTriggerInteraction.Ignore);
        }

        private static void DestroyGameUiCanvas()
        {
            GameObject canvas = GameObject.Find(GameUiCanvasName);
            if (canvas)
                Destroy(canvas);
        }
    }
}
