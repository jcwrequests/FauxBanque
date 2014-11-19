using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Faux.Banque.Domain.Tests.EventStore;
using Faux.Banque.Domain.Aggregates;
using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.ValueObjects;
using Faux.Banque.Domain.Storage;
using System.Linq;

namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class CustomerTests
    {
        static InMemoryEventStore store = new InMemoryEventStore();

        [TestMethod]
        public void Create_Customer()
        {
            CustomerApplicationService service = new CustomerApplicationService(store);
            CustomerId id = new CustomerId("1");
            service.Execute(new CreateCustomer("John", "Smith", id));
            EventStream stream = store.LoadEventStream(id);
            CustomerState state = new CustomerState(stream.Events);
            Assert.IsTrue(state.CustomerId.Equals(id));
        }
        [TestMethod]
        public void Create_Customer_And_Open_A_SavingsAcccount()
        {
            CustomerApplicationService service = new CustomerApplicationService(store);
            CustomerId id = new CustomerId("1");
            service.Execute(new CreateCustomer("John", "Smith", id));
            service.Execute(new OpenSavingsAccount(id, new AccountId("12"), 100));
            EventStream stream = store.LoadEventStream(id);
            CustomerState state = new CustomerState(stream.Events);
            Assert.IsTrue(state.CustomerId.Equals(id));
            Assert.IsTrue(state.Accounts.First().Value.TotalBalance.Equals(100));

        }
        [TestMethod]
        public void Create_Customer_And_Open_Two_Accounts()
        {
            CustomerApplicationService service = new CustomerApplicationService(store);
            CustomerId id = new CustomerId("1");
            service.Execute(new CreateCustomer("John", "Smith", id));
            service.Execute(new OpenSavingsAccount(id, new AccountId("12"), 100));
            service.Execute(new OpenCheckingAccount(id, new AccountId("13"),100));

            EventStream stream = store.LoadEventStream(id);
            CustomerState state = new CustomerState(stream.Events);
            Assert.IsTrue(state.CustomerId.Equals(id));
            Assert.IsTrue(state.Accounts.Sum(a => a.Value.TotalBalance).Equals(200));
        }
        [TestMethod]
        public void Create_Customer_And_Open_Two_Accounts_Then_Transfer_Money()
        {
            CustomerApplicationService service = new CustomerApplicationService(store);
            CustomerId id = new CustomerId("1");
            service.Execute(new CreateCustomer("John", "Smith", id));
            service.Execute(new OpenSavingsAccount(id, new AccountId("12"), 100));
            service.Execute(new OpenCheckingAccount(id, new AccountId("13"), 100));
            service.Execute(new TransferMoney(id, new AccountId("12"), new AccountId("13"), 100));

            EventStream stream = store.LoadEventStream(id);
            CustomerState state = new CustomerState(stream.Events);
            Assert.IsTrue(state.CustomerId.Equals(id));
            Assert.IsTrue(state.Accounts.Sum(a => a.Value.TotalBalance).Equals(200));
            Assert.IsTrue(state.Accounts[new AccountId("12")].TotalBalance.Equals(0));
            Assert.IsTrue(state.Accounts[new AccountId("13")].TotalBalance.Equals(200));

        }
    }
}
