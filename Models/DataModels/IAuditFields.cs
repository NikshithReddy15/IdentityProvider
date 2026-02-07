namespace Models.DataModels;

public interface IAuditFields
{
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}