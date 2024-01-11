using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FORGE3D;

public class Tower_Attack : NetworkBehaviour
{
    public static Tower_Attack instance;
    public Head_Data head_Data;

    [Header("Turret Setup")]
    [SerializeField] private Transform[] turretSocket;

    [Header("Offset")]
    public float vulcanOffset;
    public float sniperOffset;
    public float seekerOffset;
    public float soloGunOffset;
    public float laserImpulseOffset;

    [SerializeField] private F3DFXController start_fire;
    [SerializeField] private Effect_Pooling pool;
    [SerializeField] private Transform mount;
    [SerializeField] private SFX_Manager manager;

    [SerializeField] private LayerMask target_Layer;
    public string towerName;
    public string towerType;
    public int H_Cost;
    public float H_Rot_Speed;
    public float H_Damage;
    public float H_ATK_Speed;
    public float H_ATK_Range;
    public int H_Reload;
    public Transform target;
    public Transform testTarget;
    public float current_ATK_Speed = 0;

    #region Unity Callback
    private void Start()
    {
        // Initialize singleton  
        instance = this;
        mount = transform.parent;
        Init_Data(head_Data);
        pool = FindObjectOfType<Effect_Pooling>();
        manager = FindObjectOfType<SFX_Manager>();
        //if (isServer)
        //{
        //    InvokeRepeating("Search_Enemy", 0f, 0.05f);
        //}
    }

    private void Update()
    {
        if (head_Data.atk_Type == Head_Data.Atk_Type.Scan) return;
       
        if (isServer)
        {
            Search_Enemy();

            if (target == null)
            {
                mount.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
            }
            else
            {
                if (target.GetComponent<Monster_Control>().isDie)
                {
                    target = null;
                }
                else
                {
                    Look_Target();
                }
            }


        }
    }
    #endregion
    #region SyncVar
    [SyncVar(hook = nameof(OnCurSocketChanged))]
    private int curSocket = 0;
    #endregion
    #region Client
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_AdvanceSocket()
    {
        curSocket++;
        if (curSocket >= turretSocket.Length)
            curSocket = 0;
    }
    [ClientRpc]
    private void RPC_Die(Monster_Control mon)
    {
        StartCoroutine(mon.onDie());
    }
    #endregion
    #region Hook Method
    private void OnCurSocketChanged(int old_, int new_)
    {
        curSocket = new_;
    }
    #endregion
    #region Server
    [Server]
    private void Fire()
    {
        switch(head_Data.atk_Type)
        {
            case Head_Data.Atk_Type.Vulcan: Projectile(head_Data.atk_Type, 1, 0); break;
            case Head_Data.Atk_Type.Sniper: Beam(head_Data.atk_Type, 4, 3); break;
            case Head_Data.Atk_Type.Laser: Beam(head_Data.atk_Type, 6); break;
            case Head_Data.Atk_Type.Missile: break;
            case Head_Data.Atk_Type.Seeker: Projectile(head_Data.atk_Type, 11, 10); break;
            case Head_Data.Atk_Type.Air: Projectile(head_Data.atk_Type, 14, 13); break;
            case Head_Data.Atk_Type.Flame: Beam(head_Data.atk_Type, 16); break;
            case Head_Data.Atk_Type.PlasmaBeam: Beam(head_Data.atk_Type, 17); break;
            case Head_Data.Atk_Type.LaserImpulse: Projectile(head_Data.atk_Type, 19, 18); break;
        }
    }
    #endregion

    private void Look_Target()
    {
        // 조건에 안맞으면 그냥 뺑뺑이
        Monster_Control mon = target.gameObject.GetComponent<Monster_Control>();
        if (mon.state.type == MonsterState.monType.Fly && head_Data.atk_Area == Head_Data.Atk_Area.Ground) return;
        else if (mon.state.type != MonsterState.monType.Fly && head_Data.atk_Area == Head_Data.Atk_Area.Air) return;
        else if (mon.isInvi || mon.isDie) return; //대상이 투명상태이면 공격못함 / 죽어도 공격안함

        float M_Rot_Speed = mount.gameObject.GetComponent<Tower_Mount>().M_Rot_Speed;
        Quaternion look_Rot = Quaternion.LookRotation(target.position - transform.position);
        Vector3 m_euler = Quaternion.RotateTowards(mount.rotation, look_Rot, M_Rot_Speed * Time.deltaTime).eulerAngles;
        mount.rotation = Quaternion.Euler(0, m_euler.y, 0);
        Quaternion fire_Rot = Quaternion.Euler(0, look_Rot.eulerAngles.y, 0);

       
            current_ATK_Speed -= Time.deltaTime;
     

        if (Quaternion.Angle(mount.rotation, fire_Rot) < 3f)
        {

            if (current_ATK_Speed <= 0)
            {
                StartCoroutine(Calculate_Fire(target));
                
                current_ATK_Speed = H_ATK_Speed;
            }
        }
    }

