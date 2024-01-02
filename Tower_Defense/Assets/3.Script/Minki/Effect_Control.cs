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
        Flame,
        impact
    }

    public Head_Data head_Data;
    public Effect_type type;
    private Effect_Pooling pool;
    private LineRenderer lineRenderer;

    [Header("�ð� ����")]
    [SerializeField] private float delay;       // Ȱ��ȭ �� ���� delay �ڿ� ��Ȱ��ȭ �ϴ� �ð�
    public float despawnDelay;                  // hit�� �� ����Ʈ Ȱ��ȭ �ǰ� ������� �ð�
    public float timer = 0f;                   // �ǽð� Ÿ�̸�

    [Header("Bool��")]
    public bool DelayDespawn = false;           
    public bool OneShot;                        // �� ���� ���� ������ ���� ������
    public bool isHit = false;                 // ��Ʈ �ƴ��� �ȵƴ���
    private bool isFXSpawned = false;           // ����Ʈ�� �������� ����������
    private bool despawn;                       // �Ҹ� ���� �÷���
    private bool dontRot = false;

    [Header("��ƼŬ")]
    public ParticleSystem[] delayedParticles;   
    private ParticleSystem[] particles;
    private ParticleSystem flame;               // ȭ������

    [Header("��ġ ����")]
    new Transform transform;                    // ĳ�� �� Ʈ������
    private float fxOffset;                     // ����Ʈ�� offset
    public float velocity = 300;                // ����ü �ӵ�
    public Transform rayImpact;                 // �浹 Ʈ������
    public Transform rayMuzzle;                 // �߻� �÷��� Ʈ������
    private float initialBeamOffset;            // �ʱ� UV ������

    [Header("Ray")]
    private RaycastHit hitPoint;                // 3D ����ĳ��Ʈ
    private RaycastHit2D hitPoint2D;            // 2D ����ĳ��Ʈ
    public float RaycastAdvance = 2f;           // ����ĳ��Ʈ ���� ����
    public LayerMask layerMask;                 // ���̾� ����ũ

    [Header("�ִϸ��̼�")]
    public Texture[] BeamFrames;                // �ִϸ��̼� ������ ����
    public float FrameStep;                     // �ִϸ��̼� ����
    public bool AnimateUV;                      // UV �ִϸ��̼� ����
    public float UVTime;                        // UV �ִϸ��̼� �ӵ�
    private int frameNo;                        // ������ ī����
    private int FrameTimerID;                   // ������ Ÿ�̸� ����
    private float animateUVTime;

    [Header("Beam")]
    public float beamScale;                     // �⺻ �� ������
    public float MaxBeamLength;                 // �ִ� �� ����
    private float beamLength;                   // ���� �� ����

    [Header("Flame")]
    public Light pLight;                        // ����� ����Ʈ
    public ParticleSystem heat;                 // heat ��ƼŬ
    private int lightState;                     // ����Ʈ ����Ʈ ���� �÷��� (fade in, out)

    public Transform target_;
    public Quaternion current_Rot;
    [SerializeField] private float homingSpeed = 5f;

    #region Unity Callback
    private void Awake()
    {
        pool = FindObjectOfType<Effect_Pooling>();
        transform = GetComponent<Transform>();
        // ��ȯ ĳ�� �� ����� ��� ���� �ý��� ��������
        if (type == Effect_type.projectile)
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }
        else if(type == Effect_type.Beam)
        {
            // ���� ������ ������Ʈ ��������
            lineRenderer = GetComponent<LineRenderer>();

            // UV �ִϸ��̼� ������� �ʰ� BeamFrames �迭�� 1�� �̻��� ��� ù ��° ������ �ؽ�ó �Ҵ�
            if (!AnimateUV && BeamFrames.Length > 0)
                lineRenderer.material.mainTexture = BeamFrames[0];

            // ������ UV ������ ����
            initialBeamOffset = Random.Range(0f, 5f);
        }
        else if(type == Effect_type.Flame)
        {
            flame = GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (type == Effect_type.projectile && !dontRot)
        {
            isProjectile(target_);
        }
        else if(type == Effect_type.Beam)
        {
            // �ؽ�ó UV �ִϸ��̼�
            if (AnimateUV)
            {
                animateUVTime += Time.deltaTime;

                if (animateUVTime > 1.0f)
                    animateUVTime = 0f;
                var v = animateUVTime * UVTime + initialBeamOffset;
                lineRenderer.material.SetTextureOffset("_BaseMap", new Vector2(v, 0));
            }

            // ������ ���� ���� ����ĳ��Ʈ
            if (!OneShot && target_ != null)
                isBeam(target_);
        }
        else if(type ==  Effect_type.Flame)
        {
            StartCoroutine(Flame());
        }
    }

    // Ȱ��ȭ �� �ʱ�ȭ �� ���� �ð� �� ��Ƽ�� ����
    private void OnEnable()
    {
        if (isServer)
        {
            if (type == Effect_type.projectile) { OnSpawned_Pro(); }
            else if (type == Effect_type.Beam) { OnSpawned_Beam(); }
            else if (type == Effect_type.Flame) { OnSpawned_Flame(); }
        }
        else
        {
            if(type == Effect_type.projectile) { OnSpawned_Pro(); }
            else if (type == Effect_type.Flame) { OnSpawned_Flame(); }
        }

        Invoke("active", delay);
    }

    // ������ delay���� ���� ������ active �κ�ũ ����
    private void OnDisable()
    {
        if(isServer)
        {
            if(type == Effect_type.Beam)
            {
                // ������ ī���� �ʱ�ȭ
                frameNo = 0;

                // Ÿ�̸� �ʱ�ȭ
                if (FrameTimerID != -1)
                {
                    FrameTimerID = -1;
                }
                target_ = null;
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
                Transform_Set(vul_impact, pos, Quaternion.identity);
                GameManager.instance.RPC_TransformSet(vul_impact, pos, Quaternion.identity);
                break;
            case Head_Data.Atk_Type.Sniper:
                GameObject sni_impact = pool.GetEffect(index);
                Transform_Set(sni_impact, pos, Quaternion.identity);
                GameManager.instance.RPC_TransformSet(sni_impact, pos, Quaternion.identity);
                break;
            case Head_Data.Atk_Type.Missile:
                break;
            case Head_Data.Atk_Type.Seeker:
                GameObject Seeker_impact = pool.GetEffect(index);
                Transform_Set(Seeker_impact, pos, Quaternion.identity);
                GameManager.instance.RPC_TransformSet(Seeker_impact, pos, Quaternion.identity);
                break;
            case Head_Data.Atk_Type.Air:
                GameObject Air_impact = pool.GetEffect(index);
                Transform_Set(Air_impact, pos, Quaternion.identity);
                GameManager.instance.RPC_TransformSet(Air_impact, pos, Quaternion.identity);
                break;
            case Head_Data.Atk_Type.LaserImpulse:
                GameObject LaserImpulse_impact = pool.GetEffect(index);
                Transform_Set(LaserImpulse_impact, pos, Quaternion.identity);
                GameManager.instance.RPC_TransformSet(LaserImpulse_impact, pos, Quaternion.identity);
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
        // �÷��� �� ����ĳ��Ʈ ����ü �缳��
        isHit = false;
        dontRot = false;
        isFXSpawned = false;
        timer = 0f;
        hitPoint = new RaycastHit();
    }

    private void OnSpawned_Beam()
    {
        // OneShot �÷��װ� true�� ��� �� ���� ����ĳ��Ʈ ����
        if (OneShot && target_ != null)
            isBeam(target_);

        // BeamFrames �迭�� 2�� �̻��� ��� �ִϸ��̼� ����
        if (BeamFrames.Length > 1)
            Animate();
    }

    private void OnSpawned_Flame()
    {
        despawn = false;

        lightState = 1;
        pLight.intensity = 0f;
    }

    private void isProjectile(Transform target)
    {
        // ���� �浹�� ���
        if(isHit)
        {
            // ����Ʈ �ѹ��� ����
            if (!isFXSpawned)
            {
                // ����Ʈ�� �����ϴ� �ش� �޼ҵ带 ȣ��
                switch (head_Data.atk_Type)
                {
                    case Head_Data.Atk_Type.Vulcan:
                        if (isServer) { Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 2); }
                        break;
                    case Head_Data.Atk_Type.Missile:
                        break;
                    case Head_Data.Atk_Type.Seeker:
                        if (isServer) { Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 12); }
                        break;
                    case Head_Data.Atk_Type.Air:
                        if(isServer) { Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 15); }
                        break;
                    case Head_Data.Atk_Type.LaserImpulse:
                        if(isServer) { Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 20); }
                        break;
                }

                isFXSpawned = true;
                dontRot = true;
            }

            // ���� �߻�ü �Ҹ�
            if (DelayDespawn && (timer >= despawnDelay))
            {
                active();
            }
            else if(!DelayDespawn)
            {
                active();
            }
        }

        // ���� �浹�� �߻����� �ʾ��� ��
        else
        {
            current_Rot = transform.rotation;
            // �߻�ü �̵� ����
            Vector3 targetDirection = (target_.position - transform.position).normalized;

            // ����ĳ��Ʈ ���� ����� ���
            /*if(Vector3.Distance(transform.position, target.position)<0.5f)*/
            if (Physics.Raycast(transform.position, transform.forward, out hitPoint, targetDirection.magnitude * RaycastAdvance, layerMask))
            {
                isHit = true;
                // �ʿ��� ��� ���� ��ƾ
                if (DelayDespawn)
                {
                    // �߻�ü Ÿ�̸Ӹ� �缳���ϰ� ��ƼŬ �ý����� �ùٸ��� ������ �����ϰ� ���̵� �ƿ�
                    timer = 0f;
                    Delay();
                }

                return;
            }

            // �ƹ��͵� �浹���� ����
            else
            {
                // �ð��� �� �Ǿ� �߻�ü �Ҹ�
                if (timer >= delay)
                    active();
            }

            // �߻�ü ����
            float step = homingSpeed * Time.deltaTime;

            // Ÿ�ٰ��� �Ÿ��� ���� �����̼� ����
            if (Vector3.Distance(transform.position, target_.position) > 0.5f)
            {
                transform.position += targetDirection * step;

                // ��Ʈ�� ���������� ���� ���͸� ����Ͽ� �����̼� ����
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // ���� �����̼ǿ��� ���� ����� �����̼����� �ε巴�� ��ȯ
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
            }
        }
        // �߻�ü Ÿ�̸� ������Ʈ
        timer += Time.deltaTime;
    }

    private void isBeam(Transform target)
    {
        // ����ü �ʱ�ȭ �� ���� ����
        hitPoint = new RaycastHit();
        
        Vector3 directionToTarget = (target_.position - transform.position).normalized;

        Ray ray = new Ray(transform.position, directionToTarget);
        Debug.DrawRay(ray.origin, ray.direction * MaxBeamLength, Color.red, 1f);
        // �⺻ ������ �� �ִ� ���̸� ��������� �⺻ �� ���� ��� ���
        float propMult = MaxBeamLength * (beamScale / 10f);
        // ����ĳ��Ʈ
        if (Physics.Raycast(ray, out hitPoint, MaxBeamLength, layerMask))
        {
            // ���� �� ���� �������� ���� ������ ������Ʈ
            beamLength = Vector3.Distance(transform.position, hitPoint.point);
            lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));

            // ���� ���̸� ��������� �⺻ �� ���� ��� ���
            propMult = beamLength * (beamScale / 10f);

            // ������ ����
            switch (head_Data.atk_Type)
            {
                case Head_Data.Atk_Type.Sniper:
                    Request_Impact(hitPoint.point + hitPoint.normal * fxOffset, 5);
                    break;
            }

            // �浹 ȿ�� ��ġ ����
            if (rayImpact)
                rayImpact.position = hitPoint.point - transform.forward * 0.5f;
        }
        // 2D ��忡�� Ȯ��
        else
        {
            RaycastHit2D ray2D = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y),
                new Vector2(transform.forward.x, transform.forward.y), beamLength, layerMask);
            if (ray2D)
            {
                // ���� �� ���� �������� ���� ������ ������Ʈ
                beamLength = Vector3.Distance(transform.position, ray2D.point);
                lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));
                Debug.Log(hitPoint.transform.position);

                // ���� ���̸� ��������� �⺻ �� ���� ��� ���
                propMult = beamLength * (beamScale / 10f);
                // ������ ����
                switch (head_Data.atk_Type)
                {
                    case Head_Data.Atk_Type.Sniper:
                        Request_Impact(ray2D.point + ray2D.normal * fxOffset, 5);
                        break;
                }

                // �浹 ȿ�� ��ġ ����
                if (rayImpact)
                    rayImpact.position = new Vector3(ray2D.point.x,
                                             ray2D.point.y,
                                             this.gameObject.transform.position.z) - transform.forward * 0.5f;
            }
            // �ƹ��͵� �浹���� �ʾ��� ��
            else
            {
                // ���� �ִ� ���̷� ����
                beamLength = MaxBeamLength;
                lineRenderer.SetPosition(1, new Vector3(0f, 0f, beamLength));

                // �浹 ȿ�� ��ġ ����
                if (rayImpact)
                    rayImpact.position = transform.position + transform.forward * beamLength;
            }
        }

        // ���� ��ġ ����
        if (rayMuzzle)
            rayMuzzle.position = transform.position + transform.forward * 0.1f;

        // �� ���̿� ���� �� ������ ����
        lineRenderer.material.SetTextureScale("_BaseMap", new Vector2(propMult, 1f));
    }

    private IEnumerator Flame()
    {
        if (!despawn)
        {
            yield return new WaitForSeconds(0.2f);
            // �Ҹ� �÷��� ���� �� ���ڰ� ������ ��������� �Ҹ� Ÿ�̸� �߰�
            despawn = true;

            // ���� �ý��� ����
            flame.Stop();
            if (heat)
                heat.Stop();

            // ����Ʈ ���� ��ȯ
            pLight.intensity = 0.6f;
            lightState = -1;

            // ����Ʈ ����Ʈ ���̵� ��
            if (lightState == 1)
            {
                pLight.intensity = Mathf.Lerp(pLight.intensity, 0.7f, Time.deltaTime * 10f);

                if (pLight.intensity >= 0.5f)
                    lightState = 0;
            }
            // ����Ʈ ����Ʈ ���̵� �ƿ�
            else if (lightState == -1)
            {
                pLight.intensity = Mathf.Lerp(pLight.intensity, -0.1f, Time.deltaTime * 10f);

                if (pLight.intensity <= 0f)
                    lightState = 0;
            }
        }
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

    // ������ ����
    public void SetOffset(float offset)
    {
        fxOffset = offset;
    }

    // �ִϸ��̼� ������ ����ȭ
    private IEnumerator AnimateFrames()
    {
        float timer = 0f;

        while (true)
        {
            // ���� �������� ������ ��ȯ
            timer += Time.deltaTime;
            if (timer >= FrameStep)
            {
                // ���� �����ӿ� ���� ���� �ؽ�ó ������ ����
                lineRenderer.material.mainTexture = BeamFrames[frameNo];
                frameNo++;

                // ������ ī���� �ʱ�ȭ
                if (frameNo == BeamFrames.Length)
                {
                    frameNo = 0;
                }

                timer = 0f;
            }

            // ���
            yield return null;
        }
    }

    // ������ �ִϸ��̼� �ʱ�ȭ
    private void Animate()
    {
        if (BeamFrames.Length > 1)
        {
            // ���� ������ ����
            frameNo = 0;
            lineRenderer.material.mainTexture = BeamFrames[frameNo];

            // �ڷ�ƾ���� �ִϸ��̼� ����
            StartCoroutine(AnimateFrames());
        }
    }

    private void Transform_Set(GameObject gameObject, Vector3 pos, Quaternion rot)
    {
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
    }
}
