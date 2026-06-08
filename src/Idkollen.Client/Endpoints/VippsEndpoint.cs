using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class VippsEndpoint
{
    private readonly Transport _transport;
    internal VippsEndpoint(Transport transport) { _transport = transport; }

    public Task<IVippsStatus> AuthAsync(VippsAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IVippsStatus>("/v3/vipps/auth", req, ct);
    public Task<IVippsStatus> BackchannelAuthAsync(VippsBackchannelAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IVippsStatus>("/v3/vipps/backchannel/auth", req, ct);
    public Task<IVippsStatus> AuthStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IVippsStatus>("/v3/vipps/auth/" + id, ct);
    public Task CancelAuthAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/vipps/auth/" + id, ct);

    public async Task<IVippsStatus> WaitForAuthAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await AuthStatusAsync(id, ct).ConfigureAwait(false);
            if (status is not VippsPending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }
}
