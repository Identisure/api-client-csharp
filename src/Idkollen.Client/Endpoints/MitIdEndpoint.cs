using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class MitIdEndpoint
{
    private readonly Transport _transport;
    internal MitIdEndpoint(Transport transport) { _transport = transport; }

    public Task<IMitIdStatus> AuthAsync(MitIdAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IMitIdStatus>("/v3/mitid/auth", req, ct);
    public Task<IMitIdStatus> BackchannelAuthAsync(MitIdBackchannelAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IMitIdStatus>("/v3/mitid/backchannel/auth", req, ct);
    public Task<IMitIdStatus> SignAsync(MitIdSignRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IMitIdStatus>("/v3/mitid/sign", req, ct);
    public Task<IMitIdStatus> AuthStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IMitIdStatus>("/v3/mitid/auth/" + id, ct);
    public Task<IMitIdStatus> SignStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IMitIdStatus>("/v3/mitid/sign/" + id, ct);
    public Task CancelAuthAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/mitid/auth/" + id, ct);
    public Task CancelSignAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/mitid/sign/" + id, ct);

    public Task<IMitIdStatus> WaitForAuthAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
        => PollAsync(() => AuthStatusAsync(id, ct), opts, ct);
    public Task<IMitIdStatus> WaitForSignAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
        => PollAsync(() => SignStatusAsync(id, ct), opts, ct);

    private static async Task<IMitIdStatus> PollAsync(Func<Task<IMitIdStatus>> fn, PollOptions? opts, CancellationToken ct)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await fn().ConfigureAwait(false);
            if (status is not MitIdPending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }
}
