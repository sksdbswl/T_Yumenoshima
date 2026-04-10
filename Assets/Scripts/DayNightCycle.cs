using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0f, 1f)]
    public float time;

    [Header("Routine Time Mapping")]
    public float morningTime = 0.30f;
    public float noonTime = 0.50f;
    public float nightTime = 0.80f;
    public float transitionSpeed = 0.2f;

    public Vector3 noon;

    [Header("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplier;
    public AnimationCurve reflectionIntensityMultiplier;

    private const float DayStart = 0.25f;
    private const float NightStart = 0.75f;

    private bool isDayTime;
    private float targetTime;
    private Material skyboxMaterial;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (RenderSettings.skybox != null)
        {
            skyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;
        }

        targetTime = time;

        UpdateLightingImmediate();
        CheckAndUpdateDayNightCycle();

        GameManager.Singleton.OnRoutineChanged += HandleRoutineChanged;
        HandleRoutineChanged(GameManager.Singleton.CurrentRoutine);
    }

    private void OnDestroy()
    {
        if (GameManager.Singleton != null)
        {
            GameManager.Singleton.OnRoutineChanged -= HandleRoutineChanged;
        }
    }

    private void Update()
    {
        if (Mathf.Abs(time - targetTime) > 0.001f)
        {
            time = Mathf.MoveTowards(time, targetTime, transitionSpeed * Time.deltaTime);
            UpdateLightingImmediate();
            CheckAndUpdateDayNightCycle();
        }
    }

    private void HandleRoutineChanged(RoutineState state)
    {
        switch (state)
        {
            case RoutineState.Morning:
                targetTime = morningTime;
                break;

            case RoutineState.Noon:
                targetTime = noonTime;
                break;

            case RoutineState.Night:
                targetTime = nightTime;
                break;
        }

        Debug.Log($"DayNightCycle changed by RoutineState: {state}, targetTime: {targetTime}");
    }

    private void UpdateLightingImmediate()
    {
        UpdateLighting(sun, sunColor, sunIntensity, DayStart);
        UpdateLighting(moon, moonColor, moonIntensity, NightStart);
        UpdateEnvironmentLighting();
    }

    private void UpdateLighting(Light lightSource, Gradient colorGradient, AnimationCurve intensityCurve, float timeOffset)
    {
        float adjustedTime = (time - timeOffset) * 4.0f;
        float intensity = intensityCurve.Evaluate(time);

        lightSource.transform.eulerAngles = adjustedTime * noon;
        lightSource.color = colorGradient.Evaluate(time);
        lightSource.intensity = intensity;

        bool shouldBeActive = intensity > 0f;
        if (lightSource.gameObject.activeSelf != shouldBeActive)
        {
            lightSource.gameObject.SetActive(shouldBeActive);
        }
    }

    private void UpdateEnvironmentLighting()
    {
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionIntensityMultiplier.Evaluate(time);
    }

    private void CheckAndUpdateDayNightCycle()
    {
        bool newDaytimeStatus = time >= DayStart && time <= NightStart;
        if (newDaytimeStatus != isDayTime)
        {
            isDayTime = newDaytimeStatus;
            UpdateSunReference();
            UpdateSkyboxExposure();
        }
    }

    private void UpdateSunReference()
    {
        RenderSettings.sun = isDayTime ? sun : moon;
    }

    private void UpdateSkyboxExposure()
    {
        if (RenderSettings.skybox == null) return;

        if (RenderSettings.skybox.HasProperty("_Exposure"))
        {
            float targetExposure = isDayTime ? 1.3f : 0.3f;
            RenderSettings.skybox.SetFloat("_Exposure", targetExposure);
        }

        if (RenderSettings.skybox.HasProperty("_AtmosphereThickness"))
        {
            float targetThickness = isDayTime ? 1f : 0.1f;
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", targetThickness);
        }
    }
}