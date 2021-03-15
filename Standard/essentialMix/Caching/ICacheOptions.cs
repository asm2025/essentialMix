namespace essentialMix.Caching
{
    public interface ICacheOptions
    {
        bool AllowSlidingTime { get; set; }  
        long ExpirationInMilliSeconds { get; set; }
        string[] SkipProvider { get; set; }
    }
}