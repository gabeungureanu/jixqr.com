using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataAccess.IDataAccess
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T Fetch_By_Id(long id);
        T Fetch_SingleString(string Name);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Remove(T entity);
        Int32 SaveChanges();

        #region function for Multiple entry using Stored Procedure
        string Execute_Multiple_Query(List<string> lstquery);
        System.Data.DataSet SelectCommand(string strQ);

        string SelectString(string strQ);

        Int32 UpdateCommand(string strQ);

        // Sorting Datatable according to the Column
        System.Data.DataTable SortTable(DataTable dt, string columnname);

        System.Data.DataTable SelectDataTable(string strQ);

        Int32 SelectCount(string strQ);

        Int32 InsertCommand(string strQ);

        Int32 DeleteCommand(string strQ);

        Int32 InsertCommandReturnIdentity(string strQ);

        #endregion
    }
}
