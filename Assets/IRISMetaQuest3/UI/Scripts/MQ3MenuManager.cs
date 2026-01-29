using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;
using IRIS.Node;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class MQ3MenuManager : Singleton<MQ3MenuManager>
{
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private TMP_InputField appNameInput;
    [SerializeField] OffsetConfigMenuManager offsetConfigMenuManagerPrefab;
    [SerializeField] GameObject OffsetConfigMenuManagerParent;
    public UnityEvent onQRTrackingStarted;
    public UnityEvent onQRTrackingStopped;
    public UnityEvent onAlignmentStarted;
    public UnityEvent onAlignmentStopped;
    public UnityEvent<string> onChangeName;

    private Dictionary<string, OffsetConfigMenuManager> offsetConfigMenuManagers = new Dictionary<string, OffsetConfigMenuManager>();
    private ConcurrentQueue<Dictionary<string, MQ3SceneManager.SceneData>> _pendingConfigs = new ConcurrentQueue<Dictionary<string, MQ3SceneManager.SceneData>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onQRTrackingStarted.AddListener(() => debugText.text = "QR Tracking Started");
        onQRTrackingStopped.AddListener(() => debugText.text = "QR Tracking Stopped");
        onAlignmentStarted.AddListener(() => debugText.text = "Alignment Started");
        onAlignmentStopped.AddListener(() => debugText.text = "Alignment Stopped");
        UpdateDisplayName();

        MQ3SceneManager.Instance.NewSceneConfig += (dictionary) => _pendingConfigs.Enqueue(dictionary);
    }


    // Update is called once per frame
    void Update()
    {
        while (_pendingConfigs.TryDequeue(out var dictionary))
        {
            OnNewSceneConfig(dictionary);
        }
    }

    public void QRTrackingToggled(bool isTracking)
    {
        if (isTracking)
        {
            onQRTrackingStarted?.Invoke();
        }
        else
        {
            onQRTrackingStopped?.Invoke();
        }
    }
    private void OnNewSceneConfig(Dictionary<string, MQ3SceneManager.SceneData> dictionary)
    {
        foreach (var kv in dictionary)
        {
            string sceneName = kv.Key;
            if (!offsetConfigMenuManagers.ContainsKey(sceneName))
            {
                var instance = Instantiate(offsetConfigMenuManagerPrefab, OffsetConfigMenuManagerParent.transform);
                instance.GetComponent<OffsetConfigMenuManager>().Initialize(sceneName, kv.Value.QrCode, kv.Value.ToRawJsonItem().offset);
                offsetConfigMenuManagers[sceneName] = instance.GetComponent<OffsetConfigMenuManager>();
            }
        }
    }

    public void AlignmentToggled(bool isAligning)
    {
        if (isAligning)
        {
            onAlignmentStarted?.Invoke();
        }
        else
        {
            onAlignmentStopped?.Invoke();
        }
    }

    private void UpdateDisplayName()
    {
        debugText.text = "UpdateDisplayName called. ";
        name = IRISXRNode.Instance.localInfo.nodeInfo.Name;
        if (appNameInput != null)
        {
            debugText.text = "App Name: " + name;
            appNameInput.text = name;
        }
        else
        {
            debugText.text = "App Name Input is null, " + "App Name: " + name;
        }
    }


}
