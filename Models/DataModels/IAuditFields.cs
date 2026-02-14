namespace Models.DataModels;

public interface IAuditFields : ICreatedAuditFields
{
    public DateTime DateModified { get; set; }
}