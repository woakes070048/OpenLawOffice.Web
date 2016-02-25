using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenLawOffice.Web.ViewModels.Home
{
    public class DashboardGraphDataViewModel
    {
        public List<ChartItem> TasksInActiveMatters { get; set; }
        public List<ChartItem> TopBillingClients { get; set; }

        public DashboardGraphDataViewModel()
        {
            TasksInActiveMatters = new List<ChartItem>();
            TopBillingClients = new List<ChartItem>();
        }

        public class ChartItem
        {
            public double value { get; set; }
            public string color { get; set; }
            public string highlight { get; set; }
            public string label { get; set; }
        }
    }
}