using System;
using System.Collections.Generic;
using System.Linq;
using Faross.Models;
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
            var allStats = _checkStats.GetAllStats();
            var statsByEnvironment = allStats
                .GroupBy(stat => stat.Check.Environment, stat => stat)
                .ToDictionary(group => group.Key, group => (IEnumerable<Statistics>)group);

            var viewModel = new IndexViewModel(statsByEnvironment);
            return View(viewModel);
        }
    }
}