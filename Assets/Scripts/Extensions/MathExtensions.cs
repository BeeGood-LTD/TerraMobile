public static class MathExtensions
{
    public static float ValueToRange(float max, float min, float value)
    {
        return (value - min) / (max - min);
    }
        
    public static float RangeToValue(float max, float min, float range)
    {
        return (max - min) * range + min;
    }
}