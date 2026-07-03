using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PenelopisGames.StylizedGlass
{
    [DisallowMultipleComponent]
    public class StylizedBreakableGlass : MonoBehaviour
    {
        public enum ShardEndMode
        {
            Freeze = 0,
            Disable = 1,
            Destroy = 2
        }

        [Header("Impact")]
        [SerializeField] private LayerMask impactMask = ~0;
        [SerializeField] private float minImpactSpeed = 1.5f;
        [SerializeField] private float shatterImpactSpeed = 3f;
        [SerializeField] private bool allowInstantShatterBySpeed = true;
        [SerializeField] private float impactCooldown = 0.05f;
        [SerializeField] private float damagePerSpeed = 1.25f;
        [SerializeField] private float flatDamagePerHit = 0.5f;
        [SerializeField] private float damageToShatter = 4f;
        [SerializeField, Min(1)] private int hitsToShatter = 2;

        [Header("Contact Shatter")]
        [SerializeField] private bool shatterOnAllowedContact;
        [SerializeField] private LayerMask contactShatterMask;

        [Header("Cracked Stage")]
        [SerializeField] private GameObject crackedVisual;
        [SerializeField] private bool crackedVisualReplacesIntact;

        [Header("Shards")]
        [SerializeField] private GameObject shatteredPrefab;
        [SerializeField] private Transform preplacedShardsRoot;
        [SerializeField] private bool detachShardsOnShatter = true;
        [SerializeField] private bool activateShardPhysicsOnShatter = true;
        [SerializeField] private float shardImpulseMultiplier = 1f;
        [SerializeField] private float shardUpBias = 0.1f;
        [SerializeField] private float randomImpulseJitter = 0.15f;
        [SerializeField] private float shardMaxAngularVelocity = 40f;
        [SerializeField] private GameObject shatterVfxPrefab;

        [Header("Impact VFX")]
        [SerializeField] private GameObject hitSparkPrefab;
        [SerializeField] private float hitSparkScale = 1f;
        [SerializeField] private Vector3 hitSparkWorldOffset = new Vector3(0f, 0.15f, 0f);
        [SerializeField] private float hitSparkNormalOffset = 0.02f;

        [Header("Audio")]
        [SerializeField] private AudioClip crackClip;
        [SerializeField] private AudioClip shatterClip;
        [SerializeField, Range(0f, 1f)] private float audioVolume = 0.9f;

        [Header("After Shatter")]
        [SerializeField] private float shardLifetime = 2f;
        [SerializeField] private ShardEndMode shardEndMode = ShardEndMode.Freeze;
        [SerializeField] private bool disableIntactRootOnShatter = true;
        [SerializeField] private bool disableThisScriptAfterShatter = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onShattered;
        [SerializeField] private bool notifyGameStateControllerOnShatter;
        [SerializeField] private GameStateController gameStateController;

        private readonly List<Renderer> intactRenderers = new List<Renderer>(8);
        private readonly List<Collider> intactColliders = new List<Collider>(8);
        private readonly List<Rigidbody> shardBodies = new List<Rigidbody>(32);
        private readonly List<Collider> shardColliders = new List<Collider>(32);
        private readonly List<Renderer> shardRenderers = new List<Renderer>(32);

        private bool isCracked;
        private bool isShattered;
        private float damage;
        private int hitCount;
        private float nextImpactTime;

        private void Awake()
        {
            CacheIntactParts();
            SetupShardsInitialState();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isShattered) return;
            if (!IsLayerAllowed(collision.gameObject.layer)) return;

            float speed = collision.relativeVelocity.magnitude;
            Vector3 point;
            Vector3 normal;
            if (collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                point = contact.point;
                normal = contact.normal;
            }
            else
            {
                point = transform.position;
                normal = -collision.relativeVelocity.normalized;
            }

            if (ShouldShatterOnContact(collision.gameObject.layer))
            {
                ForceShatter(point, normal, speed);
                return;
            }

            if (Time.time < nextImpactTime) return;
            if (speed < minImpactSpeed) return;

            RegisterImpact(speed, point, normal);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isShattered) return;
            if (!IsLayerAllowed(other.gameObject.layer)) return;

            Rigidbody body = other.attachedRigidbody;
            float speed = body ? body.linearVelocity.magnitude : 0f;

            Vector3 point = other.ClosestPoint(transform.position);
            Vector3 normal = transform.position - point;
            if (normal.sqrMagnitude < 0.0001f)
                normal = transform.forward;

            if (ShouldShatterOnContact(other.gameObject.layer))
            {
                ForceShatter(point, normal.normalized, speed);
                return;
            }

            if (Time.time < nextImpactTime) return;
            if (speed < minImpactSpeed) return;

            RegisterImpact(speed, point, normal.normalized);
        }

        public void ForceShatter()
        {
            ForceShatter(transform.position, transform.forward, shatterImpactSpeed);
        }

        public void ForceShatter(Vector3 impactPoint, Vector3 impactNormal, float impactSpeed)
        {
            if (isShattered) return;

            if (impactNormal.sqrMagnitude < 0.0001f)
                impactNormal = transform.forward;

            Shatter(impactPoint, impactNormal.normalized, Mathf.Max(impactSpeed, shatterImpactSpeed));
        }

        public void RegisterImpact(float speed, Vector3 point, Vector3 normal)
        {
            if (isShattered) return;
            if (Time.time < nextImpactTime) return;
            if (speed < minImpactSpeed) return;

            nextImpactTime = Time.time + impactCooldown;
            damage += flatDamagePerHit + Mathf.Max(0f, speed - minImpactSpeed) * damagePerSpeed;
            hitCount++;

            SpawnHitSpark(point, normal);
            TryEnterCrackedStage(point);

            bool shatterBySpeed = allowInstantShatterBySpeed && speed >= shatterImpactSpeed;
            bool shatterByDamage = damage >= damageToShatter;
            bool shatterByHits = hitCount >= Mathf.Max(1, hitsToShatter);

            if (shatterBySpeed || shatterByDamage || shatterByHits)
                Shatter(point, normal, speed);
        }

        private void Shatter(Vector3 impactPoint, Vector3 impactNormal, float impactSpeed)
        {
            if (isShattered) return;
            isShattered = true;

            onShattered?.Invoke();
            NotifyGameStateController();

            PlayClip(shatterClip, impactPoint);
            SpawnOneShot(shatterVfxPrefab, impactPoint, impactNormal, 1f);

            if (crackedVisual)
                crackedVisual.SetActive(false);

            SetIntactVisibleAndSolid(false);

            Transform shardRoot = GetOrCreateShardsRoot();
            if (!shardRoot)
            {
                if (disableThisScriptAfterShatter) enabled = false;
                return;
            }

            ActivateShards(shardRoot);
            if (activateShardPhysicsOnShatter)
                ApplyShardImpulse(impactPoint, impactNormal, impactSpeed);

            if (shardLifetime > 0f)
                StartCoroutine(ShardEndRoutine(shardLifetime));

            if (disableThisScriptAfterShatter)
                enabled = false;
        }

        private void CacheIntactParts()
        {
            intactRenderers.Clear();
            intactColliders.Clear();
            intactRenderers.AddRange(GetComponentsInChildren<Renderer>(true));
            intactColliders.AddRange(GetComponentsInChildren<Collider>(true));
        }

        private void SetupShardsInitialState()
        {
            shardBodies.Clear();
            shardColliders.Clear();
            shardRenderers.Clear();

            if (preplacedShardsRoot)
                CacheShardParts(preplacedShardsRoot);

            for (int i = 0; i < shardBodies.Count; i++)
            {
                Rigidbody body = shardBodies[i];
                if (!body) continue;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                body.isKinematic = true;
                body.detectCollisions = false;
                body.maxAngularVelocity = shardMaxAngularVelocity;
            }

            for (int i = 0; i < shardColliders.Count; i++)
            {
                if (shardColliders[i])
                    shardColliders[i].enabled = false;
            }

            for (int i = 0; i < shardRenderers.Count; i++)
            {
                if (shardRenderers[i])
                    shardRenderers[i].enabled = false;
            }

            if (preplacedShardsRoot)
                preplacedShardsRoot.gameObject.SetActive(false);
            if (crackedVisual)
                crackedVisual.SetActive(false);
        }

        private Transform GetOrCreateShardsRoot()
        {
            if (preplacedShardsRoot)
                return preplacedShardsRoot;
            if (!shatteredPrefab)
                return null;

            GameObject instance = Instantiate(shatteredPrefab, transform.position, transform.rotation, transform);
            preplacedShardsRoot = instance.transform;
            CacheShardParts(preplacedShardsRoot);
            return preplacedShardsRoot;
        }

        private void CacheShardParts(Transform root)
        {
            shardBodies.Clear();
            shardColliders.Clear();
            shardRenderers.Clear();

            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++)
            {
                Rigidbody body = bodies[i];
                if (!body) continue;
                if (crackedVisual && body.transform.IsChildOf(crackedVisual.transform)) continue;
                shardBodies.Add(body);
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider col = colliders[i];
                if (!col) continue;
                if (crackedVisual && col.transform.IsChildOf(crackedVisual.transform)) continue;
                shardColliders.Add(col);
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!renderer) continue;
                if (crackedVisual && renderer.transform.IsChildOf(crackedVisual.transform)) continue;
                shardRenderers.Add(renderer);
            }
        }

        private void ActivateShards(Transform shardRoot)
        {
            shardRoot.gameObject.SetActive(true);

            if (detachShardsOnShatter)
                shardRoot.SetParent(null, true);

            for (int i = 0; i < shardRenderers.Count; i++)
            {
                if (shardRenderers[i])
                    shardRenderers[i].enabled = true;
            }

            for (int i = 0; i < shardColliders.Count; i++)
            {
                if (shardColliders[i])
                    shardColliders[i].enabled = true;
            }

            for (int i = 0; i < shardBodies.Count; i++)
            {
                Rigidbody body = shardBodies[i];
                if (!body) continue;

                if (activateShardPhysicsOnShatter)
                {
                    body.isKinematic = false;
                    body.useGravity = true;
                }
                else
                {
                    if (!body.isKinematic)
                    {
                        body.linearVelocity = Vector3.zero;
                        body.angularVelocity = Vector3.zero;
                    }
                    body.isKinematic = true;
                }

                body.detectCollisions = true;
                body.maxAngularVelocity = shardMaxAngularVelocity;
            }
        }

        private void ApplyShardImpulse(Vector3 impactPoint, Vector3 impactNormal, float impactSpeed)
        {
            float baseImpulse = Mathf.Max(0f, impactSpeed) * shardImpulseMultiplier;

            for (int i = 0; i < shardBodies.Count; i++)
            {
                Rigidbody body = shardBodies[i];
                if (!body) continue;

                Vector3 direction = body.worldCenterOfMass - impactPoint;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = impactNormal.sqrMagnitude > 0.0001f ? impactNormal : transform.forward;
                direction.Normalize();
                direction += Vector3.up * shardUpBias;
                direction += Random.insideUnitSphere * randomImpulseJitter;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = transform.forward;
                direction.Normalize();

                body.AddForce(direction * baseImpulse, ForceMode.Impulse);
                body.AddTorque(Random.insideUnitSphere * baseImpulse, ForceMode.Impulse);
            }
        }

        private IEnumerator ShardEndRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (shardEndMode == ShardEndMode.Destroy)
            {
                if (preplacedShardsRoot)
                    Destroy(preplacedShardsRoot.gameObject);
                if (!disableIntactRootOnShatter)
                    Destroy(gameObject);
                yield break;
            }

            if (shardEndMode == ShardEndMode.Disable)
            {
                if (preplacedShardsRoot)
                    preplacedShardsRoot.gameObject.SetActive(false);
                yield break;
            }

            for (int i = 0; i < shardBodies.Count; i++)
            {
                Rigidbody body = shardBodies[i];
                if (!body) continue;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                body.isKinematic = true;
                body.detectCollisions = false;
            }
        }

        private void TryEnterCrackedStage(Vector3 point)
        {
            if (isCracked) return;
            isCracked = true;

            PlayClip(crackClip, point);

            if (!crackedVisual)
                return;

            if (preplacedShardsRoot && crackedVisual.transform.IsChildOf(preplacedShardsRoot))
                preplacedShardsRoot.gameObject.SetActive(true);

            crackedVisual.SetActive(true);
            Renderer[] crackedRenderers = crackedVisual.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < crackedRenderers.Length; i++)
            {
                if (crackedRenderers[i])
                    crackedRenderers[i].enabled = true;
            }

            if (!crackedVisualReplacesIntact)
                return;

            SetIntactVisible(false);
        }

        private void SetIntactVisible(bool visible)
        {
            for (int i = 0; i < intactRenderers.Count; i++)
            {
                Renderer renderer = intactRenderers[i];
                if (!renderer) continue;
                if (preplacedShardsRoot && renderer.transform.IsChildOf(preplacedShardsRoot)) continue;
                renderer.enabled = visible;
            }
        }

        private void SetIntactVisibleAndSolid(bool state)
        {
            SetIntactVisible(state);

            for (int i = 0; i < intactColliders.Count; i++)
            {
                Collider collider = intactColliders[i];
                if (!collider) continue;
                if (preplacedShardsRoot && collider.transform.IsChildOf(preplacedShardsRoot)) continue;
                collider.enabled = state;
            }

            if (disableIntactRootOnShatter || state)
                return;

            for (int i = 0; i < intactColliders.Count; i++)
            {
                Collider collider = intactColliders[i];
                if (collider && collider.transform == transform)
                    collider.enabled = false;
            }
        }

        private bool IsLayerAllowed(int layer)
        {
            return impactMask.value == 0 || (impactMask.value & (1 << layer)) != 0;
        }

        private bool ShouldShatterOnContact(int layer)
        {
            if (!shatterOnAllowedContact)
                return false;

            int mask = contactShatterMask.value != 0 ? contactShatterMask.value : impactMask.value;
            return mask == 0 || (mask & (1 << layer)) != 0;
        }

        private void SpawnHitSpark(Vector3 point, Vector3 normal)
        {
            if (!hitSparkPrefab) return;

            Vector3 direction = normal;
            if (direction.sqrMagnitude < 0.0001f)
                direction = transform.forward;
            direction.Normalize();

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            Vector3 spawnPos = point + hitSparkWorldOffset + direction * hitSparkNormalOffset;
            GameObject instance = Instantiate(hitSparkPrefab, spawnPos, rotation);
            if (hitSparkScale != 1f)
                instance.transform.localScale *= hitSparkScale;
        }

        private void SpawnOneShot(GameObject prefab, Vector3 point, Vector3 normal, float scale)
        {
            if (!prefab) return;

            Quaternion rotation = normal.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(normal.normalized, Vector3.up)
                : Quaternion.identity;
            GameObject instance = Instantiate(prefab, point, rotation);
            instance.transform.localScale *= Mathf.Max(0.01f, scale);
        }

        private void PlayClip(AudioClip clip, Vector3 point)
        {
            if (clip)
                AudioSource.PlayClipAtPoint(clip, point, audioVolume);
        }

        private void NotifyGameStateController()
        {
            if (!notifyGameStateControllerOnShatter)
                return;

            if (gameStateController == null)
                gameStateController = FindAnyObjectByType<GameStateController>();

            if (gameStateController != null)
                gameStateController.HandleFinalChamberGlassShattered();
            else
                Debug.LogWarning("StylizedBreakableGlass could not find GameStateController for final glass shatter flow.", this);
        }
    }
}
