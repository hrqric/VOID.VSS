using VOID.VSS.Domain.Enums;

namespace VOID.VSS.Application.Commands.Components.Stock;

public class InsertComponentCommand
{
    public string ComponentName { get; set; }
    public EComponentCategory ComponentClass { get; set; }
    public EStatus Status { get; set; }
    public string Details { get; set; }
    public float Value { get; set; }
    public int Address { get; set; }
    public int Quantity { get; set; }
    
}