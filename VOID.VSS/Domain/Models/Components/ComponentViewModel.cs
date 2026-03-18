using Swashbuckle.AspNetCore.Filters;

namespace VOID.VSS.Domain.Models.Components;

public class ComponentViewModel : IExamplesProvider<ComponentViewModel>
{
    public Guid Id { get; set; }
    public string ComponentName { get; set; }
    public string ComponentClass { get; set; }
    public int Address { get; set; }

    public ComponentViewModel GetExamples()
    {
        return new()
        {
            Id = Guid.Parse("55d0d7dd-b314-430c-841d-0fa83c94c005"),
            ComponentName = "string",
            ComponentClass = "string",
            Address = 0,
        };
    }

}