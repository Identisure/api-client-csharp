using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class BankIdNoEndpoint
{
    private readonly Transport _transport;

    internal BankIdNoEndpoint(Transport transport)
    {
        _transport = transport;
    }

    public Task<IBankIdNoStatus> AuthAsync(BankIdNoAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdNoStatus>("/v3/bankid-no/auth", req, ct);

    public Task<IBankIdNoStatus> BackchannelAuthAsync(BankIdNoBackchannelAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdNoStatus>("/v3/bankid-no/backchannel/auth", req, ct);

    public Task<IBankIdNoStatus> SignAsync(BankIdNoSignRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdNoStatus>("/v3/bankid-no/sign", req, ct);

    public Task<IBankIdNoStatus> AuthStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IBankIdNoStatus>("/v3/bankid-no/auth/" + id, ct);

    public Task<IBankIdNoStatus> SignStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IBankIdNoStatus>("/v3/bankid-no/sign/" + id, ct);

    public Task CancelAuthAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/bankid-no/auth/" + id, ct);

    public Task CancelSignAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/bankid-no/sign/" + id, ct);

    public Task<IBankIdNoStatus> WaitForAuthAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
        => PollAsync(() => AuthStatusAsync(id, ct), opts, ct);

    public Task<IBankIdNoStatus> WaitForSignAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
        => PollAsync(() => SignStatusAsync(id, ct), opts, ct);

    private static async Task<IBankIdNoStatus> PollAsync(Func<Task<IBankIdNoStatus>> fn, PollOptions? opts, CancellationToken ct)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await fn().ConfigureAwait(false);
            if (status is not BankIdNoPending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }
}
