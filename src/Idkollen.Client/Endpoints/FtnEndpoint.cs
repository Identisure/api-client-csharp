using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class FtnEndpoint
{
    private readonly Transport _transport;
    internal FtnEndpoint(Transport transport) { _transport = transport; }

    public Task<IFtnStatus> AuthAsync(FtnAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IFtnStatus>("/v3/ftn/auth", req, ct);
    public Task<IAgeVerificationStatus> AgeVerificationAsync(AgeVerificationRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IAgeVerificationStatus>("/v3/ftn/age-verification", req, ct);
    public Task<IFtnStatus> AuthStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IFtnStatus>("/v3/ftn/auth/" + id, ct);
    public Task<IAgeVerificationStatus> AgeVerificationStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IAgeVerificationStatus>("/v3/ftn/age-verification/" + id, ct);
    public Task CancelAuthAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/ftn/auth/" + id, ct);
    public Task CancelAgeVerificationAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/ftn/age-verification/" + id, ct);

    public async Task<IFtnStatus> WaitForAuthAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await AuthStatusAsync(id, ct).ConfigureAwait(false);
            if (status is not FtnPending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }

    public async Task<IAgeVerificationStatus> WaitForAgeVerificationAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await AgeVerificationStatusAsync(id, ct).ConfigureAwait(false);
            if (status is not AgeVerificationPending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }
}
