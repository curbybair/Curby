//DO NOT MODIFY THIS RUNS ALL THE PUBLISHED AND HTTP DATA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour     //Fetching the MJPEG stream is done asynchronously to avoid blocking the main thread. This operation runs on a background thread to keep the UI responsive.

{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(IEnumerator action) //Enqueues a coroutine to be executed on the main thread.
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() => { StartCoroutine(action); });
        }
    }

    public void Enqueue(Action action)  //Enqueues an action to be executed on the main thread.

    {
        Enqueue(ActionWrapper(action));
    }

    IEnumerator ActionWrapper(Action action)
    {
        action();
        yield return null;
    }

    private static UnityMainThreadDispatcher _instance = null;

    public static bool Exists()
    {
        return _instance != null;
    }

    public static UnityMainThreadDispatcher Instance()
    {
        if (!Exists())
        {
            throw new Exception("UnityMainThreadDispatcher not found. Please ensure you have added the UnityMainThreadDispatcher to your scene.");
        }
        return _instance;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
