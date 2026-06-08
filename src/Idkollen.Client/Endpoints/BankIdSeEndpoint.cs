using Idkollen.Client.Models;

namespace Idkollen.Client.Endpoints;

public sealed class BankIdSeEndpoint
{
    private readonly Transport _transport;

    internal BankIdSeEndpoint(Transport transport)
    {
        _transport = transport;
    }

    public Task<IBankIdSeStatus> AuthAsync(BankIdSeAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdSeStatus>("/v3/bankid-se/auth", req, ct);

    public Task<IBankIdSePhoneStatus> PhoneAuthAsync(BankIdSePhoneAuthRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdSePhoneStatus>("/v3/bankid-se/phone/auth", req, ct);

    public Task<IBankIdSeStatus> SignAsync(BankIdSeSignRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdSeStatus>("/v3/bankid-se/sign", req, ct);

    public Task<IBankIdSePhoneStatus> PhoneSignAsync(BankIdSePhoneSignRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IBankIdSePhoneStatus>("/v3/bankid-se/phone/sign", req, ct);

    public Task<BankIdSeVerifyResponse> VerifyAsync(BankIdSeVerifyRequest req, CancellationToken ct = default)
        => _transport.PostAsync<BankIdSeVerifyResponse>("/v3/bankid-se/verify", req, ct);

    public Task<IAgeVerificationStatus> AgeVerificationAsync(AgeVerificationRequest req, CancellationToken ct = default)
        => _transport.PostAsync<IAgeVerificationStatus>("/v3/bankid-se/age-verification", req, ct);

    public Task<IBankIdSeStatus> AuthStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IBankIdSeStatus>("/v3/bankid-se/auth/" + id, ct);

    public Task<IBankIdSeStatus> SignStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IBankIdSeStatus>("/v3/bankid-se/sign/" + id, ct);

    public Task<IAgeVerificationStatus> AgeVerificationStatusAsync(string id, CancellationToken ct = default)
        => _transport.GetAsync<IAgeVerificationStatus>("/v3/bankid-se/age-verification/" + id, ct);

    public Task CancelAuthAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/bankid-se/auth/" + id, ct);

    public Task CancelSignAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/bankid-se/sign/" + id, ct);

    public Task CancelAgeVerificationAsync(string id, CancellationToken ct = default)
        => _transport.DeleteAsync("/v3/bankid-se/age-verification/" + id, ct);

    public async Task<IBankIdSeStatus> WaitForAuthAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await AuthStatusAsync(id, ct).ConfigureAwait(false);
            if (status is not BankIdSePending) return status;
            if (DateTime.UtcNow >= deadline) throw new WaitException(timeout: true);
            await Task.Delay(opts.EffectiveInterval, ct).ConfigureAwait(false);
        }
    }

    public async Task<IBankIdSeStatus> WaitForSignAsync(string id, PollOptions? opts = null, CancellationToken ct = default)
    {
        opts ??= new PollOptions();
        var deadline = DateTime.UtcNow + opts.EffectiveTimeout;
        while (true)
        {
            var status = await SignStatusAsync(id, ct).ConfigureAwait(false);
            if (status is not BankIdSePending) return status;
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
