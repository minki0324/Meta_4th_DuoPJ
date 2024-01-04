using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public struct DialogData
{
    public string characterName;
    public string context;
}

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    [Header("CSV")]
    [SerializeField] private TextAsset csvFile = null;

    public Dictionary<string, DialogData[]> dialogDictionary = new Dictionary<string, DialogData[]>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        SetDialogDictionary();
    }

    private void SetDialogDictionary()
    {
        string csvText = csvFile.text.Substring(0, csvFile.text.Length - 1);    // 맨 끝 EndLine 제거
        Debug.Log(csvText);
        string[] rows = csvText.Split('\n');                                    // 줄바꿈 문자를 기준으로 csv파일을 쪼개서 rows 배열에 저장
        List<DialogData> dialogDatasList = new List<DialogData>();              // 대화의 길이를 모르기 때문에 List로 선언 후 배열로 변환하여 저장

        string eventID = "";

        for(int i = 1; i < rows.Length; i++)                                    // rows에 담긴 모든 행을 순회 (0번째 행은 제외)
        {
            // rows[i] = 이벤트ID,캐릭터이름,대사 (붙어있음)
            string[] currentLine = rows[i].Split(',');                          // 이벤트ID, 캐릭터이름, 대사 나누어 저장 (0 = ID, 1 = 캐릭터 이름, 2 = 대사)

            if(currentLine[0].Trim() != "" && currentLine[0].Trim() != "end")   // 다음 이벤트로 넘어간경우 eventID 업데이트 (공백, end 제외)
            {
                eventID = currentLine[0].Trim();
            }

            if(currentLine[0].Trim() == "end")                                  // 한 이벤트의 모든 대화가 종료 되었을 때
            {
                dialogDictionary.Add(eventID, dialogDatasList.ToArray());       // 그 시점까지의 구조체 리스트를 배열로 변환해 딕셔너리로 저장
                dialogDatasList = new List<DialogData>();                       // 구조체 리스트 초기화
                continue;
            }

            DialogData currentDialogData = new DialogData();                    // 현재 줄의 캐릭터이름, 대사 저장
            currentDialogData.characterName = currentLine[1];
            currentDialogData.context = currentLine[2];

            dialogDatasList.Add(currentDialogData);                             // 현재 구조체 리스트에 추가
        }
    }
}
