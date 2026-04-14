using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using TMPro;

public class LinearAnimation : MonoBehaviour
{
    public enum AnimatedProperty { transform, rectTransform, scale, alpha, rotation, textColor };


    public AnimationCurve curve;
    public float animationScale = 1f;
    [SerializeField] AnimatedProperty animatedProperty = AnimatedProperty.transform;
    public AnimatedProperty AnimationType => animatedProperty;
    public void SetAnimatedProperty(AnimatedProperty targetAnimatedProperty)
    {
        animatedProperty = targetAnimatedProperty;
    }

    public bool useUnscaledTime = true;
    public bool playOnEnable = false;
    public bool playOnAwake = false;
    public bool loop = false;

    [SerializeField] Image animatedColor;
    [SerializeField] RawImage animatedRawImage;
    [SerializeField] SpriteRenderer animatedSprite;
    [SerializeField] SpriteShapeRenderer animatedSpriteShapeRenderer;
    [SerializeField] TextMeshProUGUI animatedText;
    public bool moveToEndPosition = false;
    public bool overwriteStartPosition = false;
    public Vector3 startPosition;
    public Vector3 endPosition;
    public bool overwriteStartScale = false;
    public float endScale;
    public float startScale;
    public bool overwriteStartAlpha = false;
    public float startAlpha = 0f;
    public float endAlpha;
    public bool overwriteStartRotation = false;
    public float startZRotation;
    public float endZRotation;
    Vector3 startPos;
    Vector3 endPos;

    Vector3 startScaleValue;
    Vector3 finalScale;

    float startRotation;
    float _startAlpha;

    [SerializeField] UnityEvent OnAnimationStarted;
    [SerializeField] UnityEvent OnAnimationCompleted;
    private void OnEnable()
    {
        if (animatedColor == null)
            animatedColor = GetComponent<Image>();
        if (animatedRawImage == null)
            animatedRawImage = GetComponent<RawImage>();

        if (overwriteStartScale)
        {
            startScaleValue = Vector3.one * startScale;
        }
        else
        {
            startScaleValue = transform.localScale;
        }

        if (overwriteStartRotation)
        {
            startRotation = startZRotation;
        }
        else
        {
            startRotation = transform.localRotation.z;
        }

        if (playOnEnable)
        {
            StartMovement(moveByTransform: !moveToEndPosition);
        }
    }
    void Start()
    {
        if (animatedColor == null)
            animatedColor = GetComponent<Image>();
        if (animatedRawImage == null)
            animatedRawImage = GetComponent<RawImage>();
        if (playOnAwake)
        {
            StartMovement(moveByTransform: !moveToEndPosition);
        }
    }

