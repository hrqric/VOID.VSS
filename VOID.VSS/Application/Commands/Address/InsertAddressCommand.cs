using VOID.VSS.Domain.Enums;

namespace VOID.VSS.Application.Commands.Address;

public class InsertAddressCommand
{
    public int AddressId { get; set; }
    public EAddressClassification Classification { get; set; }
}