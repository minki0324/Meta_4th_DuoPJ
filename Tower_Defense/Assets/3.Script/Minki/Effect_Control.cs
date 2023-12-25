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
        Beam,
        impact
    }

    public Head_Data head_Data;
    public Effect_type type;
    private Effect_Pooling pool;
    private LineRenderer lineRenderer;

    [Header("시간 관련")]
    [SerializeField] private float delay;       // 활성화 후 일정 delay 뒤에 비활성화 하는 시간
    public float despawnDelay;                  // hit된 후 이펙트 활성화 되고 사라지는 시간
    private float timer = 0f;                   // 실시간 타이머

    [Header("Bool값")]
    public bool DelayDespawn = false;           
    public bool OneShot;                        // 빔 관련 지속 빔인지 단일 빔인지
    private bool isHit = false;                 // 히트 됐는지 안됐는지
    private bool isFXSpawned = false;           // 임팩트가 터졌는지 안터졌는지

    [Header("파티클")]
    public ParticleSystem[] delayedParticles;   
    private ParticleSystem[] particles;

    [Header("위치 관련")]
    new Transform transform;                    // 캐시 된 트랜스폼
    private float fxOffset;                     // 임팩트의 offset
    public float velocity = 300;                // 투사체 속도
    public Transform rayImpact;                 // 충돌 트랜스폼
    public Transform rayMuzzle;                 // 발사 플래시 트랜스폼
    private float initialBeamOffset;            // 초기 UV 오프셋

    [Header("Ray")]
    private RaycastHit hitPoint;                // 3D 레이캐스트
    private RaycastHit2D hitPoint2D;            // 2D 레이캐스트
    public float RaycastAdvance = 2f;           // 레이캐스트 진행 배율
    public LayerMask layerMask;                 // 레이어 마스크

    [Header("애니메이션")]
    public Texture[] BeamFrames;                // 애니메이션 프레임 순서
    public float FrameStep;                     // 애니메이션 간격
    public bool AnimateUV;                      // UV 애니메이션 여부
    public float UVTime;                        // UV 애니메이션 속도
    private int frameNo;                        // 프레임 카운터
    private int FrameTimerID;                   // 프레임 타이머 참조
    private float animateUVTime;

    [Header("Beam")]
    public float beamScale;                     // 기본 빔 스케일
    public float MaxBeamLength;                 // 최대 빔 길이
    private float beamLength;                   // 현재 빔 길이


    #region Unity Callback
    private void Awake()
    {
        pool = FindObjectOfType<Effect_Pooling>();
        transform = GetComponent<Transform>();
        // 변환 캐시 및 연결된 모든 입자 시스템 가져오기
        if(type == Effect_type.projectile)
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }
        else if(type == Effect_type.Beam)
        {
            // 라인 렌더러 컴포넌트 가져오기
            lineRenderer = GetComponent<LineRenderer>();

            // UV 애니메이션 사용하지 않고 BeamFrames 배열이 1개 이상일 경우 첫 번째 프레임 텍스처 할당
            if (!AnimateUV && BeamFrames.Length > 0)
                lineRenderer.material.mainTexture = BeamFrames[0];

            // 무작위 UV 오프셋 설정
            initialBeamOffset = Random.Range(0f, 5f);
        }
    }

    private void Update()
    {
        if(type == Effect_type.projectile)
        {
            isProjectile();
        }
        else if(type == Effect_type.Beam)
        {
            // 텍스처 UV 애니메이션
            if (AnimateUV)
            {
                animateUVTime += Time.deltaTime;

                if (animateUVTime > 1.0f)
                    animateUVTime = 0f;
                var v = animateUVTime * UVTime + initialBeamOffset;
                lineRenderer.material.SetTextureOffset("_BaseMap", new Vector2(v, 0));
            }

            // 레이저 빔을 위한 레이캐스트
            if (!OneShot)
                isBeam();
        }
    }

    // 활성화 후 초기화 및 일정 시간 뒤 액티브 끄기
    private void OnEnable()
    {
        if(isServer)
        {
            if(type == Effect_type.projectile) { OnSpawned_Pro(); }
            else if(type == Effect_type.Beam) { OnSpawned_Beam(); }
        }
        Invoke("active", delay);
    }

    // 꺼질때 delay보다 먼저 꺼지면 active 인보크 해제
    private void OnDisable()
    {
        if(isServer)
        {
            if(type == Effect_type.Beam)
            {
                // 프레임 카운터 초기화
                frameNo = 0;

                // 타이머 초기화
                if (FrameTimerID != -1)
                {
                    FrameTimerID = -1;
                }
            }
        }
        CancelInvoke("active");
    }
    #endregion

    #region SyncVar
    #endregion
    #region Client
    #endregion
    #region Command
    [Server]
    public void Request_Impact(Vector3 pos, int index)
    {
        switch (head_Data.atk_Type)
        {
            case Head_Data.Atk_Type.Vulcan:
                GameObject vul_impact = pool.GetEffect(index);
                GameManager.instance.RPC_TransformSet(vul_impact, pos, Quaternion.identity);
                break;
            case Head_Data.Atk_Type.Sniper:
                GameObject sni_impact = pool.GetEffect(index);
                GameManager.instance.RPC_TransformSet(sni_impact, pos, Quaternion.identity);
                break;
        }
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

    private void OnSpawned_Pro()
    {
        // 플래그 및 레이캐스트 구조체 재설정
        isHit = false;
        isFXSpawned = false;
        timer = 0f;
        hitPoint = new RaycastHit();
    }

    void OnSpawned_Beam()
    {
        // OneShot 플래그가 true일 경우 한 번만 레이캐스트 수행
        if (OneShot)
            isBeam();

        // BeamFrames 배열이 2개 이상일 경우 애니메이션 시작
        if (BeamFrames.Length > 1)
            Animate();
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
                // 임팩트를 생성하는 해당 메소드를 호출
                switch (head_Data.atk_Type)
                {
                    case Head_Data.Atk_Type.Vulcan:
                        if (isServer)
                        {
                            Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 2);
                        }
                        break;
                }

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

    private void isBeam()
    {
        // 구조체 초기화 및 레이 생성
        hitPoint = new RaycastHit();
        Ray ray = new Ray(transform.position, transform.forward);
        // 기본 스케일 및 최대 길이를 기반으로한 기본 빔 비율 계수 계산
        float propMult = MaxBeamLength * (beamScale / 10f);

        // 레이캐스트
        if (Physics.Raycast(ray, out hitPoint, MaxBeamLength, layerMask))
        {
            // 현재 빔 길이 가져오고 라인 렌더러 업데이트
            beamLength = Vector3.Distance(transform.position, hitPoint.point);
            lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));

            // 현재 길이를 기반으로한 기본 빔 비율 계수 계산
            propMult = beamLength * (beamScale / 10f);
            // 프리팹 스폰
            switch (head_Data.atk_Type)
            {
                case Head_Data.Atk_Type.Sniper:
                    Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 5);
                    break;
            }

            // 충돌 효과 위치 조정
            if (rayImpact)
                rayImpact.position = hitPoint.point - transform.forward * 0.5f;
        }
        // 2D 모드에서 확인
        else
        {
            RaycastHit2D ray2D = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y),
                new Vector2(transform.forward.x, transform.forward.y), beamLength, layerMask);
            if (ray2D)
            {
                // 현재 빔 길이 가져오고 라인 렌더러 업데이트
                beamLength = Vector3.Distance(transform.position, ray2D.point);
                lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));

                // 현재 길이를 기반으로한 기본 빔 비율 계수 계산
                propMult = beamLength * (beamScale / 10f);
                // 프리팹 스폰
                switch (head_Data.atk_Type)
                {
                    case Head_Data.Atk_Type.Sniper:
                        Request_Impact(ray2D.point + ray2D.normal * fxOffset, 5);
                        break;
                }

                // 충돌 효과 위치 조정
                if (rayImpact)
                    rayImpact.position = new Vector3(ray2D.point.x,
                                             ray2D.point.y,
                                             this.gameObject.transform.position.z) - transform.forward * 0.5f;
            }
            // 아무것도 충돌하지 않았을 때
            else
            {
                // 빔을 최대 길이로 설정
                beamLength = MaxBeamLength;
                lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));

                // 충돌 효과 위치 조정
                if (rayImpact)
                    rayImpact.position = transform.position + transform.forward * beamLength;
            }
        }

        // 머즐 위치 조정
        if (rayMuzzle)
            rayMuzzle.position = transform.position + transform.forward * 0.1f;

        // 빔 길이에 따라 빔 스케일 조정
        lineRenderer.material.SetTextureScale("_BaseMap", new Vector2(propMult, 1f));
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

    // 애니메이션 프레임 동기화
    private IEnumerator AnimateFrames()
    {
        float timer = 0f;

        while (true)
        {
            // 일정 간격으로 프레임 전환
            timer += Time.deltaTime;
            if (timer >= FrameStep)
            {
                // 현재 프레임에 맞춰 현재 텍스처 프레임 설정
                lineRenderer.material.mainTexture = BeamFrames[frameNo];
                frameNo++;

                // 프레임 카운터 초기화
                if (frameNo == BeamFrames.Length)
                {
                    frameNo = 0;
                }

                timer = 0f;
            }

            // 대기
            yield return null;
        }
    }

    // 프레임 애니메이션 초기화
    private void Animate()
    {
        if (BeamFrames.Length > 1)
        {
            // 현재 프레임 설정
            frameNo = 0;
            lineRenderer.material.mainTexture = BeamFrames[frameNo];

            // 코루틴으로 애니메이션 구현
            StartCoroutine(AnimateFrames());
        }
    }
}
