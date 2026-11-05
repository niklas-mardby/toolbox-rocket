namespace CustomerAuditTrail;

public record Customer(int Id, string Name, string Email, bool Active);

public record CreateCustomerRequest(string? Name, string? Email);
public record UpdateEmailRequest(string? Email);
public record ErrorResponse(IReadOnlyList<string> Errors);

public enum OperationResult { Ok, NotFound, Conflict }

public class CustomerStore
{
    private readonly Lock _lock = new();
    private readonly Dictionary<int, Customer> _customers = [];
    private int _nextId = 1;

    public Customer Add(string name, string email)
    {
        lock (_lock)
        {
            var customer = new Customer(_nextId++, name, email, Active: true);
            _customers[customer.Id] = customer;
            return customer;
        }
    }

    public Customer? Find(int id)
    {
        lock (_lock) return _customers.GetValueOrDefault(id);
    }

    public IReadOnlyList<Customer> All()
    {
        lock (_lock) return _customers.Values.OrderBy(c => c.Id).ToList();
    }

    public void Save(Customer customer)
    {
        lock (_lock) _customers[customer.Id] = customer;
    }
}

public class CustomerService(CustomerStore store)
{
    public Customer Create(string name, string email) => store.Add(name.Trim(), email.Trim());

    public OperationResult UpdateEmail(int id, string email)
    {
        var customer = store.Find(id);
        if (customer is null) return OperationResult.NotFound;
        store.Save(customer with { Email = email.Trim() });
        return OperationResult.Ok;
    }

    public OperationResult Deactivate(int id) => SetActive(id, active: false);

    public OperationResult Reactivate(int id) => SetActive(id, active: true);

    private OperationResult SetActive(int id, bool active)
    {
        var customer = store.Find(id);
        if (customer is null) return OperationResult.NotFound;
        if (customer.Active == active) return OperationResult.Conflict;
        store.Save(customer with { Active = active });
        return OperationResult.Ok;
    }
}

// --- Compliance asked for this last month. Nothing writes to it yet. ---

public record AuditEntry(string User, string Action, string Target, string Outcome, DateTimeOffset At);

public class AuditLog
{
    private readonly Lock _lock = new();
    private readonly List<AuditEntry> _entries = [];

    public IReadOnlyList<AuditEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    public void Write(AuditEntry entry)
    {
        lock (_lock) _entries.Add(entry);
    }
}
