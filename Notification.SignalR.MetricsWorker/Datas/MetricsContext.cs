using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.SignalR.MetricsWorker.Datas
{
    public class MetricsContext : DbContext
    {
        public MetricsContext(DbContextOptions<MetricsContext> options) : base(options)
        { }

        public DbSet<SignalRMetric> SignalrMetrics { get; set; }

        public DbSet<Server> Servers { get; set; }
    }
}
