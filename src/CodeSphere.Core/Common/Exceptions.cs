namespace CodeSphere.Core.Common;

/// <summary>Thrown when a requested entity (article, comment, user, ...) does not exist.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.") { }
}

/// <summary>Thrown for business-rule violations (e.g. duplicate follow, empty comment).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Thrown when the current user is not allowed to perform an action on an entity they don't own.</summary>
public class ForbiddenActionException : Exception
{
    public ForbiddenActionException(string message = "You are not allowed to perform this action.") : base(message) { }
}
