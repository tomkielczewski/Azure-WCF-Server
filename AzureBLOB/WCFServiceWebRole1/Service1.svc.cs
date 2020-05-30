using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // UWAGA: możesz użyć polecenia „Zmień nazwę” w menu „Refaktoryzuj”, aby zmienić nazwę klasy „Service1” w kodzie, usłudze i pliku konfiguracji.
    // UWAGA: aby uruchomić klienta testowego WCF w celu przetestowania tej usługi, wybierz plik Service1.svc lub Service1.svc.cs w eksploratorze rozwiązań i rozpocznij debugowanie.
    public class Service1 : IService1
    {
        private static CloudTable GetTableReference(string tableName)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            table.CreateIfNotExists();
            return table;
        }

        private static CloudBlobContainer GetBlobContainer(string containerName)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

            return container;
        }

        public bool Create(string login, string password)
        {

            var table = GetTableReference("users");

            var operationDoesTableExists = TableOperation.Retrieve<User>(login, password);
            var existsResult = table.Execute(operationDoesTableExists);
            
            if(existsResult.Result != null)
            {
                return false;
            }


            var user = new User(login, password)
            {
                Login = login,
                Password = password,
                SessionId = Guid.Empty
            };

            TableOperation operationInsert = TableOperation.Insert(user);
            var insertResult = table.Execute(operationInsert);

            if (insertResult.Result == null)
            {
                return false;
            }

            return true;
        }

        public Guid Login(string login, string password)
        {
            var table = GetTableReference("users");

            var operationDoesTableExists = TableOperation.Retrieve<User>(login, password);
            var existsResult = table.Execute(operationDoesTableExists);

            if (!(existsResult.Result is User user))
            {
                return Guid.Empty;
            }

            var guid = Guid.NewGuid();
            user.SessionId = guid;

            var operationUpdate = TableOperation.Replace(user);
            table.Execute(operationUpdate);

            return guid;
        }

        public bool Logout(string login)
        {
            var table = GetTableReference("users");

            TableQuery<User> query =
                new TableQuery<User>()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, login));
            var resultQuery = table.ExecuteQuery(query);
            var user = resultQuery.SingleOrDefault();

            if (user == null)
            {
                return false;
            }

            user.SessionId = Guid.Empty;

            var updateOperation = TableOperation.Replace(user);
            table.Execute(updateOperation);

            return true;
        }

        public bool Put(string name, string content, Guid sessionId)
        {
            var table = GetTableReference("users");
            var container = GetBlobContainer("container");

            TableQuery<User> query = new TableQuery<User>()
               .Where(TableQuery.GenerateFilterConditionForGuid("SessionId", QueryComparisons.Equal, sessionId));
            var result = table.ExecuteQuery(query);
            var user = result.SingleOrDefault();

            if (user == null)
            {
                return false;
            }

            var blob = container.GetBlockBlobReference(name);
            var bytes = new System.Text.ASCIIEncoding().GetBytes(content);
            var s = new System.IO.MemoryStream(bytes);
            blob.UploadFromStream(s);
            return true;
        }

        public string Get(string name, Guid sessionId)
        {
            var table = GetTableReference("users");
            var container = GetBlobContainer("container");

            TableQuery<User> query = new TableQuery<User>()
                .Where(TableQuery.GenerateFilterConditionForGuid("SessionId", QueryComparisons.Equal, sessionId));
            var result = table.ExecuteQuery(query);
            var user = result.SingleOrDefault();

            if (user == null)
            {
                return "Niepoprawne ID sesji.";
            }

            
            var blob = container.GetBlockBlobReference(name);
            if (blob == null)
            {
                return "Nie ma pliku o takiej nazwie.";
            }

            var s = new System.IO.MemoryStream();
            blob.DownloadToStream(s);
            string content = System.Text.Encoding.UTF8.GetString(s.ToArray());
            return content;
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
