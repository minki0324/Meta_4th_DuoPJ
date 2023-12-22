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

    [Header("Vulcan")]
    public float vulcanOffset;

    [SerializeField] private F3DFXController start_fire;
    [SerializeField] private Effect_Pooling pool;
    [SerializeField] private Transform mount;
    [SerializeField] private LayerMask target_Layer;
    private Transform target;

    public int H_Cost;
    public float H_Rot_Speed;
    public float H_Damage;
    public float H_ATK_Speed;
    public float H_ATK_Range;
    public int H_Reload;

    public float current_ATK_Speed;
    #region Unity Callback
    private void Start()
    {
        // Initialize singleton  
        instance = this;
        mount = transform.root.GetChild(1);
        Init_Data(head_Data);
        pool = FindObjectOfType<Effect_Pooling>();
        if (isServer)
        {
            InvokeRepeating("Search_Enemy", 0f, 0.5f);
        }
    }

    private void Update()
    {
        if (isServer)
        {
            if (target == null)
            {
                mount.Rotate(new Vector3(0, 45, 0) * Time.deltaTime);
            }
            else
            {
                Look_Target();
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
            case Head_Data.Atk_Type.Vulcan:
                vulcan();
                break;
        }
    }
    #endregion

    private void Look_Target()
    {
        float M_Rot_Speed = mount.gameObject.GetComponent<Tower_Mount>().M_Rot_Speed;
        Quaternion look_Rot = Quaternion.LookRotation(target.position - transform.position);
        Vector3 m_euler = Quaternion.RotateTowards(mount.rotation, look_Rot, M_Rot_Speed * Time.deltaTime).eulerAngles;
        Vector3 h_euler = Quaternion.Slerp(transform.rotation, Quaternion.Euler(look_Rot.eulerAngles.x, look_Rot.eulerAngles.y, 0), H_Rot_Speed * Time.deltaTime).eulerAngles;
       /* h_euler.x = Mathf.Clamp(h_euler.x, -20f, 20f);
        transform.rotation = Quaternion.Euler(h_euler.x, 0, 0);*/
        mount.rotation = Quaternion.Euler(0, m_euler.y, 0);
        Quaternion fire_Rot = Quaternion.Euler(0, look_Rot.eulerAngles.y, 0);

        if (Quaternion.Angle(mount.rotation, fire_Rot) < 5f)
        {
            current_ATK_Speed -= Time.deltaTime;
            if (current_ATK_Speed <= 0)
            {
                current_ATK_Speed = H_ATK_Speed;
                Fire();
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
        current_ATK_Speed = H_ATK_Speed;
    }

    private void Search_Enemy()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, H_ATK_Range, target_Layer);
        Transform nearest_target = null;
        if(cols.Length > 0)
        {
            float Near_Dis = Mathf.Infinity;
            foreach (Collider coltarget in cols)
            {
                float dis = Vector3.SqrMagnitude(transform.position - coltarget.transform.position);
                if (Near_Dis > dis) 
                {
                    Near_Dis = dis;
                    nearest_target = coltarget.transform;
                }
            }
        }

        target = nearest_target;
    }

    private IEnumerator Calculate_Fire(Head_Data head_Data, Transform target)
    {
        Monster_Control mon = target.GetComponent<Monster_Control>();
        switch(head_Data.weapon_Type)
        {
            case Head_Data.Weapon_Type.Targeting:
                if(mon.state.type == MonsterState.monType.Fly)
                {
                    if(head_Data.atk_Area == Head_Data.Atk_Area.Ground)
                    {
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(head_Data.DelayTime);
                    Monster_Control mon_con = target.gameObject.GetComponent<Monster_Control>();

                }
                break;
            case Head_Data.Weapon_Type.Splash:
                break;
        }
    }

    private void vulcan()
    {
        var offset = Quaternion.Euler(Random.onUnitSphere);
        
        // 총신 이펙트
        GameObject Muzzle = pool.GetEffect(1);
        Set_Pos_Rot(Muzzle, turretSocket[curSocket].position, turretSocket[curSocket].rotation);

        GameObject Projectile = pool.GetEffect(0);
        Set_Pos_Rot(Projectile, turretSocket[curSocket].position + turretSocket[curSocket].forward, offset * turretSocket[curSocket].rotation);

        var proj = Projectile.gameObject.GetComponent<F3DProjectile>();
        if(proj)
        {
            proj.SetOffset(vulcanOffset);
        }

        // 다음 총신에서 발사
        AdvanceSocket();
        RPC_AdvanceSocket();
    }

    public void VulcanImpact(Vector3 pos)
    {
        GameObject impact = pool.GetEffect(2);
        Set_Pos_Rot(impact, pos, Quaternion.identity);
    }

    // Advance to next turret socket
    private void AdvanceSocket()
    {
        curSocket++;
        if (curSocket >= turretSocket.Length)
            curSocket = 0;
    }

    private void Set_Pos_Rot(GameObject obj, Vector3 pos, Quaternion rot)
    {
        if(obj == null)
        {
            Debug.Log("obj 가 널");
        }
        else if (pos == null)
        {
            Debug.Log("pos 가 널");
        }
        else if (rot == null)
        {
            Debug.Log("rot 가 널");
        }
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        GameManager.instance.RPC_TransformSet(obj, pos, rot);
    }
}
