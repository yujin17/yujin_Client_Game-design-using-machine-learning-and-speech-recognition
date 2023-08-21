using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using StackExchange.Redis;
using System.Linq;
using System.Collections.Generic;
using System;

public class CommentManager : MonoBehaviour
{

    private string m_channel = "test";
    List<string> MsgList;
    TMP_Text textUI;
    [SerializeField] GameObject commentPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform scrollParent;
    [SerializeField] GameObject canv;
    [SerializeField] InputField chatInput;
    [SerializeField] Scrollbar scrollbarVertical;
    GameObject previousComment;


    #region MonoBehaviour

    private void Awake()
    {

    }

    private void Update()
    {
        try
        {
            // ä�� �޽��� �׻� ������
            Redis.Instance.Subscribe("test", SubAction);
            // ������ �޽����� ������
            if (MsgList.Count > 0)
            { 
                string[] messages = MsgList[0].Split(",", 2);
                MsgList.RemoveAt(0);
                // �޽����� id, comment�� �и��ؼ� �߰�
                AddComment(messages[0], messages[1]);
                StartCoroutine("Waitmillisec");
            }
        }
        catch (Exception e) { }
        

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // ĵ���� ���� ���� ��
            if (!canv.active)
            {
                canv.active = true;
                chatInput.Select();
            }
            // ĵ���� ���� ���� ��
            else if (canv.active)
            {
                chatInput.Select();
                if (chatInput.text == "") canv.SetActive(!canv.active);
                else
                {
                    Redis.Instance.Publish(m_channel, Redis.Instance.entireMessage);
                    AddComment("����", chatInput.text);
                    chatInput.text = "";
                    StartCoroutine("Waitmillisec");
                }
            }
        }
    }

    #endregion


    #region CommentAction
    public void AddComment(string id, string comment)
    {
        // ������ ����� �޽����� �ִٸ� �����ϰ� �ٽ� ����
        if(Redis.Instance.entireMessage != "") Destroy(previousComment);
        GameObject prefab = Instantiate(commentPrefab);
        prefab.transform.SetParent(scrollParent.transform);
        prefab.transform.SetAsLastSibling();
        textUI = prefab.GetComponent<TMP_Text>();
        textUI.horizontalAlignment = HorizontalAlignmentOptions.Left;
        scrollbarVertical.value = 0;

        // ���� ����� �޽����� id�� comment�� �߰�
        Redis.Instance.entireMessage += id + " : " + comment + '\n';
        // �߰��� �޽����� ������
        textUI.text = GetEntireMessage();
        scrollRect.verticalNormalizedPosition = 0;
        previousComment = prefab;

        Debug.Log(GetEntireMessage());
    }

    IEnumerator Waitmillisec()
    {
        chatInput.Select();
        yield return new WaitForSeconds(0.001f);
    }

    public void SubAction(RedisChannel ch, RedisValue va)
    {
        MsgList.Add(va);
    }

    string GetEntireMessage()
    {
        return Redis.Instance.entireMessage;
    }

    #endregion
}