# Razorsharp.Guard

**Prevent sensitive data leaks in your .NET APIs — with attribute-based classification.**

Razorsharp.Guard is a library for ASP.NET Core that enforces **data classification at runtime**.  
It ensures that no sensitive data is accidentally exposed through API responses.

- 🔒 **Fail-safe defaults** – all unclassified data is treated as `Restricted`
- 🏷️ **Attribute-driven** – annotate your DTOs with `[Public]`, `[Confidential("reason")]`, `[Restricted("reason")]`
- 🌐 **Configurable** – policies can differ for `GuardMode.Audit` vs `GuardMode.ThrowException`
- 📜 **Audit trail** – every sensitive access can be logged for compliance (ISO 27001, NIS2, GDPR)

---

## Installation

Add the NuGet package:

```bash
dotnet add package Razorsharp.Guard
```

---

## Usage

In your `Program.cs`, after `AddControllers`:

```csharp
builder.Services.AddControllers();

builder.Services.AddRazorsharpGuard((options) =>
{
    // Define context for this application
    options.Context = GuardContext.ThrowException; // default Audit that will not throw exception

    // Define what happens when sensitive data is detected
                options.Audit = (logger, httpContext, evt) =>
                {
                    var user = httpContext.User?.Identity?.Name ?? "anonymous";
                    var path = httpContext.Request.Path;
                    var method = httpContext.Request.Method;
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    var worst = evt.Classifications
                        .OrderByDescending(c => c.SensitivityLevel)
                        .FirstOrDefault();

                    logger.LogWarning(
                        "Sensitive data access detected. User={User}, Path={Method} {Path}, IP={IP}, MaxLevel={Level}, Reason={Reason}, Types=[{Types}]",
                        user,
                        method,
                        path,
                        ip,
                        worst?.SensitivityLevel,
                        worst?.Reason ?? "n/a",
                        string.Join(", ", evt.Classifications.Select(c => c.Type))
                    );
                };
});
```

---

## Example DTOs

```csharp
public class CustomerDTO
{
    [Public] 
    public string DisplayName { get; set; } = string.Empty; 

    [Internal("ISO 27001 A.9.2 – Only for authenticated employees")]
    public string InternalNotes { get; set; } = string.Empty;

    [Confidential("GDPR Art. 6 – Email is personal data, protect from external sharing")]
    public string Email { get; set; } = string.Empty;

    [Confidential("NIS2 Art. 21 – Availability and integrity of critical service data")]
    public DateTime LastLogin { get; set; }

    [Restricted("GDPR Art. 9 – Contains health data (sensitive category)")]
    public string MedicalCondition { get; set; } = string.Empty;

    [Restricted("ISO 27001 A.10.1 – Strong protection of encryption keys")]
    public Guid EncryptionKeyId { get; set; }
}
```

- If an controller and GuardMode.ThrowException returns `SensitiveDTO`, the response is blocked and a `RazorsharpGuardException` is thrown.  
- If the same DTO is returned and GuardMode.Audit, the response is allowed but logged for audit.

---

## How it works

1. **Classification**  
   Mark DTOs and properties with classification attributes.

2. **Evaluation**  
   A result filter inspects all responses. Unclassified = `Restricted` by default.

3. **Policy enforcement**  
   Depending on context, the Guard decides to **Allow**, **Block**, or **Log**.

---

## Compliance

Razorsharp.Guard helps map code to compliance frameworks like:

- ISO 27001 (Access Control, Information Handling)
- NIS2 (Risk management, Audit logging)
- GDPR (Data minimization, Access control)

---

## Development & Tests

Unit tests are included to validate classification across combinations:  
public, confidential, restricted, nested DTOs, inheritance, lists, nullables, etc.

Run them with:

```bash
dotnet test
```

---

## License

This project is licensed under the **AGPL-3.0**.  
You may freely use, study and modify the code, but distributing modified versions or running it as a service requires making your source code available under the same license.  

For commercial licensing, please contact license@razorsharp.dev 
