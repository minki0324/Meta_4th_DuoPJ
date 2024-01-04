using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPrint : MonoBehaviour
{
    public static DialogPrint instance;

    public Text characterNameText;
    public Text dialogText;

    [SerializeField] private GameObject arrow;
    private string currentEventID;
    public int currentDialogIndex;

    private Coroutine typingCoroutine;
    private AudioSource currentSFXSource;

    public bool isEnter = false;
    public bool isTypingCompleted = false;
    public bool nextDialog = false;
    public bool isAudioPlay = false;

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
    }

    private void Update()
    {
        if(Input.anyKeyDown && isTypingCompleted)
        {
            nextDialog = true;
        }
        else if(Input.anyKeyDown)
        {
            isTypingCompleted = true;
        }
    }

    private void StopCurrentSFX()
    {
        AudioManager.instance.StopSFX(currentSFXSource);
    }

    public void PrintEvent(string eventID)
    {
        SetCurrentEvent(eventID);
        StartTyping();
    }

    private void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        isEnter = true;
        currentDialogIndex = 0;
        typingCoroutine = StartCoroutine(TypeDialog());
    }

    private IEnumerator TypeDialog()
    {
        if (DialogManager.instance.dialogDictionary.ContainsKey(currentEventID))
        {
            DialogData[] dialogDatas = DialogManager.instance.dialogDictionary[currentEventID];

            while (currentDialogIndex < dialogDatas.Length)
            {
                DialogData currentDialogData = dialogDatas[currentDialogIndex];

                characterNameText.text = currentDialogData.characterName;

                Convert_Sound();
                // Ÿ���� ȿ��
                dialogText.text = "";
                isTypingCompleted = false;
                nextDialog = false;

                foreach (char letter in currentDialogData.context)
                {
                    dialogText.text += letter;

                    if (isTypingCompleted) // Ÿ���� �߰��� �Է��� �޾Ƽ� isTypingCompleted�� true�� �ٲ��
                    {
                        dialogText.text = currentDialogData.context; // ��ü ���� ���
                        break; // Ÿ���� ����
                    }

                    yield return new WaitForSeconds(0.1f); // 0.1�ʸ��� �� ���ھ� ���
                }

                currentDialogIndex++;
                isTypingCompleted = true;

                while (!Input.anyKeyDown)
                {
                    bool isActive = arrow.activeSelf;
                    arrow.SetActive(!isActive);
                    // � Ű�� ������ �ڷ�ƾ ����
                    if (nextDialog)
                    {
                        if(isAudioPlay)
                        {
                            StopCurrentSFX();
                        }
                        Debug.Log("??");
                        arrow.SetActive(false);
                        break;
                    }
                    yield return new WaitForSeconds (0.3f);
                }
            }
            Debug.Log("�̺�Ʈ ����: " + currentEventID);
        }
        else
        {
            Debug.LogError("�̺�Ʈ�� ã�� �� ����: " + currentEventID);
        }
        isEnter = false;
    }

    private void SetCurrentEvent(string _event)
    {
        currentEventID = _event;
    }

    private void Convert_Sound()
    {
        // ���� SFX ����
        StopCurrentSFX();

        currentSFXSource = null;  // currentSFXSource �ʱ�ȭ

        switch (currentEventID)
        {
            case "1":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("01_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("01_02");
                }
                break;
            case "2":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("02_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("02_02");
                }
                break;
            case "3":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("03_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("03_02");
                }
                break;
            case "4":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("04_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("04_02");
                }
                break;
            case "5":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("05_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("05_02");
                }
                break;
            case "6":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("06_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("06_02");
                }
                else if (currentDialogIndex == 2)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("06_03");
                }
                break;
            case "7":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("07_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("07_02");
                }
                break;
            case "8":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("08_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("08_02");
                }
                break;
            case "9":
                if (currentDialogIndex == 0)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("09_01");
                }
                else if (currentDialogIndex == 1)
                {
                    currentSFXSource = AudioManager.instance.PlaySFX("09_02");
                }
                break;
        }
    }
}
