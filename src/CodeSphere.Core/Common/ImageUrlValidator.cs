namespace CodeSphere.Core.Common;

/// <summary>
/// Validates user-supplied profile image URLs before they are ever stored or
/// rendered. Shared by both the Razor Pages Register page and the API's
/// registration/profile endpoints, so the rule is enforced exactly once.
///
/// Security notes:
///  - The application never fetches this URL server-side; it is only ever
///    rendered client-side as an &lt;img src="..."&gt;. That means there is no
///    server-side SSRF surface from this field (SSRF requires the *server*
///    to make an outbound request to an attacker-chosen address, which this
///    code never does).
///  - The real risk is a value that a browser might treat as *executable*
///    rather than as image data — e.g. a "javascript:" or "data:" URI. Modern
///    browsers already refuse to run "javascript:" from an &lt;img src&gt;, and
///    Razor HTML-encodes the attribute value automatically, but this
///    validator still rejects anything other than plain http/https at the
///    source so a malicious value is never persisted in the first place.
///  - Length is capped to match the database column (nvarchar(255)) so an
///    oversized value fails with a friendly validation message instead of a
///    raw SqlException from EF Core.
/// </summary>
public static class ImageUrlValidator
{
    private const int MaxLength = 255;

    public static bool IsValid(string? url, out string? errorMessage)
    {
        errorMessage = null;

        // The field is optional — no URL at all is valid (the UI falls back
        // to a placeholder icon).
        if (string.IsNullOrWhiteSpace(url))
            return true;

        var trimmed = url.Trim();

        if (trimmed.Length > MaxLength)
        {
            errorMessage = $"Profile image URL cannot exceed {MaxLength} characters.";
            return false;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            errorMessage = "Profile image URL must be a valid, absolute URL (e.g. https://example.com/photo.jpg).";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            errorMessage = "Profile image URL must start with http:// or https://.";
            return false;
        }

        return true;
    }

    /// <summary>Returns the trimmed URL, or null if it was empty/whitespace.</summary>
    public static string? Normalize(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.Trim();
}
