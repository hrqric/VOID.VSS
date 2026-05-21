using Swashbuckle.AspNetCore.Filters;

namespace VOID.VSS.Domain.Models.Components;

public class ComponentViewModel : IExamplesProvider<ComponentViewModel>
{
    public Guid Id { get; set; } 
    public string ComponentName { get; set; }
    public string ComponentClass { get; set; }
    public int Address { get; set; }
    public string Status  { get; set; }
    public string Details { get; set; }
    public float Price { get; set; }
    
    public int Quantity { get; set; }

    public ComponentViewModel GetExamples()
    {
        return new()
        {
            Id = Guid.Parse("55d0d7dd-b314-430c-841d-0fa83c94c005"),
            ComponentName = "string",
            ComponentClass = "string",
            Address = 0,
            Status = "string",
            Details = "string",
            Price = 0.1f,
            Quantity = 0
        };
    }

}