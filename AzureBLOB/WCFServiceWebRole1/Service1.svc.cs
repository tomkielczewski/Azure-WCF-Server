using Microsoft.WindowsAzure.Storage;
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
        public bool Create(string login, string password)
        {

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient client = account.CreateCloudTableClient();
            var table = client.GetTableReference("users");
            table.CreateIfNotExists(); // utworzenie tabeli jeżeli nie istnieje
            //table.DeleteIfExists(); // usunięcie tabeli gdy istnieje

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
