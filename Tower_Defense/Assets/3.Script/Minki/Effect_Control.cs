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
    [Header("�ð� ����")]
    [SerializeField] private float delay;       // Ȱ��ȭ �� ���� delay �ڿ� ��Ȱ��ȭ �ϴ� �ð�
    public float despawnDelay;                  // hit�� �� ����Ʈ Ȱ��ȭ �ǰ� ������� �ð�
    private float timer = 0f;                   // �ǽð� Ÿ�̸�

    [Header("Bool��")]
    public bool DelayDespawn = false;           
    private bool isHit = false;                 // ��Ʈ �ƴ��� �ȵƴ���
    private bool isFXSpawned = false;           // ����Ʈ�� �������� ����������

    [Header("��ƼŬ")]
    public ParticleSystem[] delayedParticles;   
    private ParticleSystem[] particles;

    [Header("��ġ ����")]
    new Transform transform;                    // ĳ�� �� Ʈ������
    private float fxOffset;                     // ����Ʈ�� offset
    public float velocity = 300;                // ����ü �ӵ�

    [Header("Ray")]
    private RaycastHit hitPoint;                // ���̰� ���� ��ġ
    public float RaycastAdvance = 2f;           // ����ĳ��Ʈ ���� ����
    public LayerMask layerMask;                 // ���̾� ����ũ

    #region Unity Callback
    private void Awake()
    {
        // ��ȯ ĳ�� �� ����� ��� ���� �ý��� ��������
        transform = GetComponent<Transform>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        isProjectile();
    }

    // Ȱ��ȭ �� �ʱ�ȭ �� ���� �ð� �� ��Ƽ�� ����
    private void OnEnable()
    {
        if(type == Effect_type.projectile)
        {
            OnSpawned();
        }
        Invoke("active", delay);
    }

    // ������ delay���� ���� ������ active �κ�ũ ����
    private void OnDisable()
    {
        CancelInvoke("active");
    }
    #endregion

    #region SyncVar
    #endregion
    #region Client
    [Client]
    private void Request_Impact()
    {
        CMD_Request_Impact();
    }
    #endregion
    #region Command
    [Command(requiresAuthority = false)]
    private void CMD_Request_Impact()
    {
        Vector3 pos = hitPoint.point + hitPoint.normal * fxOffset;
        Tower_Attack.instance.VulcanImpact(pos);
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
        // �÷��� �� ����ĳ��Ʈ ����ü �缳��
        isHit = false;
        isFXSpawned = false;
        timer = 0f;
        hitPoint = new RaycastHit();
    }

    private void isProjectile()
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
                        if(isClient)
                        {
                            Request_Impact();
                        }
                        else if(isServer)
                        {
                            Tower_Attack.instance.VulcanImpact(hitPoint.point + hitPoint.normal * fxOffset);
                        }
                        break;
                }

                isFXSpawned = true;
            }

            // ���� �߻�ü �Ҹ�
            if (!DelayDespawn || (DelayDespawn && (timer >= despawnDelay)))
                active();
        }

        // ���� �浹�� �߻����� �ʾ��� ��
        else
        {
            // �߻�ü �̵� ���� �� �ӵ�
            Vector3 step = transform.forward * Time.deltaTime * velocity;

            // ����ĳ��Ʈ ���� ����� ���
            if (Physics.Raycast(transform.position, transform.forward, out hitPoint, step.magnitude * RaycastAdvance, layerMask))
            {
                isHit = true;

                // �ʿ��� ��� ���� ��ƾ
                if (DelayDespawn)
                {
                    // �߻�ü Ÿ�̸Ӹ� �缳���ϰ� ��ƼŬ �ý����� �ùٸ��� ������ �����ϰ� ���̵� �ƿ�
                    timer = 0f;
                    Delay();
                }
            }

            // �ƹ��͵� �浹���� ����
            else
            {
                // �ð��� �� �Ǿ� �߻�ü �Ҹ�
                if (timer >= delay)
                    active();
            }

            // �߻�ü ����
            transform.position += step;
        }
        // �߻�ü Ÿ�̸� ������Ʈ
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

    // ������ ����
    public void SetOffset(float offset)
    {
        fxOffset = offset;
    }

    
}
