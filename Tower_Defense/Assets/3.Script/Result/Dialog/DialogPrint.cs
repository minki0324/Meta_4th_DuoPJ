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
                // 타이핑 효과
                dialogText.text = "";
                isTypingCompleted = false;
                nextDialog = false;

                foreach (char letter in currentDialogData.context)
                {
                    dialogText.text += letter;

                    if (isTypingCompleted) // 타이핑 중간에 입력을 받아서 isTypingCompleted가 true로 바뀌면
                    {
                        dialogText.text = currentDialogData.context; // 전체 문장 출력
                        break; // 타이핑 종료
                    }

                    yield return new WaitForSeconds(0.1f); // 0.1초마다 한 글자씩 출력
                }

                currentDialogIndex++;
                isTypingCompleted = true;

                while (!Input.anyKeyDown)
                {
                    bool isActive = arrow.activeSelf;
                    arrow.SetActive(!isActive);
                    // 어떤 키가 눌리면 코루틴 종료
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
            Debug.Log("이벤트 종료: " + currentEventID);
        }
        else
        {
            Debug.LogError("이벤트를 찾을 수 없음: " + currentEventID);
        }
        isEnter = false;
    }

    private void SetCurrentEvent(string _event)
    {
        currentEventID = _event;
    }

    private void Convert_Sound()
    {
        // 이전 SFX 정지
        StopCurrentSFX();

        currentSFXSource = null;  // currentSFXSource 초기화

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
