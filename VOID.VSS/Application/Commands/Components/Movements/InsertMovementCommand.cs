using VOID.VSS.Domain.Enums;

namespace VOID.VSS.Application.Commands.Components.Stock.Movements;

public class InsertMovementCommand
{
    public Guid ComponentId { get; set; }
    public int Quantity { get; set; }
    public EMovementType MovementType { get; set; }
    public string UserId { get; set; }
}