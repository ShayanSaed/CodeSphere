using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodeSphere.Web.Pages.Error;

public class ForbiddenModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Message { get; set; }
    public void OnGet() { }
}
