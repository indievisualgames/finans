using System.Collections.Generic;

/* public class KeyValuePairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
{
    public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
    {
        return x.Key.Equals(y.Key);
    }
    public int GetHashCode(KeyValuePair<TKey, TValue> x)
    {
        return x.GetHashCode();
    }
} */

public class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, string>>
{
    public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
    {
        return x.Key.Equals(y.Key);
    }

    public int GetHashCode(KeyValuePair<string, string> obj)
    {
        return obj.Key.GetHashCode();
    }
}