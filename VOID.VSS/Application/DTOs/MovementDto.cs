using VOID.VSS.Domain.Enums;

namespace VOID.VSS.Application.DTOs;

public class MovementDto
{
    public Guid ComponentId { get; set; }
    public int Quantity { get; set; }
    public EMovementType MovementType { get; set; }
    
    public Guid MovId  { get; set; }
}