    private void Init_Data(Head_Data head_Data)
    {
        H_Cost = head_Data.Cost;
        H_Rot_Speed = head_Data.Rot_Speed;
        H_Damage = head_Data.Damage;
        H_ATK_Speed = head_Data.ATK_Speed;
        H_ATK_Range = head_Data.ATK_Range;
        H_Reload = head_Data.Reload;
    }

    private void Search_Enemy()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, H_ATK_Range, target_Layer);

        if(cols.Length > 0 && target == null)
        {
            Transform nearest_target = null;
            float Near_Dis = Mathf.Infinity;
            foreach (Collider coltarget in cols)
            {
                if (coltarget.gameObject.GetComponent<Monster_Control>().isInvi) continue;
                if (coltarget.gameObject.GetComponent<Monster_Control>().isDie) continue;
                float dis = Vector3.SqrMagnitude(transform.position - coltarget.transform.position);
                if (Near_Dis > dis) 
                {
                    Near_Dis = dis;
                    nearest_target = coltarget.transform;

                   
                    target = nearest_target;
                }
            }
        }

        CheckTargetOutOfRange();
    }

    private void CheckTargetOutOfRange()
    {
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > H_ATK_Range)
            {
                // 타겟이 공격 범위를 벗어났으므로 null로 설정
                target = null;
            }
        }
    }
    private IEnumerator Calculate_Fire(Transform _target)
    {
        Monster_Control mon = _target.GetComponent<Monster_Control>();
        switch (head_Data.weapon_Type)
        {
            case Head_Data.Weapon_Type.Targeting:
                Fire();
                yield return new WaitForSeconds(head_Data.DelayTime);
                mon.M_currentHP -= H_Damage;
                if (mon.M_currentHP <= 0)
                {
                    StartCoroutine(mon.onDie());
                    RPC_Die(mon);

                }
                break;
            case Head_Data.Weapon_Type.Splash:
                Fire();
                if (head_Data.atk_Type == Head_Data.Atk_Type.Flame)
                {
                    FlameAttack();
                }
                else if (head_Data.atk_Type == Head_Data.Atk_Type.Seeker)
                {
                    yield return new WaitForSeconds(head_Data.DelayTime);
                    SeekerAttack();
                }

                break;
        }
    }

    private void UnitsInRange_Damage(Collider[] col)
    {
        foreach (Collider item in col)
        {
            //item.collider.gameObject
            Monster_Control monster = item.gameObject.GetComponent<Monster_Control>();
            if (monster.isDie || monster.isInvi ) continue;

            monster.M_currentHP -= H_Damage;
            if (monster.M_currentHP <= 0)
            {
                StartCoroutine(monster.onDie());
                RPC_Die(monster);

            }
        }
    }

    private void SeekerAttack()
    {
        if (target == null) return;
        Collider[] col = Physics.OverlapSphere(target.position , 3, target_Layer);

        UnitsInRange_Damage(col);
        

    }

    private void FlameAttack()
    {
        Collider[] col = Physics.OverlapCapsule(transform.position + transform.forward, transform.position + transform.forward * 6.0f, 1.5f, target_Layer);
        Debug.Log(col.Length);

        UnitsInRange_Damage(col);
    }

    private void Projectile(Head_Data.Atk_Type atk_Type, int Muzzle_index, int Pro_index)
    {
        var offset = Quaternion.Euler(Random.onUnitSphere);

        switch (atk_Type)
        {
            case Head_Data.Atk_Type.Vulcan:
                // 총신 이펙트
                GameObject Muzzle_vul = pool.GetEffect(Muzzle_index);
                Set_Pos_Rot(Muzzle_vul, turretSocket[curSocket].position, turretSocket[curSocket].rotation);

                GameObject Projectile_vul = pool.GetEffect(Pro_index);
                Set_Pos_Rot(Projectile_vul, turretSocket[curSocket].position + turretSocket[curSocket].forward, offset * turretSocket[curSocket].rotation);

                var proj_vul = Projectile_vul.gameObject.GetComponent<Effect_Control>();
                proj_vul.target_ = target;
                if (proj_vul) { proj_vul.SetOffset(vulcanOffset); }

                manager.SFX_VulcanShot(turretSocket[curSocket].position);
                break;
            case Head_Data.Atk_Type.Missile:
                // 시간 남으면 구현
                break;
            case Head_Data.Atk_Type.Seeker:
                GameObject Muzzle_seeker = pool.GetEffect(Muzzle_index);
                Set_Pos_Rot(Muzzle_seeker, turretSocket[curSocket].position, turretSocket[curSocket].rotation);

                GameObject Projectile_seeker = pool.GetEffect(Pro_index);
                Set_Pos_Rot(Projectile_seeker, turretSocket[curSocket].position, offset * turretSocket[curSocket].rotation);

                var proj_seeker = Projectile_seeker.gameObject.GetComponent<Effect_Control>();
                proj_seeker.target_ = target;
                if (proj_seeker) { proj_seeker.SetOffset(seekerOffset); }

                manager.SFX_SeekerShot(turretSocket[curSocket].position);
                break;
            case Head_Data.Atk_Type.Air:
                GameObject Muzzle_Air = pool.GetEffect(Muzzle_index);
                Set_Pos_Rot(Muzzle_Air, turretSocket[curSocket].position, turretSocket[curSocket].rotation);

                GameObject Projectile_Air = pool.GetEffect(Pro_index);
                Set_Pos_Rot(Projectile_Air, turretSocket[curSocket].position, offset * turretSocket[curSocket].rotation);

                var proj_Air = Projectile_Air.gameObject.GetComponent<Effect_Control>();
                proj_Air.target_ = target;
                if (proj_Air) { proj_Air.SetOffset(soloGunOffset); }

                manager.SFX_AirShot(turretSocket[curSocket].position);
                break;
            case Head_Data.Atk_Type.LaserImpulse:
                GameObject Muzzle_LaserImpulse = pool.GetEffect(Muzzle_index);
                Set_Pos_Rot(Muzzle_LaserImpulse, turretSocket[curSocket].position, turretSocket[curSocket].rotation);

                GameObject Projectile_LaserImpulse = pool.GetEffect(Pro_index);
                Set_Pos_Rot(Projectile_LaserImpulse, turretSocket[curSocket].position, offset * turretSocket[curSocket].rotation);

                var proj_LaserImpulse = Projectile_LaserImpulse.gameObject.GetComponent<Effect_Control>();
                proj_LaserImpulse.target_ = target;
                if (proj_LaserImpulse) { proj_LaserImpulse.SetOffset(soloGunOffset); }

                manager.SFX_LaserImpulseShot(turretSocket[curSocket].position);
                break;
        }

        // 다음 총신에서 발사
        AdvanceSocket();
        RPC_AdvanceSocket();
    }

    private void Beam(Head_Data.Atk_Type atk_Type, int Muzzle_index, int Pro_index = -1)
    {
        switch(atk_Type)
        {
            case Head_Data.Atk_Type.Sniper:
                var offset = Quaternion.Euler(Random.onUnitSphere);
                // 총신 이펙트
                GameObject Muzzle_sni = pool.GetEffect(Muzzle_index);
                Set_Pos_Rot(Muzzle_sni, turretSocket[curSocket].position, turretSocket[curSocket].rotation);

                GameObject Projectile_sni = pool.GetEffect(Pro_index);
                Set_Pos_Rot(Projectile_sni, turretSocket[curSocket].position, offset * turretSocket[curSocket].rotation);

                var beam = Projectile_sni.GetComponent<Effect_Control>();
                beam.target_ = target;
                if (beam) { beam.SetOffset(sniperOffset); }

                manager.SFX_SniperShot(turretSocket[curSocket].position);
                break;
            case Head_Data.Atk_Type.Laser:
                for(int i = 0; i < turretSocket.Length; i++)
                {
                    GameObject laser = pool.GetEffect(Muzzle_index);
                    var laser_Beam = laser.GetComponent<Effect_Control>();
                    if(target != null)
                    {
                        laser_Beam.target_ = target;
                    }
                    Set_Pos_Rot(laser, turretSocket[i].position, turretSocket[i].rotation);
                }
                manager.SFX_Laser(turretSocket[0].position, 0.1f);
                break;
            case Head_Data.Atk_Type.Flame:
                GameObject flame = pool.GetEffect(Muzzle_index);
                var flame_ = GetComponent<Effect_Control>();
                Set_Pos_Rot(flame, turretSocket[0].position, turretSocket[0].rotation);
                manager.SFX_Flame(turretSocket[0].position, 0.1f);
                break;
            case Head_Data.Atk_Type.PlasmaBeam:
                for (int i = 0; i < turretSocket.Length; i++)
                {
                    GameObject laser = pool.GetEffect(Muzzle_index);
                    var plasma = laser.GetComponent<Effect_Control>();
                    plasma.target_ = target;
                    Set_Pos_Rot(laser, turretSocket[i].position, turretSocket[i].rotation);
                }
                manager.SFX_Plasma(turretSocket[0].position);
                break;
        }

        // 다음 총신에서 발사
        AdvanceSocket();
        RPC_AdvanceSocket();
    }

    // Advance to next turret socket
    private void AdvanceSocket()
    {
        curSocket++;
        if (curSocket >= turretSocket.Length)
            curSocket = 0;
    }

    public void Set_Pos_Rot(GameObject obj, Vector3 pos, Quaternion rot)
    {
        obj.transform.position = pos;
        if (target != null)
        {
            // 머즐에서 타겟을 향하는 벡터
            Vector3 directionToTarget = target.position - pos;
            // 현재 방향
            Vector3 currentDirection = rot * Vector3.forward;

            // 두 벡터 간의 회전값 계산
            Quaternion rotationToTarget = Quaternion.FromToRotation(currentDirection, directionToTarget);

            // 현재 로테이션에 두 벡터 간의 회전값을 더하여 새로운 로테이션 계산
            Quaternion finalRotation = rot * rotationToTarget;

            obj.transform.rotation = finalRotation;
        }
        else
        {
            obj.transform.rotation = rot;
        }
        GameManager.instance.RPC_TransformSet(obj, pos, rot);
    }
}
