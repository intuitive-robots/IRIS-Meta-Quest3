using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MQ3SceneManager;

public class OffsetConfigMenuManager : MonoBehaviour
{
    [SerializeField] Slider offsetX;
    [SerializeField] Slider offsetY;
    [SerializeField] Slider offsetZ;
    [SerializeField] Slider rotX;
    [SerializeField] Slider rotY;
    [SerializeField] Slider rotZ;

    [SerializeField] private TMP_Text offsetXText;
    [SerializeField] private TMP_Text offsetYText;
    [SerializeField] private TMP_Text offsetZText;
    [SerializeField] private TMP_Text rotXText;
    [SerializeField] private TMP_Text rotYText;
    [SerializeField] private TMP_Text rotZText;
    [SerializeField] private TMP_Text SceneNameText;

    private RawOffset offset;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // No subscription here because MQ3SceneManager does not expose a 'NewSceneConfig' member.
        // If MQ3SceneManager provides an event for scene config changes, subscribe here using the correct event name, e.g.:
        MQ3SceneManager.Instance.NewSceneConfig += OnNewSceneConfig;
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize(string name, string qrCode, RawOffset offset)
    {
        // init Text
        offsetXText.text = offset.x.ToString();
        offsetYText.text = offset.y.ToString();
        offsetZText.text = offset.z.ToString();
        rotXText.text = offset.rotX.ToString();
        rotYText.text = offset.rotY.ToString();
        rotZText.text = offset.rotZ.ToString();

        // init Slider
        offsetX.value = offset.x;
        offsetY.value = offset.y;
        offsetZ.value = offset.z;
        rotX.value = offset.rotX;
        rotY.value = offset.rotY;
        rotZ.value = offset.rotZ;


        SceneNameText.text = name;
        this.offset = offset;
    }

    // event which will be triggered by Slider
    public void OnXOffsetChanged(float value)
    {
        offset.x = value;
        MQ3SceneManager.Instance.UpdateRawOffset(SceneNameText.text, offset);
    }

    public void OnYOffsetChanged(float value)
    {
        offset.y = value;
        MQ3SceneManager.Instance.UpdateRawOffset(SceneNameText.text, offset);
    }

    public void OnZOffsetChanged(float value)
    {
        offset.z = value;
        MQ3SceneManager.Instance.UpdateRawOffset(SceneNameText.text, offset);
    }

    public void OnRotXChanged(float value)
    {
        offset.rotX = value;
        MQ3SceneManager.Instance.UpdateRawOffset(SceneNameText.text, offset);
    }

    public void OnRotYChanged(float value)
    {
        offset.rotY = value;
        MQ3SceneManager.Instance.UpdateRawOffset(SceneNameText.text, offset);
    }

    public void OnRotZChanged(float value)
    {
        offset.rotZ = value;
        MQ3SceneManager.Instance.UpdateRawOffset(SceneNameText.text, offset);
    }



    private void OnNewSceneConfig(Dictionary<string, SceneData> dictionary)
    {
        SceneData sceneData = dictionary[SceneNameText.text];
        if (sceneData == null) return;
        Initialize(SceneNameText.text, sceneData.QrCode, sceneData.ToRawJsonItem().offset);
    }

    public string GetSceneName()
    {
        return SceneNameText.text;
    }
}
