namespace Idkollen.Client;

public sealed record PollOptions(TimeSpan? Interval = null, TimeSpan? Timeout = null)
{
    public TimeSpan EffectiveInterval => Interval ?? TimeSpan.FromSeconds(2);
    public TimeSpan EffectiveTimeout => Timeout ?? TimeSpan.FromMinutes(5);
}
