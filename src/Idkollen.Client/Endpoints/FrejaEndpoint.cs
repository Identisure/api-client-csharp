using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class FrejaEndpoint
{
    private readonly Transport _transport;

    internal FrejaEndpoint(Transport transport) { _transport = transport; }

    public Task<IFrejaStatus> AuthAsync(FrejaAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IFrejaStatus>("/v3/freja/auth", req, ct);
    public Task<IFrejaStatus> BackchannelAuthAsync(FrejaBackchannelAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IFrejaStatus>("/v3/freja/backchannel/auth", req, ct);
    public Task<IFrejaStatus> SignAsync(FrejaSignRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IFrejaStatus>("/v3/freja/sign", req, ct);
    public Task<IFrejaStatus> BackchannelSignAsync(FrejaBackchannelSignRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IFrejaStatus>("/v3/freja/backchannel/sign", req, ct);
    public Task<IFrejaStatus> AuthStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IFrejaStatus>("/v3/freja/auth/" + id, ct);
    public Task<IFrejaStatus> SignStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IFrejaStatus>("/v3/freja/sign/" + id, ct);
    public Task CancelAuthAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/freja/auth/" + id, ct);
    public Task CancelSignAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/freja/sign/" + id, ct);

    public Task<IFrejaStatus> WaitForAuthAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
        => PollAsync(() => AuthStatusAsync(id, ct), opts, ct);
    public Task<IFrejaStatus> WaitForSignAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
        => PollAsync(() => SignStatusAsync(id, ct), opts, ct);

    private static async Task<IFrejaStatus> PollAsync(Func<Task<IFrejaStatus>> fn, PollOptions? opts, CancellationToken ct)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await fn().ConfigureAwait(false);
            if (status is not FrejaPending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }
}
