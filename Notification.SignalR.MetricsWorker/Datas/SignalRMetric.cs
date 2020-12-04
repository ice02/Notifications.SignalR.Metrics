using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notification.SignalR.MetricsWorker.Datas
{
    public class SignalRMetric
    {
        private string _extendedData;

        public Server HostServer { get; set; }

        [NotMapped]
        public JObject ExtendedData
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(string.IsNullOrEmpty(_extendedData) ? "{}" : _extendedData);
            }
            set
            {
                _extendedData = value.ToString();
            }
        }

        public DateTime CreationDate { get; set; }
    }
}