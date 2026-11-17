namespace RSS.DTOs
{
    // The one error-response shape every endpoint should use. FieldErrors is reserved for
    // a later pass that maps specific invalid fields (e.g. SignUp's email) — not wired to
    // any controller logic yet, just present so the frontend type doesn't need to change twice.
    public record ApiErrorResponse(string Message, Dictionary<string, string[]>? FieldErrors = null);
}
