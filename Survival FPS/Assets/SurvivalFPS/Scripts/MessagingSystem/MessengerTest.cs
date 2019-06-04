using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SurvivalFPS.Messaging;

public class TestEventData : MessengerEventData
{
    public string stringData;
    public int integerData;
    public KeyValuePair<int, int> pairData;
}

public class MessengerTest : MonoBehaviour
{
    private void Start()
    {
        //meaningless code, just make the messenger initialized
        //Messenger.Cleanup();
    }
}

public class MessengerTestListener : MonoBehaviour
{
    private static bool added = false;
    private int callCnt1=0, callCnt2=0, callCnt3=0;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            if (!added)
            {
                Messenger.AddPersistentListener(M_EventType.TestEvent, PersistentHandler);
                Messenger.AddPersistentListener<TestEventData>(M_DataEventType.TestDataEvent, PersistentHandlerWithData);
                added = true;
            }

            Messenger.AddListener(M_EventType.TestEvent, Handler1);
            Messenger.AddListener(M_EventType.TestEvent, Handler1);

            Messenger.AddListener(M_EventType.TestEvent, Handler2);

            Messenger.AddListener(M_EventType.TestEvent, InvalidHandler);
            Messenger.RemoveListener(M_EventType.TestEvent, InvalidHandler);

            Messenger.AddListener<TestEventData>(M_DataEventType.TestDataEvent, HandlerWithData);
        }
        catch(Messenger.ListenerException exception)
        {
            Debug.Log(exception.Message);
        }
    }

    // Update is called once per frame
    void InvalidHandler()
    {
        Debug.Log("This should not be printed");
    }

    void Handler1()
    {
        callCnt1++;
        Debug.Log("Handler1 is called!");

        if (callCnt1 >= 2)
            Messenger.RemoveListener(M_EventType.TestEvent, Handler1);
    }

    void Handler2()
    {
        callCnt2++;
        Debug.Log("Handler2 is called!");

        if (callCnt2 >= 2)
            Messenger.RemoveListener(M_EventType.TestEvent, Handler2);
    }

    void PersistentHandler()
    {
        callCnt3++;
        Debug.Log("PersistentHandler called!");

        if (callCnt3 >= 2)
            Messenger.RemovePersistentListener(M_EventType.TestEvent, PersistentHandler);
    }

    void HandlerWithData(TestEventData data)
    {
        Debug.Log("HandlerWithData is called!");
        Debug.Log(string.Format("we have received string: {0}, integer: {1}, pair: {2}", data.stringData, data.integerData.ToString(), data.pairData.ToString()));

        Messenger.RemoveListener<TestEventData>(M_DataEventType.TestDataEvent, HandlerWithData);
    }

    void PersistentHandlerWithData(TestEventData data)
    {
        Debug.Log("PersistentHandlerWithData called!");
        Debug.Log(string.Format("we have received string: {0}, integer: {1}, pair: {2}", data.stringData, data.integerData.ToString(), data.pairData.ToString()));

        Messenger.RemovePersistentListener<TestEventData>(M_DataEventType.TestDataEvent, PersistentHandlerWithData);
    }
}

public class MessengerTestProducer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(countDown());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator countDown()
    {
        yield return new WaitForSeconds(4.0f);
        Debug.Log("invoking test event 1, 3 handlers should be called, among which 1 is persistent");
        Messenger.Broadcast(M_EventType.TestEvent);

        yield return new WaitForSeconds(4.0f);
        Debug.Log("invoking test event 2, 2 handlers should be called, among which 1 is persistent");

        TestEventData data = new TestEventData();
        data.stringData = "haha!";
        data.integerData = 100;
        data.pairData = new KeyValuePair<int, int>(10310, 10312);
        Messenger.Broadcast(M_DataEventType.TestDataEvent, data);

        yield return new WaitForSeconds(4.0f);
        Debug.Log("invoking test event 2 again, nothing should happen now");
        Messenger.Broadcast(M_DataEventType.TestDataEvent, data);

        Debug.Log("invoking test event 1 again, 3 handlers should be called, among which 1 is persistent");
        Messenger.Broadcast(M_EventType.TestEvent);

        Debug.Log("invoking test event 1 for the 3rd time, nothing should happen now");
        Messenger.Broadcast(M_EventType.TestEvent);
    }
}