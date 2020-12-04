using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Notification.SignalR.MetricsWorker.Datas;

namespace Notification.SignalR.MetricsWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private int _runIntervall;
        private int _numberOfDaysBeforeDelete;

        private System.Diagnostics.PerformanceCounter _connectionCnt = new System.Diagnostics.PerformanceCounter("", "", "");
        private System.Diagnostics.PerformanceCounter _messageCnt = new System.Diagnostics.PerformanceCounter("", "", "");
        private System.Diagnostics.PerformanceCounter _requestsCnt = new System.Diagnostics.PerformanceCounter("", "", "");
        private System.Diagnostics.PerformanceCounter _errorsCnt = new System.Diagnostics.PerformanceCounter("", "", "");

        private readonly string _serverName = System.Environment.MachineName;

        private MetricsContext _context;
        private Server currentServer;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var _configuration = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
                _context = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<MetricsContext>();
                _numberOfDaysBeforeDelete = int.Parse(_configuration["App.Configurations:NumberOfDaysBeforeDelete"]);
                _runIntervall = int.Parse(_configuration["App.Configurations:RunIntervall"]);

                var existServer = _context.Servers.Count(p => p.ServerName == _serverName) > 0;
                if (!existServer)
                {
                    _context.Servers.Add(new Server()
                    {
                        ServerName = _serverName
                    });
                    _context.SaveChanges();
                }

                currentServer = _context.Servers.FirstOrDefault(p => p.ServerName == _serverName);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                if (!stoppingToken.IsCancellationRequested)
                {
                    //TODO : get metrics from local perfmon
                    _connectionCnt.NextValue();
                    _messageCnt.NextValue();
                    _requestsCnt.NextValue();
                    _errorsCnt.NextValue();

                    //TODO : push it in database with servername key
                    var newEntry = _context.SignalrMetrics.Add(new SignalRMetric()
                    {
                        CreationDate = DateTime.Now,
                        HostServer = currentServer,
                        ExtendedData = new JObject(new { })
                    });

                    //TODO : Check for remove old data
                    await Task.Factory.StartNew(() =>
                    {
                         CheckForClean();
                    });
                }

                await Task.Delay(TimeSpan.FromSeconds(_runIntervall), stoppingToken);
            }
        }

        /// <summary>
        /// Check in DB if data must be removed
        /// </summary>
        /// <returns></returns>
        protected void CheckForClean()
        { 

        }
    }
}
