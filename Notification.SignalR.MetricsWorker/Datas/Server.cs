using System.ComponentModel.DataAnnotations;

namespace Notification.SignalR.MetricsWorker.Datas
{
    public class Server
    {
        [Key]
        public int ID { get; set; }

        public string ServerName { get; set; }
    }
}