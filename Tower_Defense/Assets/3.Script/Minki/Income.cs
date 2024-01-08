using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Income : NetworkBehaviour
{
    [SerializeField] private Text income_timer;
    [SerializeField] private GameObject income_panel;
    private float current_timer;
    private bool panel_open = false;

    [SerializeField] private Text P1_txt;
    [SerializeField] private Text P2_txt;
    [SerializeField] private Text P3_txt;
    [SerializeField] private Text P4_txt;
    #region Unity Callback

    private void Update()
    {
        if(current_timer > 45)
        {
            // 각 플레이어들에게 인컴 지급 및 타이머 초기화

            current_timer = 0;
        }

        // 인컴 판넬 동기화 및 업데이트
        if(isServer)
        {
            RPC_Print_income_Timer();
        }

        current_timer += Time.deltaTime;
    }
    #endregion
    #region SyncVar
    [SyncVar(hook = nameof(Hook_P1_income))]
    public int P1_income = 5;
    [SyncVar(hook = nameof(Hook_P2_income))]
    public int P2_income = 5;
    [SyncVar(hook = nameof(Hook_P3_income))]
    public int P3_income = 5;
    [SyncVar(hook = nameof(Hook_P4_income))]
    public int P4_income = 5;
    #endregion
    #region Client
    #endregion
    #region Command
    #endregion
    #region ClientRPC
    [ClientRpc]
    private void RPC_Print_income_Timer()
    {
        P1_txt.text = string.Format("{0:N0}\n1", P1_income);
        P2_txt.text = string.Format("{0:N0}\n1", P2_income);
        P3_txt.text = string.Format("{0:N0}\n1", P3_income);
        P4_txt.text = string.Format("{0:N0}\n1", P4_income);
        income_timer.text = $"NEXT INCOME : " + (int)current_timer;
    }
    #endregion
    #region Hook Method
    private void Hook_P1_income(int old_, int new_)
    {
        P1_income = new_;
    }
    private void Hook_P2_income(int old_, int new_)
    {
        P2_income = new_;
    }
    private void Hook_P3_income(int old_, int new_)
    {
        P3_income = new_;
    }
    private void Hook_P4_income(int old_, int new_)
    {
        P4_income = new_;
    }
    #endregion

    // 판넬 열고닫고 버튼 콜백
    public void OnClick()
    {
        StartCoroutine(Move_IncomePanel_co(panel_open));
    }

        // 실제 판넬 열고 닫는 메소드
    private IEnumerator Move_IncomePanel_co(bool isStart)
    {
        RectTransform textPanelRectTransform_1 = income_panel.GetComponent<RectTransform>();
        float targetX = isStart ? 0f : -400f;
        float startX = isStart ? -400f : 0f;
        float duration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            textPanelRectTransform_1.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, elapsedTime / duration), textPanelRectTransform_1.anchoredPosition.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textPanelRectTransform_1.anchoredPosition = new Vector2(targetX, textPanelRectTransform_1.anchoredPosition.y);
        panel_open = !panel_open;
    }
}
