using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Npgsql;
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

        /// <summary>
        /// Builds a SELECT * FROM function_name(p_param1 => @Param1, ...) SQL string
        /// for calling PostgreSQL functions via Dapper with CommandType.Text.
        /// Maps C# parameter names (e.g. @UserID) to PostgreSQL naming convention (p_user_id).
        /// </summary>
        private string BuildFunctionCallSql(string functionName, DynamicParameters parameters)
        {
            if (parameters == null || !parameters.ParameterNames.Any())
                return $"SELECT * FROM {functionName}()";

            var paramList = string.Join(", ", parameters.ParameterNames.Select(p => $"{ToSnakeCase(p)} => @{p}"));
            return $"SELECT * FROM {functionName}({paramList})";
        }

        /// <summary>
        /// Converts CamelCase/PascalCase to snake_case with p_ prefix for PostgreSQL parameters.
        /// Example: UserID -> p_user_id, FileName -> p_file_name
        /// </summary>
        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return "p_" + input.ToLower();
        }

        public List<T> SelectCommand_SP<T>(List<T> ObjList, string stored_procedure_name, DynamicParameters parameters)
        {
            try
            {
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjList = SqlCon.Query<T>(sql, parameters, commandType: CommandType.Text).ToList();
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    return SqlCon.Query<T>(sql, parameters, commandType: CommandType.Text).ToList();
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjList = (await SqlCon.QueryAsync<T>(sql, parameters, commandType: CommandType.Text)).ToList();
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjModel = SqlCon.Query<T>(sql, parameters, commandType: CommandType.Text).FirstOrDefault();
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    await SqlCon.OpenAsync();
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjModel = (await SqlCon.QueryAsync<T>(sql,
                                parameters,
                                commandType: CommandType.Text,
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjList = SqlCon.Query<T>(sql, parameters, commandType: CommandType.Text).ToList();
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
            try
            {
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    SqlCon.QueryFirstOrDefault(sql, parameters, commandType: CommandType.Text);
                }
                return 1;
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjList = SqlCon.Query<T>(sql, parameters, commandType: CommandType.Text).FirstOrDefault();
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    await SqlCon.OpenAsync();
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    ObjList = (await SqlCon.QueryAsync<T>(sql, parameters, commandType: CommandType.Text)).FirstOrDefault();
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
            try
            {
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr.ToString()))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    await SqlCon.QueryFirstOrDefaultAsync(sql, parameters, commandType: CommandType.Text);
                }
                return 1;
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
                using (NpgsqlConnection SqlConn = new NpgsqlConnection(constr))
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr))
                {
                    var sql = BuildFunctionCallSql(stored_procedure_name, parameters);
                    await SqlCon.QueryFirstOrDefaultAsync(sql, parameters, commandType: CommandType.Text);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }



        public System.Data.DataTable SelectDataTable(string strQ)
        {
            var dt = new DataTable();
            try
            {
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr))
                {
                    using (NpgsqlCommand sqlComd = new NpgsqlCommand(strQ.ToString(), SqlCon))
                    {
                        using (NpgsqlDataAdapter sqlAdpt = new NpgsqlDataAdapter { SelectCommand = sqlComd })
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
                using (NpgsqlConnection SqlCon = new NpgsqlConnection(constr))
                {
                    using (NpgsqlCommand sqlComd = new NpgsqlCommand(strQ.ToString(), SqlCon))
                    {
                        using (NpgsqlDataAdapter sqlAdpt = new NpgsqlDataAdapter { SelectCommand = sqlComd })
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

            var columnNames = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
            var paramNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            string sql = $"INSERT INTO \"{tableName}\" ({columnNames}) VALUES ({paramNames})";

            using var connection = new NpgsqlConnection(constr);
            return await connection.ExecuteAsync(sql, model);
        }
        //For the purpose return multiple Lists.
        public (T1?, List<T2>) QueryMultiple<T1, T2>(string storedProc,DynamicParameters? param = null)
        {
            using (var connection = new NpgsqlConnection(constr))
            {
                var sql = BuildFunctionCallSql(storedProc, param);
                using (var multi = connection.QueryMultiple(
                    sql,
                    param,
                    commandType: CommandType.Text))
                {
                    var singleModel = multi.ReadFirstOrDefault<T1>();
                    var list = multi.Read<T2>().ToList();
                    return (singleModel, list);
                }
            }
        }
    }
}
