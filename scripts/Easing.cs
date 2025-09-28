using Godot;

// see: https://easings.net/
public static class Easing
{
    /// <summary>
    /// Remap a value between min and max using an easing function
    /// </summary>
    /// <param name="value">The value to ease</param>
    /// <param name="min">Min value</param>
    /// <param name="max">Max value</param>
    /// <param name="easingFnc">The easing function</param>
    /// <returns>Value eased between min and max</returns>
    public static float Remap(float value, float min, float max, System.Func<float, float> easingFnc)
    {
        return Mathf.Lerp(min, max, easingFnc(Mathf.InverseLerp(min, max, value)));
    }

    /// <summary>
    /// Remap a value to 0-1 using an easing function
    /// </summary>
    /// <param name="min">Min value</param>
    /// <param name="max">Max value</param>
    /// <param name="value">The value to ease</param>
    /// <param name="easingFnc">The easing function</param>
    /// <returns>Value between 0 and 1</returns>
    public static float InverseLerp(float min, float max, float value, System.Func<float, float> easingFnc)
    {
        return easingFnc(Mathf.InverseLerp(min, max, value));
    }

    /// <summary>
    /// Interpolates value between min and max using an easing function.
    /// </summary>
    /// <param name="min">Min value</param>
    /// <param name="max">Max value</param>
    /// <param name="value">The value to ease</param>
    /// <param name="easingFnc">The easing function</param>
    /// <returns>Value between 0 and 1</returns>
    public static float Lerp(float min, float max, float value, System.Func<float, float> easingFnc)
    {
        return Mathf.Lerp(min, max, easingFnc(Mathf.Clamp(value, 0, 1)));
    }

    public static float Linear(float x)
    {
        return x;
    }

    public static float InQuad(float x)
    {
        return x * x;
    }
    public static float OutQuad(float x)
    {
        return 1f - (1f - x) * (1f - x);
    }
    public static float InOutQuad(float x)
    {
        return x < 0.5f ? 2f * x * x : 1f - Mathf.Pow(-2f * x + 2f, 2f) / 2f;
    }

    public static float InCubic(float x)
    {
        return x * x * x;
    }
    public static float OutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }
    public static float InOutCubic(float x)
    {
        return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }

    public static float InQuart(float x)
    {
        return x * x * x * x;
    }
    public static float OutQuart(float x)
    {
        return 1f - Mathf.Pow(1 - x, 4f);
    }
    public static float InOutQuart(float x)
    {
        return x < 0.5f ? 8f * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 4f) / 2f;
    }

    public static float InQuint(float x)
    {
        return x * x * x * x * x;
    }
    public static float OutQuint(float x)
    {
        return 1f - Mathf.Pow(1f - x, 5f);
    }
    public static float InOutQuint(float x)
    {
        return x < 0.5f ? 16f * x * x * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 5f) / 2f;
    }

    public static float InExpo(float x)
    {
        return x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);
    }
    public static float OutExpo(float x)
    {
        return x == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);
    }
    public static float InOutExpo(float x)
    {
        return x == 0f
            ? 0f
            : x == 1f
            ? 1f
            : x < 0.5f
            ? Mathf.Pow(2f, 20f * x - 10f) / 2f
            : (2f - Mathf.Pow(2f, -20f * x + 10f)) / 2f;
    }

    public static float InBack(float x, float backAmount = 1.70158f)
    {
        return (backAmount + 1f) * x * x * x - backAmount * x * x;
    }
    public static float OutBack(float x, float backAmount = 1.70158f)
    {
        return 1f + (backAmount + 1f) * Mathf.Pow(x - 1f, 3f) + backAmount * Mathf.Pow(x - 1f, 2f);
    }
    public static float InOutBack(float x, float backAmount = 1.70158f, float stabilize = 1.525f)
    {
        return x < 0.5f
            ? (Mathf.Pow(2f * x, 2f) * (((backAmount * stabilize) + 1f) * 2f * x - (backAmount * stabilize))) / 2f
            : (Mathf.Pow(2f * x - 2f, 2f) * (((backAmount * stabilize) + 1f) * (x * 2f - 2f) + (backAmount * stabilize)) + 2f) / 2f;
    }

    // UNITY ONLY
    // public static AnimationCurve ToAnimationCurve(EasingType easingType)
    // {
    //     return EasingAnimationCurve.EaseToAnimationCurve(easingType);
    // }
}
