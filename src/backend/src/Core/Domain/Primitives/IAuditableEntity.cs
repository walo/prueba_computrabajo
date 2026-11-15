namespace Domain.Primitives;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? ModifiedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    string? ModifiedBy { get; set; }
}
