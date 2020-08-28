using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SQLHelper.Abstractions
{
    public class Insert : IQuery
    {

        public Insert(BaseSQLHelper SQLH, string TableName) :base(SQLH, TableName)
        {

        }

 
        public IQuery AddField(string Name, object Value)
        {
            this.Parameters.Add(Name, Value);
            return this;
        }


        public override int Execute()
        {
            throw new NotImplementedException();
        }

        protected override string BuildQuery()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
