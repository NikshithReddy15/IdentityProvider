using Models.DataModels;

namespace Extensions;

public static class AuditFieldExtensions
{
    public static void SetAuditFieldsOnCreate<T>(this T dataModel)
        where T : ICreatedAuditFields
    {
        var currentDate = DateTime.UtcNow;
        dataModel.DateCreated = currentDate;
        if (dataModel is IAuditFields auditFields)
        {
            auditFields.DateModified = currentDate;
        }
    }
    
    public static void SetAuditFieldsOnUpdate<T>(this T dataModel)
        where T : IAuditFields
    {
        dataModel.DateModified = DateTime.UtcNow;
    }
}