    IEnumerator Move(Transform endPoint, bool dynamic = false)
    {
        startPos = transform.position;
        if (endPoint == null)
            endPos = transform.parent.position;
        else
            endPos = endPoint.position;

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                if (dynamic)
                    transform.position = Vector3.LerpUnclamped(startPos, endPoint.position, curve.Evaluate(t));
                else
                    transform.position = Vector3.LerpUnclamped(startPos, endPos, curve.Evaluate(t));
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                if (dynamic)
                    transform.position = Vector3.LerpUnclamped(startPos, endPoint.position, curve.Evaluate(t));
                else
                    transform.position = Vector3.LerpUnclamped(startPos, endPos, curve.Evaluate(t));
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Move(endPoint, dynamic));
    }
    IEnumerator Move(Vector3 endPoint)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if (overwriteStartPosition)
            startPos = startPosition;
        else
        {
            if (animatedProperty == AnimatedProperty.rectTransform)
                startPos = rectTransform.localPosition;
            else
                startPos = transform.localPosition;
        }
        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                if (animatedProperty == AnimatedProperty.rectTransform)
                    rectTransform.localPosition = Vector3.LerpUnclamped(startPos, endPoint, curve.Evaluate(t));
                else
                    transform.localPosition = Vector3.LerpUnclamped(startPos, endPoint, curve.Evaluate(t));
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                if (animatedProperty == AnimatedProperty.rectTransform)
                    rectTransform.localPosition = Vector3.LerpUnclamped(startPos, endPoint, curve.Evaluate(t));
                else
                    transform.localPosition = Vector3.LerpUnclamped(startPos, endPoint, curve.Evaluate(t));
                yield return new WaitForFixedUpdate();
            }
        }

        OnAnimationCompleted?.Invoke();
        if (loop)
            StartCoroutine(Move(endPoint));
    }
    IEnumerator Scale()
    {
        finalScale = Vector3.one * endScale;

        if (overwriteStartScale)
        {
            startScaleValue = Vector3.one * startScale;
        }
        else
        {
            startScaleValue = transform.localScale;
        }

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                transform.localScale = Vector3.LerpUnclamped(startScaleValue, finalScale, curve.Evaluate(t));
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                transform.localScale = Vector3.LerpUnclamped(startScaleValue, finalScale, curve.Evaluate(t));
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Scale());
    }
    IEnumerator Fade()
    {
        if (animatedColor != null)
        {
            if (overwriteStartAlpha)
                _startAlpha = startAlpha;
            else
                _startAlpha = animatedColor.color.a;
            Color color = new Color(animatedColor.color.r, animatedColor.color.g, animatedColor.color.b, animatedColor.color.a);

            if (useUnscaledTime)
            {
                for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
                {
                    color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                    animatedColor.color = color;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
                {
                    color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                    animatedColor.color = color;
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Fade());
    }
    IEnumerator Fade(RawImage rawImage)
    {
        if (overwriteStartAlpha)
            _startAlpha = startAlpha;
        else
            _startAlpha = rawImage.color.a;
        Color color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, rawImage.color.a);

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                rawImage.color = color;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                rawImage.color = color;
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Fade(rawImage));
    }
    IEnumerator Fade(SpriteRenderer sprite)
    {
        if (overwriteStartAlpha)
            _startAlpha = startAlpha;
        else
            _startAlpha = sprite.color.a;
        Color color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a);

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                sprite.color = color;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                sprite.color = color;
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Fade(sprite));
    }
    IEnumerator Fade(SpriteShapeRenderer sprite)
    {
        if (overwriteStartAlpha)
            _startAlpha = startAlpha;
        else
            _startAlpha = sprite.color.a;
        Color color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a);

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                sprite.color = color;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                sprite.color = color;
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Fade(sprite));
    }
    IEnumerator Fade(TextMeshProUGUI text)
    {
        if (overwriteStartAlpha)
            _startAlpha = startAlpha;
        else
            _startAlpha = text.color.a;
        Color color = new Color(text.color.r, text.color.g, text.color.b, _startAlpha);

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                text.color = color;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                color.a = Mathf.LerpUnclamped(_startAlpha, endAlpha, curve.Evaluate(t));
                text.color = color;
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Fade(text));
    }
    IEnumerator Rotate(float endRotation)
    {
        if (overwriteStartRotation)
            startRotation = startZRotation;
        else
            startRotation = transform.localRotation.eulerAngles.z;

        if (useUnscaledTime)
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.unscaledDeltaTime * animationScale)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRotation, endRotation, curve.Evaluate(t)));
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            for (float t = curve.keys[0].time; t < curve.keys[curve.length - 1].time; t += Time.fixedDeltaTime * animationScale)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRotation, endRotation, curve.Evaluate(t)));
                yield return new WaitForFixedUpdate();
            }
        }
        OnAnimationCompleted?.Invoke();

        if (loop)
            StartCoroutine(Rotate(endRotation));
    }
    public Coroutine StartMovement(Transform endPoint = null, bool moveByTransform = false, bool dynamic = false)
    {
        OnAnimationStarted?.Invoke();
        if (animatedProperty == AnimatedProperty.transform)
        {
            if (moveByTransform)
                return StartCoroutine(Move(endPoint, dynamic));
            else
                return StartCoroutine(Move(endPosition));
        }
        else if (animatedProperty == AnimatedProperty.rectTransform)
        {
            return StartCoroutine(Move(endPosition));
        }
        else if (animatedProperty == AnimatedProperty.scale)
            return StartCoroutine(Scale());
        else if (animatedProperty == AnimatedProperty.alpha)
        {
            if (animatedSprite != null)
                return StartCoroutine(Fade(animatedSprite));
            else if (animatedColor != null)
                return StartCoroutine(Fade());
            else if (animatedRawImage != null)
                return StartCoroutine(Fade(animatedRawImage));
            else
                return StartCoroutine(Fade(animatedSpriteShapeRenderer));
        }
        else if (animatedProperty == AnimatedProperty.rotation)
        {
            StartCoroutine(Rotate(endZRotation));
        }
        else if (animatedProperty == AnimatedProperty.textColor)
        {
            StartCoroutine(Fade(animatedText));
        }

        return null;
    }
    public void ManualStartMovement()
    {
        StopAllCoroutines();
        OnAnimationStarted?.Invoke();
        if (animatedProperty == AnimatedProperty.transform)
        {
            StartCoroutine(Move(endPosition));
        }
        else if (animatedProperty == AnimatedProperty.rectTransform)
        {
            StartCoroutine(Move(endPosition));
        }
        else if (animatedProperty == AnimatedProperty.scale)
            StartCoroutine(Scale());
        else if (animatedProperty == AnimatedProperty.alpha)
        {
            if (animatedSprite != null)
                StartCoroutine(Fade(animatedSprite));
            else if (animatedColor != null)
                StartCoroutine(Fade());
            else if (animatedRawImage != null)
                StartCoroutine(Fade(animatedRawImage));
            else
                StartCoroutine(Fade(animatedSpriteShapeRenderer));
        }
        else if (animatedProperty == AnimatedProperty.rotation)
        {
            StartCoroutine(Rotate(endZRotation));
        }
        else if (animatedProperty == AnimatedProperty.textColor)
        {
            StartCoroutine(Fade(animatedText));
        }
    }
    public float GetCompletionTime()
    {
        return (curve[curve.length - 1].time - curve[0].time) / animationScale;
    }

    public void ResetState()
    {
        switch (animatedProperty)
        {
            case AnimatedProperty.transform:
                transform.localPosition = startPos;
                break;
            case AnimatedProperty.rectTransform:
                GetComponent<RectTransform>().localPosition = startPos;
                break;
            case AnimatedProperty.alpha:
                if (animatedColor != null)
                    animatedColor.color = new Color(animatedColor.color.r, animatedColor.color.g, animatedColor.color.b, _startAlpha);
                else if (animatedSprite != null)
                    animatedSprite.color = new Color(animatedSprite.color.r, animatedSprite.color.g, animatedSprite.color.b, _startAlpha);
                else if (animatedSpriteShapeRenderer != null)
                    animatedSpriteShapeRenderer.color = new Color(animatedSpriteShapeRenderer.color.r, animatedSpriteShapeRenderer.color.g, animatedSpriteShapeRenderer.color.b, _startAlpha);
                else if (animatedRawImage != null)
                    animatedRawImage.color = new Color(animatedRawImage.color.r, animatedRawImage.color.g, animatedRawImage.color.b, _startAlpha);
                else
                    animatedText.color = new Color(animatedText.color.r, animatedText.color.g, animatedText.color.b, animatedText.color.a);
                break;
            case AnimatedProperty.scale:
                transform.localScale = startScaleValue;
                break;
            case AnimatedProperty.rotation:
                startRotation = transform.rotation.z;
                break;
        }
    }

    public void Activate()
    {
        StartMovement(moveByTransform: !moveToEndPosition);
    }

    public void Deactivate()
    {
        StopAllCoroutines();
        ResetState();
    }
}
