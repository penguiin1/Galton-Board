using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject sparkEffectPrefab; // 스파크 효과 프리팹
    [SerializeField] private float minCollisionForce = 2.0f; // 스파크 발생을 위한 최소 충돌 힘
    [SerializeField] private int maxSparkEffects = 5; // 한 공당 최대 스파크 효과 수
    
    private List<ParticleSystem> sparkEffectPool = new List<ParticleSystem>(); // 스파크 효과 풀
    private int currentSparkIndex = 0;
    private BallPoolManager poolManager;
    private Rigidbody rb;
    
    [SerializeField] private float maxLifetime = 40f; // 최대 생존 시간
    [SerializeField] private float bottomY = -30f; // 화면 아래 경계
    
    private float lifetimeTimer;
    private bool isInitialized = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            SetupRigidbody();
        }
        InitializeSparkEffectPool();
    }
    
    private void SetupRigidbody()
    {
        // 갤턴 보드에 적합한 물리 속성 설정
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 1f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.05f;
    }
    
    public void SetPoolManager(BallPoolManager manager)
    {
        poolManager = manager;
        isInitialized = true;
    }
    
    private void OnEnable()
    {
        lifetimeTimer = 0f;
        currentSparkIndex = 0;
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        lifetimeTimer += Time.deltaTime;
        
        // 최대 생존 시간 초과 또는 화면 아래로 떨어졌는지 확인
        if (lifetimeTimer > maxLifetime || transform.position.y < bottomY)
        {
            ReturnToPool();
        }
    }
    
    private void ReturnToPool()
    {
        if (poolManager != null)
        {
            poolManager.ReturnBallToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    // 충돌 시 효과 추가
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌 힘 계산
        float impactForce = collision.relativeVelocity.magnitude;
        
        // 충돌 시 추가 힘 적용
        if (collision.gameObject.CompareTag("Pin") || collision.gameObject.CompareTag("Ball"))
        {
            
            // 충분한 충돌 힘이 있을 때만 스파크 효과 재생
            if (impactForce > minCollisionForce)
            {
                PlaySparkEffect(collision.contacts[0].point, collision.contacts[0].normal);
            }
        }
    }
    private void PlaySparkEffect(Vector3 position, Vector3 normal)
    {
        if (sparkEffectPool.Count == 0) return;
        
        // 풀에서 다음 스파크 효과 가져오기
        ParticleSystem sparkPS = sparkEffectPool[currentSparkIndex];
        currentSparkIndex = (currentSparkIndex + 1) % sparkEffectPool.Count;
        
        // 위치 및 방향 설정
        sparkPS.transform.position = position;
        sparkPS.transform.rotation = Quaternion.LookRotation(normal);
        
        // 스파크 효과 활성화 및 재생
        sparkPS.gameObject.SetActive(true);
        sparkPS.Clear(); // 이전 파티클 제거
        sparkPS.Play();
        
        // 일정 시간 후 비활성화
        StartCoroutine(DeactivateAfterPlay(sparkPS.gameObject, sparkPS.main.duration + 0.2f));
    }

     private IEnumerator DeactivateAfterPlay(GameObject effectObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effectObj != null)
        {
            effectObj.SetActive(false);
        }
    }

    private void InitializeSparkEffectPool()
    {
        if (sparkEffectPrefab == null) return;
        
        // 스파크 효과 풀 생성
        for (int i = 0; i < maxSparkEffects; i++)
        {
            GameObject sparkObj = Instantiate(sparkEffectPrefab, Vector3.zero, Quaternion.identity);
            sparkObj.name = "SparkEffect_" + i;
            sparkObj.SetActive(false);
            
            ParticleSystem sparkPS = sparkObj.GetComponent<ParticleSystem>();
            if (sparkPS != null)
            {
                sparkEffectPool.Add(sparkPS);
            }
        }
    }
}
