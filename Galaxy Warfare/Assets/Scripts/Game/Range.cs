[System.Serializable]
public struct RangeInt
{
    public int min;
    public int max;

    public RangeInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public bool InRange(int val)
    {
        if (min <= val && val <= max)
            return true;
        return false;
    }
}
