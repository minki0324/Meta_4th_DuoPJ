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
        string csvText = csvFile.text.Substring(0, csvFile.text.Length - 1);    // �� �� EndLine ����
        Debug.Log(csvText);
        string[] rows = csvText.Split('\n');                                    // �ٹٲ� ���ڸ� �������� csv������ �ɰ��� rows �迭�� ����
        List<DialogData> dialogDatasList = new List<DialogData>();              // ��ȭ�� ���̸� �𸣱� ������ List�� ���� �� �迭�� ��ȯ�Ͽ� ����

        string eventID = "";

        for(int i = 1; i < rows.Length; i++)                                    // rows�� ��� ��� ���� ��ȸ (0��° ���� ����)
        {
            // rows[i] = �̺�ƮID,ĳ�����̸�,��� (�پ�����)
            string[] currentLine = rows[i].Split(',');                          // �̺�ƮID, ĳ�����̸�, ��� ������ ���� (0 = ID, 1 = ĳ���� �̸�, 2 = ���)

            if(currentLine[0].Trim() != "" && currentLine[0].Trim() != "end")   // ���� �̺�Ʈ�� �Ѿ��� eventID ������Ʈ (����, end ����)
            {
                eventID = currentLine[0].Trim();
            }

            if(currentLine[0].Trim() == "end")                                  // �� �̺�Ʈ�� ��� ��ȭ�� ���� �Ǿ��� ��
            {
                dialogDictionary.Add(eventID, dialogDatasList.ToArray());       // �� ���������� ����ü ����Ʈ�� �迭�� ��ȯ�� ��ųʸ��� ����
                dialogDatasList = new List<DialogData>();                       // ����ü ����Ʈ �ʱ�ȭ
                continue;
            }

            DialogData currentDialogData = new DialogData();                    // ���� ���� ĳ�����̸�, ��� ����
            currentDialogData.characterName = currentLine[1];
            currentDialogData.context = currentLine[2];

            dialogDatasList.Add(currentDialogData);                             // ���� ����ü ����Ʈ�� �߰�
        }
    }
}
