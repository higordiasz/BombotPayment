using BombotPayment.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace BombotPayment.Services
{
    public class PaymentService
    {
        private readonly IMongoCollection<Payment> _payment;

        public PaymentService(IPaymentDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _payment = database.GetCollection<Payment>(settings.PaymentCollectionName);
        }

        public List<Payment> Get() =>
            _payment.Find(book => true).ToList();

        public Payment GetByCode(string code) =>
            _payment.Find<Payment>(payment => payment.Code == code).FirstOrDefault();

        public Payment Create(Payment payment)
        {
            _payment.InsertOne(payment);
            return payment;
        }

        public void UpdateByCode(string code, Payment paymentIn) =>
            _payment.ReplaceOne(payment => payment.Code == code, paymentIn);

        public void Remove(Payment paymentIn) =>
            _payment.DeleteOne(payment => payment.Id == paymentIn.Id);

        public void RemoveByCode(string code) =>
            _payment.DeleteOne(payment => payment.Code == code);
    }
}
