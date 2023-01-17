namespace TomLonghurst.PullRequestScanner.Models;

public class MutableGroup<TKey, TValue>
{
    public MutableGroup(TKey key, IEnumerable<TValue> values)
    {
        Key = key;
        Values = values.ToList();
    }
    
    public TKey Key { get; }
    public List<TValue> Values { get; }
}