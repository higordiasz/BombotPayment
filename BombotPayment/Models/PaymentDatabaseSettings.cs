using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BombotPayment.Models
{
    public class PaymentDatabaseSettings : IPaymentDatabaseSettings
    {
        public string PaymentCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IPaymentDatabaseSettings
    {
        string PaymentCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
