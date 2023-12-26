using UnityEngine;
using System.Collections;

namespace FORGE3D
{
    public class F3DProjectile : MonoBehaviour
    {
        public F3DFXType fxType; // 무기 타입
        public LayerMask layerMask;
        public float lifeTime = 5f; // 발사체 수명
        public float despawnDelay; // 지연 소멸(ms)
        public float velocity = 300f; // 발사체 속도
        public float RaycastAdvance = 2f; // 레이캐스트 진행 배율
        public bool DelayDespawn = false; // 발사체 소멸 플래그
        public ParticleSystem[] delayedParticles; // 지연된 입자의 배열
        ParticleSystem[] particles; // 발사체 입자의 배열
        new Transform transform; // 캐시된 변환
        RaycastHit hitPoint; // 레이캐스트 구조체
        bool isHit = false; // 발사체 충돌 플래그
        bool isFXSpawned = false; // 충돌 FX 프리팹 생성 플래그
        float timer = 0f; // 발사체 타이머
        float fxOffset; // fxImpact의 오프셋

        void Awake()
        {
            // 변환 캐시 및 연결된 모든 입자 시스템 가져오기
            transform = GetComponent<Transform>();
            particles = GetComponentsInChildren<ParticleSystem>();
        }

        // 풀 매니저에 의해 호출되는 OnSpawned
        public void OnSpawned()
        {
            // 플래그 및 레이캐스트 구조체 재설정
            isHit = false;
            isFXSpawned = false;
            timer = 0f;
            hitPoint = new RaycastHit();
        }

        // 풀 매니저에 의해 호출되는 OnDespawned
        public void OnDespawned()
        {
        }

        // 연결된 입자 시스템의 방출을 중지하고 소멸되기 전에 페이드 아웃될 수 있도록 허용
        void Delay()
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

        // 풀 매니저에 의해 호출되는 OnDespawned
        void OnProjectileDestroy()
        {
            F3DPoolManager.Pools["GeneratedPool"].Despawn(transform);
        }

        // 충돌 시 힘 적용
        void ApplyForce(float force)
        {
            if (hitPoint.rigidbody != null)
                hitPoint.rigidbody.AddForceAtPosition(transform.forward * force, hitPoint.point,
                    ForceMode.VelocityChange);
        }

        void Update()
        {
            // 무언가가 충돌한 경우
            if (isHit)
            {
                // 한 번만 실행
                if (!isFXSpawned)
                {
                    // FX를 생성하는 해당 메소드를 호출
                    switch (fxType)
                    {
                        case F3DFXType.Vulcan:
                            F3DFXController.instance.VulcanImpact(hitPoint.point + hitPoint.normal * fxOffset);
                            /*ApplyForce(2.5f);*/
                            break;

                        case F3DFXType.SoloGun:
                            F3DFXController.instance.SoloGunImpact(hitPoint.point + hitPoint.normal * fxOffset);
                            /*ApplyForce(25f);*/
                            break;

                        case F3DFXType.Seeker:
                            F3DFXController.instance.SeekerImpact(hitPoint.point + hitPoint.normal * fxOffset);
                            /*ApplyForce(30f);*/
                            break;

                        case F3DFXType.PlasmaGun:
                            F3DFXController.instance.PlasmaGunImpact(hitPoint.point + hitPoint.normal * fxOffset);
                            /*ApplyForce(25f);*/
                            break;

                        case F3DFXType.LaserImpulse:
                            F3DFXController.instance.LaserImpulseImpact(hitPoint.point + hitPoint.normal * fxOffset);
                            /*ApplyForce(25f);*/
                            break;
                    }

                    isFXSpawned = true;
                }

                // 현재 발사체 소멸
                if (!DelayDespawn || (DelayDespawn && (timer >= despawnDelay)))
                    OnProjectileDestroy();
            }

            // 아직 충돌이 발생하지 않음
            else
            {
                // 발사체 단계당 프레임 기반 속도 및 시간
                Vector3 step = transform.forward * Time.deltaTime * velocity;

                // 레이캐스트 길이 기반의 대상을 위한 레이캐스트
                if (Physics.Raycast(transform.position, transform.forward, out hitPoint,
                    step.magnitude * RaycastAdvance,
                    layerMask))
                {
                    isHit = true;

                    // 필요한 경우 지연 루틴을 호출
                    if (DelayDespawn)
                    {
                        // 발사체 타이머를 재설정하고 입자 시스템이 올바르게 방출을 중지하고 페이드 아웃되도록 함
                        timer = 0f;
                        Delay();
                    }
                }
                // 아무것도 충돌하지 않음
                else
                {
                    // 시간이 다 되어 발사체 소멸
                    if (timer >= lifeTime)
                        OnProjectileDestroy();
                }

                // 발사체 전진
                transform.position += step;
            }

            // 발사체 타이머 업데이트
            timer += Time.deltaTime;
        }

        // 오프셋 설정
        public void SetOffset(float offset)
        {
            fxOffset = offset;
        }
    }
}