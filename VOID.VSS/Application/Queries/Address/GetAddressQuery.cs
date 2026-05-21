using VOID.VSS.Domain.Enums;

namespace VOID.VSS.Application.Queries.Address;

public class GetAddressQuery
{
    public int? AddressId { get; set; }
    public EAddressClassification? Classification { get; set; }
}