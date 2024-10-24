namespace tennis.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<tennisUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<tennisUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
