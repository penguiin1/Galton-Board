using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPoolManager : MonoBehaviour
{
    [Header("Ball Settings")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private int poolSize = 100;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private float ballForce = 1f;
    [SerializeField] private Vector3 initialForceDirection = Vector3.down;

    [Header("Spawn Area")]
    [SerializeField] private float spawnAreaWidth = 1f;
    
    private List<GameObject> ballPool;
    private bool isSpawning = false;
    
    private void Awake()
    {
        // 오브젝트 풀 초기화
        InitializePool();
    }
    
    private void InitializePool()
    {
        ballPool = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab);
            ball.SetActive(false);
            ball.transform.SetParent(transform);
            
            ballPool.Add(ball);
        }
    }
    
    // 풀에서 비활성화된 공을 가져오는 메서드
    private GameObject GetBallFromPool()
    {
        foreach (GameObject ball in ballPool)
        {
            if (!ball.activeInHierarchy)
            {
                return ball;
            }
        }
        
        // 모든 공이 활성화되어 있다면 null 반환
        return null;
    }
    
    // 공 스폰 시작
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnBalls());
        }
    }
    
    // 공 스폰 중지
    public void StopSpawning()
    {
        isSpawning = false;
    }
    
    private IEnumerator SpawnBalls()
    {
        while (isSpawning)
        {
            SpawnBall();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    // 단일 공 스폰
    public void SpawnBall()
    {
        GameObject ball = GetBallFromPool();
        if (ball != null)
        {
            // 스폰 위치에 약간의 랜덤성 추가
            Vector3 randomPosition = spawnPoint.position + new Vector3(Random.Range(-spawnAreaWidth/2, spawnAreaWidth/2), 0, 0);
            ball.transform.position = randomPosition;
            ball.transform.rotation = Quaternion.identity;
            
            // 공 활성화
            ball.SetActive(true);
            
            // 공에 초기 힘 적용
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddForce(initialForceDirection.normalized * ballForce, ForceMode.Impulse);
            }
            
            // BallController에 풀 매니저 참조 전달
            BallController ballController = ball.GetComponent<BallController>();
            if (ballController != null)
            {
                ballController.SetPoolManager(this);
            }
        }
    }
    
    // 공을 풀로 반환
    public void ReturnBallToPool(GameObject ball)
    {
        ball.SetActive(false);
    }
    
}
