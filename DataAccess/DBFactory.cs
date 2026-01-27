using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
//using DataTypes.ModelDataTypes;

namespace DataAccess
{
    public class DbFactory
    {
        public string constr { get; set; }
        private IDbConnection con;
        public DbFactory()
        {
            constr = DBConnection.Main();
        }

        public List<T> SelectCommand_SP<T>(List<T> ObjList, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjList = SqlCon.Query<T>(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<T> SelectCommand_SP<T>(string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    return SqlCon.Query<T>(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<T>> SelectCommand_SP_List_Async<T>(List<T> ObjList, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjList = (await SqlCon.QueryAsync<T>(stored_procedure_name, parameters, commandType: CommandType.StoredProcedure)).ToList();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T SelectCommand_SP<T>(T ObjModel, string sqlQuery)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    if (SqlCon.State == ConnectionState.Closed)
                        SqlCon.Open();

                    ObjModel = SqlCon.Query<T>(sqlQuery, commandType: CommandType.Text).FirstOrDefault();
                    return ObjModel;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> SelectCommand_SP<T>(List<T> ObjList, string strQ)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    if (SqlCon.State == ConnectionState.Closed)
                        SqlCon.Open();

                    ObjList = SqlCon.Query<T>(strQ, commandType: CommandType.Text).ToList();
                    return ObjList;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T SelectCommand_Single_SP<T>(T ObjList, string strQ)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjList = SqlCon.Query<T>(strQ, commandType: CommandType.Text).FirstOrDefault();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> SelectCommand<T>(List<T> ObjList, string Query)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjList = SqlCon.Query<T>(Query.ToString()).ToList();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T SelectCommand_SP<T>(T ObjModel, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjModel = SqlCon.Query<T>(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    return ObjModel;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<T> SelectCommand_SPAsync<T>(T ObjModel, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    await SqlCon.OpenAsync(); // Open the database connection asynchronously
                    // Execute the query asynchronously and return the result
                    ObjModel = (await SqlCon.QueryAsync<T>(stored_procedure_name.ToString(),
                                parameters,
                                commandType: CommandType.StoredProcedure,
                                commandTimeout: 300)).FirstOrDefault();
                    return ObjModel;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IList<T> SelectCommand_SP<T>(IList<T> ObjList, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjList = SqlCon.Query<T>(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertCommand_SPExecute(string stored_procedure_name, DynamicParameters parameters)
        {
            int rowAffected = 0;
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    rowAffected = SqlCon.Execute(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure);
                }
                return rowAffected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T InsertCommand_SPQuery<T>(T ObjList, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    ObjList = SqlCon.Query<T>(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<T> InsertCommand_SPQueryAsync<T>(T ObjList, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    await SqlCon.OpenAsync(); // Open the database connection asynchronously
                    ObjList = (await SqlCon.QueryAsync<T>(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
                }
                return ObjList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateCommand_SPAsync(string stored_procedure_name, DynamicParameters parameters)
        {
            int rowAffected = 0;
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr.ToString()))
                {
                    rowAffected = await SqlCon.ExecuteAsync(stored_procedure_name.ToString(), parameters, commandType: CommandType.StoredProcedure);
                }
                return rowAffected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateCommand_SP(string strQ)
        {
            int rowAffected = 0;
            try
            {
                using (SqlConnection SqlConn = new SqlConnection(constr))
                {
                    rowAffected = SqlConn.Execute(strQ, commandType: CommandType.Text);
                }
                return rowAffected;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateCommand_SPAsync_Bool(string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr))
                {
                    int rowAffected = await SqlCon.ExecuteAsync(
                        stored_procedure_name,
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return rowAffected > 0;
                }
            }
            catch (Exception)
            {
                // Optional: log exception
                return false; // or rethrow if preferred
            }
        }



        public System.Data.DataTable SelectDataTable(string strQ)
        {
            var dt = new DataTable();
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr))
                {
                    using (SqlCommand sqlComd = new SqlCommand(strQ.ToString(), SqlCon))
                    {
                        using (SqlDataAdapter sqlAdpt = new SqlDataAdapter { SelectCommand = sqlComd })
                        {
                            dt = new DataTable();
                            sqlAdpt.Fill(dt);
                        }
                    }
                }
                return dt;
            }
            catch (Exception)
            {
                dt = null;
                return dt;
            }
        }

        public System.Data.DataSet SelectDataSet(string strQ)
        {
            var ds = new DataSet();
            try
            {
                using (SqlConnection SqlCon = new SqlConnection(constr))
                {
                    using (SqlCommand sqlComd = new SqlCommand(strQ.ToString(), SqlCon))
                    {
                        using (SqlDataAdapter sqlAdpt = new SqlDataAdapter { SelectCommand = sqlComd })
                        {
                            ds = new DataSet();
                            sqlAdpt.Fill(ds);

                        }
                    }
                }
                return ds;
            }
            catch (Exception)
            {
                ds = null;
                return ds;
            }
        }

        public async Task<int> InsertAsync<T>(string tableName, T model)
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            var columnNames = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
            var paramNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            string sql = $"INSERT INTO [{tableName}] ({columnNames}) VALUES ({paramNames})";

            using var connection = new SqlConnection(constr);
            return await connection.ExecuteAsync(sql, model);
        }
        //For the purpose return multiple Lists.
        public (T1?, List<T2>) QueryMultiple<T1, T2>(string storedProc,DynamicParameters? param = null)
        {
            using (var connection = new SqlConnection(constr))
            {
                using (var multi = connection.QueryMultiple(
                    storedProc,
                    param,
                    commandType: CommandType.StoredProcedure))
                {
                    var singleModel = multi.ReadFirstOrDefault<T1>();
                    var list = multi.Read<T2>().ToList();
                    return (singleModel, list);
                }
            }
        }
    }
}





