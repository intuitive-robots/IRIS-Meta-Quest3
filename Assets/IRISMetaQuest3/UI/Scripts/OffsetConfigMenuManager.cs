using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MQ3SceneManager;

public class OffsetConfigMenuManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] Slider offsetX, offsetY, offsetZ;
    [SerializeField] Slider rotX, rotY, rotZ;

    [SerializeField] private TMP_Text offsetXText, offsetYText, offsetZText;
    [SerializeField] private TMP_Text rotXText, rotYText, rotZText;
    // [SerializeField] private TMP_Text SceneNameText;

    [Header("Settings")]
    [SerializeField] private float posStepSize = 0.001f; // Defined in Meters (e.g. 0.001 = 1mm)
    [SerializeField] private float rotStepSize = 1f;    // Defined in Degrees

    private RawOffset offset;
    private bool listenersRegistered = false;

    void Start()
    {
        MQ3SceneManager.Instance.NewSceneConfig += OnNewSceneConfig;
    }

    private void OnDestroy()
    {
        if (MQ3SceneManager.Instance != null)
            MQ3SceneManager.Instance.NewSceneConfig -= OnNewSceneConfig;

        RemoveListeners();
    }

    public void Initialize(RawOffset offset)
    {
        Debug.Log($"[OffsetConfigMenuManager] Init: {name}");

        // Prevent infinite loops if re-initializing with same object
        if (this.offset != null && offset == this.offset) return;

        this.offset = offset;
        // SceneNameText.text = name;

        // Temporarily remove listeners so setting values doesn't trigger network calls during Init
        RemoveListeners();

        // Initialize Sliders (Position: Meters -> mm, Rotation: Degrees -> Degrees)
        offsetX.value = offset.x * 1000f;
        offsetY.value = offset.y * 1000f;
        offsetZ.value = offset.z * 1000f;
        rotX.value = offset.rotX;
        rotY.value = offset.rotY;
        rotZ.value = offset.rotZ;

        // Initialize Text
        UpdatePositionText(offsetX.value, offsetXText);
        UpdatePositionText(offsetY.value, offsetYText);
        UpdatePositionText(offsetZ.value, offsetZText);
        UpdateRotationText(rotX.value, rotXText);
        UpdateRotationText(rotY.value, rotYText);
        UpdateRotationText(rotZ.value, rotZText);

        AddListeners();
    }

    // ---------------------------------------------------------
    // 1. GENERIC HANDLERS (The core logic)
    // ---------------------------------------------------------

    private void HandlePositionChange(float sliderValueMM, Action<float> setOffsetAction, TMP_Text textComponent)
    {
        // Convert Slider (mm) to Data (meters)
        setOffsetAction(sliderValueMM / 1000f);
        
        // Update UI Text
        UpdatePositionText(sliderValueMM, textComponent);

        // Send Network Update
        MQ3SceneManager.Instance.UpdateRawOffset(offset);
    }

    private void HandleRotationChange(float sliderValueDeg, Action<float> setOffsetAction, TMP_Text textComponent)
    {
        // Direct assignment (Degrees to Degrees)
        setOffsetAction(sliderValueDeg);

        // Update UI Text
        UpdateRotationText(sliderValueDeg, textComponent);

        // Send Network Update
        MQ3SceneManager.Instance.UpdateRawOffset(offset);
    }

    // Helper to format text consistently
    private void UpdatePositionText(float mm, TMP_Text text) => text.text = (mm / 10f).ToString("F1") + " cm";
    private void UpdateRotationText(float deg, TMP_Text text) => text.text = deg.ToString("F0") + "°";


    // ---------------------------------------------------------
    // 2. EVENT LISTENERS (Linked to Sliders)
    // ---------------------------------------------------------

    // We use Lambdas to inject the specific field logic
    private void AddListeners()
    {
        if (listenersRegistered) return;

        offsetX.onValueChanged.AddListener(val => HandlePositionChange(val, x => offset.x = x, offsetXText));
        offsetY.onValueChanged.AddListener(val => HandlePositionChange(val, y => offset.y = y, offsetYText));
        offsetZ.onValueChanged.AddListener(val => HandlePositionChange(val, z => offset.z = z, offsetZText));

        rotX.onValueChanged.AddListener(val => HandleRotationChange(val, x => offset.rotX = x, rotXText));
        rotY.onValueChanged.AddListener(val => HandleRotationChange(val, y => offset.rotY = y, rotYText));
        rotZ.onValueChanged.AddListener(val => HandleRotationChange(val, z => offset.rotZ = z, rotZText));

        listenersRegistered = true;
    }

    private void RemoveListeners()
    {
        if (!listenersRegistered) return;
        offsetX.onValueChanged.RemoveAllListeners();
        offsetY.onValueChanged.RemoveAllListeners();
        offsetZ.onValueChanged.RemoveAllListeners();
        rotX.onValueChanged.RemoveAllListeners();
        rotY.onValueChanged.RemoveAllListeners();
        rotZ.onValueChanged.RemoveAllListeners();
        listenersRegistered = false;
    }

    // ---------------------------------------------------------
    // 3. STEP FUNCTIONS (Triggered by Buttons)
    // ---------------------------------------------------------
    
    // We only update the slider. The slider listener (defined above) 
    // handles the text updates, data updates, and network calls automatically.
    
    public void StepOffsetX(int step) => offsetX.value += step * (posStepSize * 1000f);
    public void StepOffsetY(int step) => offsetY.value += step * (posStepSize * 1000f);
    public void StepOffsetZ(int step) => offsetZ.value += step * (posStepSize * 1000f);

    public void StepRotX(int step) => rotX.value += step * rotStepSize;
    public void StepRotY(int step) => rotY.value += step * rotStepSize;
    public void StepRotZ(int step) => rotZ.value += step * rotStepSize;

    // ---------------------------------------------------------
    // 4. MISC
    // ---------------------------------------------------------

    private void OnNewSceneConfig(SceneData sceneData)
    {
        Initialize(sceneData.ToRawJsonItem().offset);
    }

    // public string GetSceneName() => SceneNameText.text;
}