using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Effect_Control : NetworkBehaviour
{
    public enum Effect_type
    {
        muzzle,
        projectile,
        impact
    }

    public Head_Data head_Data;
    public Effect_type type;

    [Header("시간 관련")]
    [SerializeField] private float delay;       // 활성화 후 일정 delay 뒤에 비활성화 하는 시간
    public float despawnDelay;                  // hit된 후 이펙트 활성화 되고 사라지는 시간
    private float timer = 0f;                   // 실시간 타이머

    [Header("Bool값")]
    public bool DelayDespawn = false;           
    private bool isHit = false;                 // 히트 됐는지 안됐는지
    private bool isFXSpawned = false;           // 임팩트가 터졌는지 안터졌는지

    [Header("파티클")]
    public ParticleSystem[] delayedParticles;   
    private ParticleSystem[] particles;

    [Header("위치 관련")]
    new Transform transform;                    // 캐시 된 트랜스폼
    private float fxOffset;                     // 임팩트의 offset
    public float velocity = 300;                // 투사체 속도

    [Header("Ray")]
    private RaycastHit hitPoint;                // 레이가 맞은 위치
    public float RaycastAdvance = 2f;           // 레이캐스트 진행 배율
    public LayerMask layerMask;                 // 레이어 마스크

    private Effect_Pooling pool;

    #region Unity Callback
    private void Awake()
    {
        // 변환 캐시 및 연결된 모든 입자 시스템 가져오기
        transform = GetComponent<Transform>();
        particles = GetComponentsInChildren<ParticleSystem>();
        pool = FindObjectOfType<Effect_Pooling>();
    }

    private void Update()
    {
        isProjectile();
    }

    // 활성화 후 초기화 및 일정 시간 뒤 액티브 끄기
    private void OnEnable()
    {
        if(type == Effect_type.projectile)
        {
            OnSpawned();
        }
        Invoke("active", delay);
    }

    // 꺼질때 delay보다 먼저 꺼지면 active 인보크 해제
    private void OnDisable()
    {
        CancelInvoke("active");
    }
    #endregion

    #region SyncVar
    #endregion
    #region Client
    /*[Client]
    private void Request_Impact()
    {
        // 클라이언트에서 서버 명령 호출 시 권한 확인
        if (isOwned)
        {
            Vector3 pos = hitPoint.point + hitPoint.normal * fxOffset;
            CMD_VulcanImpact(pos);
        }
    }*/
    #endregion
    #region Command
    [Server]
    public void VulcanImpact(Vector3 pos)
    {
        GameObject impact = pool.GetEffect(2);
        GameManager.instance.RPC_ActiveSet(true, impact);
        Tower_Attack.instance.Set_Pos_Rot(impact, pos, Quaternion.identity);
        GameManager.instance.RPC_TransformSet(impact, pos, Quaternion.identity);
    }

    #endregion
    #region ClientRPC
    #endregion
    #region Hook Method
    #endregion
    private void active()
    {
        gameObject.SetActive(false);
    }

    private void OnSpawned()
    {
        // 플래그 및 레이캐스트 구조체 재설정
        isHit = false;
        isFXSpawned = false;
        timer = 0f;
        hitPoint = new RaycastHit();
    }

    private void isProjectile()
    {
        // 무언가 충돌한 경우
        if(isHit)
        {
            // 임팩트 한번만 실행
            if (!isFXSpawned)
            {
                // 추후 시간 남으면 임팩트 생성하는 메소드 재정비
                /*// 임팩트를 생성하는 해당 메소드를 호출
                switch (head_Data.atk_Type)
                {
                    case Head_Data.Atk_Type.Vulcan:
                        if (isServer)
                        {
                            VulcanImpact(hitPoint.point + hitPoint.normal * fxOffset);
                        }
                        break;
                }
*/
                isFXSpawned = true;
            }

            // 현재 발사체 소멸
            if (!DelayDespawn || (DelayDespawn && (timer >= despawnDelay)))
                active();
        }

        // 아직 충돌이 발생하지 않았을 때
        else
        {
            // 발사체 이동 방향 및 속도
            Vector3 step = transform.forward * Time.deltaTime * velocity;

            // 레이캐스트 길이 기반의 대상
            if (Physics.Raycast(transform.position, transform.forward, out hitPoint, step.magnitude * RaycastAdvance, layerMask))
            {
                isHit = true;

                // 필요한 경우 지연 루틴
                if (DelayDespawn)
                {
                    // 발사체 타이머를 재설정하고 파티클 시스템이 올바르게 방출을 중지하고 페이드 아웃
                    timer = 0f;
                    Delay();
                }
            }

            // 아무것도 충돌하지 않음
            else
            {
                // 시간이 다 되어 발사체 소멸
                if (timer >= delay)
                    active();
            }

            // 발사체 전진
            transform.position += step;
        }
        // 발사체 타이머 업데이트
        timer += Time.deltaTime;
    }

    private void Delay()
    {
        if (particles.Length > 0 && delayedParticles.Length > 0)
        {
            bool delayed;
            for (int i = 0; i < particles.Length; i++)
            {
                delayed = false;
                for (int y = 0; y < delayedParticles.Length; y++)
                    if (particles[i] == delayedParticles[y])
                    {
                        delayed = true;
                        break;
                    }

                particles[i].Stop(false);
                if (!delayed)
                    particles[i].Clear(false);
            }
        }
    }

    // 오프셋 설정
    public void SetOffset(float offset)
    {
        fxOffset = offset;
    }

    
}
