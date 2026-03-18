using Swashbuckle.AspNetCore.Filters;
using VOID.VSS.Domain.Enums;

namespace VOID.VSS.Domain.Models.Components;

public class AddressViewModel : IExamplesProvider<AddressViewModel>
{
    public int AddressId { get; set; }
    public required string Classification { get; set; }

    public AddressViewModel GetExamples()
    {
        return new()
        {
            AddressId = AddressId,
            Classification = Classification
        };
    }
}