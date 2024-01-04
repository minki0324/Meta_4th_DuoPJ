using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogScript : MonoBehaviour
{
    [SerializeField] private GameObject income_panel_1;
    [SerializeField] private GameObject income_panel_2;
    [SerializeField] private GameObject minimap_panel;
    [SerializeField] private GameObject util_panel;
    [SerializeField] private GameObject info_panel;
    [SerializeField] private GameObject order_panel;
    [SerializeField] private GameObject income_timer;
    [SerializeField] private GameObject life_panel;
    [SerializeField] private GameObject resourse_panel;
    [SerializeField] private Image blur;
    [SerializeField] private GameObject text_Panel;

    [SerializeField] private GameObject Tutorial;

    [SerializeField] private DollyCam_Break dolly;

    private void Start()
    {
       StartCoroutine(Print_Script_co(1));
    }

    public IEnumerator Print_Script_co(int eventID)
    {
        switch(eventID)
        {
            case 1: // 처음 시작 
                StartCoroutine(Move_TxtPanel_co(true));
                yield return new WaitForSeconds(1.5f);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 2: // 자원 현황판
                resourse_panel.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 3: // 인컴 타이머
                resourse_panel.SetActive(false);
                income_timer.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 4: // 라이프 현황판
                income_timer.SetActive(false);
                life_panel.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 5: // 인컴 현황판
                life_panel.SetActive(false);
                income_panel_2.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 6: // 유틸, 오더 판넬
                income_panel_2.SetActive(false);
                util_panel.SetActive(true);
                order_panel.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 7: // 인포 판넬
                util_panel.SetActive(false);
                order_panel.SetActive(false);
                info_panel.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 8: // 미니맵
                info_panel.SetActive(false);
                minimap_panel.SetActive(true);
                StartCoroutine(Next_Dialog(eventID));
                break;

            case 9: // 마지막 대사
                StartCoroutine(Fade.instance.fade(blur, true));
                yield return new WaitForSeconds(0.3f);
                DialogPrint.instance.PrintEvent("9");
                while (DialogPrint.instance.isEnter)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(0.6f);
                Tutorial.SetActive(false);
                yield return new WaitForSeconds(1f);
                StartCoroutine(dolly.AdjustDollySpeed());
                break;
        }
    }

    private IEnumerator Next_Dialog(int print_index)
    {
        // 뒷 배경 블러처리
        StartCoroutine(Fade.instance.fade(blur, true));
        yield return new WaitForSeconds(0.3f);

        // 대사 출력
        DialogPrint.instance.PrintEvent(print_index.ToString());

        // 인컴 판넬은 바깥에서 안쪽으로 이동해야함
        if(print_index == 5)
        {
            StartCoroutine(Move_IncomePanel_co(true));
        }

        // 스크립트가 완료될때까지 대기
        while (DialogPrint.instance.isEnter)
        {
            yield return null;
        }

        // 블러 해제
        StartCoroutine(Fade.instance.fade(blur, false));

        // 인컴판넬 다시 집어넣기
        if(print_index == 5)
        {
            StartCoroutine(Move_IncomePanel_co(false));
        }
        yield return new WaitForSeconds(0.4f);

        // 다음 스크립트로 넘어감
        yield return StartCoroutine(Print_Script_co(print_index+1));

    }

    private IEnumerator Move_TxtPanel_co(bool isStart)
    {
        RectTransform textPanelRectTransform = text_Panel.GetComponent<RectTransform>();
        float targetX = isStart ? 50f : -320f;
        float startX = isStart ? -320f : 50f;
        float duration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            textPanelRectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, elapsedTime / duration), textPanelRectTransform.anchoredPosition.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textPanelRectTransform.anchoredPosition = new Vector2(targetX, textPanelRectTransform.anchoredPosition.y);
    }

    private IEnumerator Move_IncomePanel_co(bool isStart)
    {
        RectTransform textPanelRectTransform_1 = income_panel_1.GetComponent<RectTransform>();
        RectTransform textPanelRectTransform_2 = income_panel_2.GetComponent<RectTransform>();
        float targetX = isStart ? -185f : 0f;
        float startX = isStart ? 0f : -185f;
        float duration = 1.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            textPanelRectTransform_1.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, elapsedTime / duration), textPanelRectTransform_1.anchoredPosition.y);
            textPanelRectTransform_2.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, elapsedTime / duration), textPanelRectTransform_2.anchoredPosition.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textPanelRectTransform_1.anchoredPosition = new Vector2(targetX, textPanelRectTransform_1.anchoredPosition.y);
        textPanelRectTransform_2.anchoredPosition = new Vector2(targetX, textPanelRectTransform_2.anchoredPosition.y);
    }
}
