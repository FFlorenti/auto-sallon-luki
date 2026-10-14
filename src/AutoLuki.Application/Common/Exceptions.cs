namespace AutoLuki.Application.Common;

public class NotFoundException(string resource, object key)
    : Exception($"{resource} with id '{key}' was not found.");

public class BusinessRuleException(string message) : Exception(message);
