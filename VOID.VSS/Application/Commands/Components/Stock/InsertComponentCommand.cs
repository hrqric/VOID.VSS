namespace VOID.VSS.Application.Commands.Components.Stock;

public class InsertComponentCommand
{
    public string ComponentName { get; set; }
    public string ComponentClass { get; set; }
    public int Address { get; set; }
    
    public int? Quantity { get; set; }
    
}