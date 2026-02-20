using Microsoft.AspNetCore.Mvc;

namespace AIATC.BFF.Controllers;

/// <summary>
/// Browsers automatically request /favicon.ico on every page load.
/// The WASM project uses favicon.png, so /favicon.ico does not exist.
/// Without this handler, MapFallbackToFile skips the .ico extension and
/// the request falls through to the Container Apps ingress, which times out (504).
/// Redirect to favicon.png so browsers get the actual icon.
/// </summary>
[ApiController]
public class FaviconController : ControllerBase
{
    [HttpGet("/favicon.ico")]
    public IActionResult Favicon() => RedirectPermanent("/favicon.png");
}
