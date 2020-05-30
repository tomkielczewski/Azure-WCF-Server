﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole1
{
    public class User : TableEntity
    {
        public User(string rKey, string pKey)
        {
            this.PartitionKey = pKey;
            this.RowKey = rKey;

        }
        public User()
        {
        }
        public string Login { get; set; }
        public string Password { get; set; }
        public Guid SessionId { get; set; }

    }
}