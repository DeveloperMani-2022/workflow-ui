using Microsoft.AspNetCore.Mvc;

namespace agent_builder_host.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Main entry point - serves the AI Agent Builder
    /// </summary>
    public IActionResult Index()
    {
        _logger.LogInformation("Loading AI Agent Builder");
        return View();
    }

    /// <summary>
    /// API endpoint to get configuration
    /// </summary>
    [HttpGet]
    public IActionResult GetConfig()
    {
        var config = new
        {
            apiBaseUrl = "http://localhost:5031/api/workflow",
            environment = "Development"
        };
        return Json(config);
    }
}
