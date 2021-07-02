namespace essentialMix.Caching.ExpressionCache
{
    public interface ICacheOptions
    {
        bool AllowSlidingTime { get; set; }  
        long ExpirationInMilliseconds { get; set; }
        string[] SkipProvider { get; set; }
    }
}