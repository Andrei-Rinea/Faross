using System;
using Faross.Services;
using Faross.ViewModels.Stats;
using Microsoft.AspNetCore.Mvc;

namespace Faross.Controllers
{
    public class StatsController : Controller
    {
        private readonly ICheckStats _checkStats;

        public StatsController(ICheckStats checkStats)
        {
            _checkStats = checkStats ?? throw new ArgumentNullException(nameof(checkStats));
        }

        public ActionResult Index()
        {
            var stats = _checkStats.GetAllStats();
            var viewModel = new IndexViewModel(stats);
            return View(viewModel);
        }
    }
}