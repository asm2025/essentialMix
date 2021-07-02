namespace essentialMix.Caching.ExpressionCache
{
    public interface ICacheObject
    {
        string Validator { get; set; }
        byte[] Item { get; set; }
    }
}