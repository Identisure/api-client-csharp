using Idkollen.Client.Endpoints;

namespace Idkollen.Client;

public sealed class IdkollenClient
{
    private readonly Transport _transport;

    internal IdkollenClient(Transport transport)
    {
        _transport = transport;
        BankIdSe = new BankIdSeEndpoint(transport);
        BankIdNo = new BankIdNoEndpoint(transport);
        Freja = new FrejaEndpoint(transport);
        MitId = new MitIdEndpoint(transport);
        Ftn = new FtnEndpoint(transport);
        Vipps = new VippsEndpoint(transport);
        Document = new DocumentEndpoint(transport);
    }

    public BankIdSeEndpoint BankIdSe { get; }
    public BankIdNoEndpoint BankIdNo { get; }
    public FrejaEndpoint Freja { get; }
    public MitIdEndpoint MitId { get; }
    public FtnEndpoint Ftn { get; }
    public VippsEndpoint Vipps { get; }
    public DocumentEndpoint Document { get; }
}
