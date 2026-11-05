namespace CustomerAuditTrail;

public interface IValidator<in T>
{
    IEnumerable<string> Validate(T request);
}

/// <summary>The email rule, in ONE place. Before, it only existed in the create endpoint.</summary>
public static class EmailRule
{
    public static bool IsValid(string? email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Split('@')[^1].Contains('.');
}

public class CreateCustomerValidator : IValidator<CreateCustomerRequest>
{
    public IEnumerable<string> Validate(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) yield return "Name is required.";
        if (!EmailRule.IsValid(request.Email)) yield return "Email is invalid.";
    }
}

public class UpdateEmailValidator : IValidator<UpdateEmailRequest>
{
    public IEnumerable<string> Validate(UpdateEmailRequest request)
    {
        if (!EmailRule.IsValid(request.Email)) yield return "Email is invalid.";
    }
}
