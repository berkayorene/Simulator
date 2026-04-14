using UnityEngine;
using System.Collections;

public class RainGlassControl : MonoBehaviour
{
    [Header("Renderer ve Materyal")]
    public Renderer targetRenderer;          // camın renderer'ı
    public Material rainyMat;                // yağmurlu shader'lı materyal (tek materyal kullan)

    public enum ParamMode { SeparateFloat, ParamsVector4_W }
    [Header("Parametre Ayarı")]
    [Tooltip("Rain Amount shader'da ayrı bir float mı (_RainAmount) yoksa _Params(W) içinde mi?")]
    public ParamMode paramMode = ParamMode.SeparateFloat;

    [Tooltip("SeparateFloat modunda Rain Amount'un Reference adı")]
    public string rainAmountProp = "_RainAmount";

    [Tooltip("ParamsVector4 modunda kullanılan Vector4 parametrenin adı (örn: _Params)")]
    public string paramsVectorName = "_Params";

    [Header("Geçiş Süresi")]
    public float fadeDuration = 1.2f;

    [Header("Presetler (Az/Orta/Sağanak)")]
    [Range(0f, 1f)] public float lightRain = 0.30f;
    [Range(0f, 1f)] public float mediumRain = 0.60f;
    [Range(0f, 1f)] public float heavyRain = 1.00f;

    [Header("İsteğe Bağlı: Blur/Trail/Zoom'u da Amount'a bağla")]
    public bool driveBlurTrailZoom = true;
    [Tooltip("Max yağmurda blur (x) hedefi")]
    public float blurAtMax = 4.0f;        // _Params.x
    [Tooltip("Max yağmurda trail blur (y) hedefi")]
    public float trailAtMax = 1.0f;       // _Params.y
    [Tooltip("Max yağmurda zoom (z) hedefi")]
    public float zoomAtMax = 0.0f;        // _Params.z (istersen 0 bırak)

    float currentRain;                     // 0..1
    Coroutine fadeCo;

    void Awake()
    {
        if (targetRenderer != null && rainyMat != null)
        {
            // Sahne objesine instanced materyal ver
            targetRenderer.material = rainyMat;
        }
        SetRainImmediate(0f); // sahne başında kuru
    }

    // ----------------- BUTONLAR -----------------
    public void StartRain() => FadeRain(1f, fadeDuration);   // eski kullanımın kalsın
    public void StopRain() => FadeRain(0f, fadeDuration);

    public void SetLightRain() => FadeRain(lightRain, fadeDuration);
    public void SetMediumRain() => FadeRain(mediumRain, fadeDuration);
    public void SetHeavyRain() => FadeRain(heavyRain, fadeDuration);
    // --------------------------------------------

    public void FadeRain(float to, float duration)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRainRoutine(to, Mathf.Max(0.0001f, duration)));
    }

    IEnumerator FadeRainRoutine(float to, float duration)
    {
        if (targetRenderer == null) yield break;

        float from = currentRain;
        float t = 0f;

        Material mat = targetRenderer.material; // instanced

        while (t < duration)
        {
            t += Time.deltaTime;
            currentRain = Mathf.SmoothStep(from, to, Mathf.Clamp01(t / duration));
            ApplyToMaterial(mat, currentRain);
            yield return null;
        }

        currentRain = to;
        ApplyToMaterial(mat, currentRain); // son değeri garanti et
    }

    public void SetRainImmediate(float value)
    {
        currentRain = Mathf.Clamp01(value);
        if (targetRenderer == null) return;
        var mat = targetRenderer.material;
        ApplyToMaterial(mat, currentRain);
    }

    // --- Tek yerden yaz: hem ayrı float hem _Params(W) destekli ---
    void ApplyToMaterial(Material mat, float amount01)
    {
        amount01 = Mathf.Clamp01(amount01);

        if (paramMode == ParamMode.SeparateFloat)
        {
            // Yağmur yoğunluğu tek bir float ise
            mat.SetFloat(rainAmountProp, amount01);
        }
        else // ParamsVector4_W
        {
            // Yağmur yoğunluğu _Params.w içindeyse
            Vector4 p = mat.GetVector(paramsVectorName);
            p.w = amount01;

            if (driveBlurTrailZoom)
            {
                // amount'a göre x/y/z'yi ölçekle
                p.x = blurAtMax * amount01; // blur
                p.y = trailAtMax * amount01; // trailBlur
                p.z = zoomAtMax * amount01; // zoom
            }

            mat.SetVector(paramsVectorName, p);
        }

        // SeparateFloat modunda da blur/trail/zoom sürmek istersen:
        if (paramMode == ParamMode.SeparateFloat && driveBlurTrailZoom)
        {
            // Ayrı param adları kullanıyorsan buraya ekle:
            // mat.SetFloat("_Blur",  blurAtMax  * amount01);
            // mat.SetFloat("_Trail", trailAtMax * amount01);
            // mat.SetFloat("_Zoom",  zoomAtMax  * amount01);
        }
    }
}
