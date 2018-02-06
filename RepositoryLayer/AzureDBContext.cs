using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Infrastructure;
using CoreEntities.Domain;

namespace RepositoryLayer
{
    public class AzureDbContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public AzureDbContext()
            : base("name=AzureDBContext")
        {
            //((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 1800;
        }


        /// <summary>
        /// To enable auto migration
        /// </summary>
        /// <param name="builder"></param>
        //protected override void OnModelCreating(DbModelBuilder builder)
        //{
        //    Database.SetInitializer(new MigrateDatabaseToLatestVersion<AzureDbContext, Migrations.Configuration>());
        //}

        ///Register Classes
        public DbSet<UserMembership> UserMembership { get; set; }
        public DbSet<AuthenticationDetail> AuthenticationDetail { get; set; }
      
        public DbSet<AuthenticationToken> AuthenticationToken { get; set; }
        public DbSet<EmailTemplets> EmailTemplets { get; set; }
        public DbSet<EmailDetails> EmailDetails { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            var context = ((IObjectContextAdapter)(this)).ObjectContext;

            var connection = Database.Connection;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 1000;

                if (parameters != null)
                    foreach (var p in parameters)
                    {
                        if (p != null)
                            cmd.Parameters.Add(p);
                    }
                IList<TEntity> result;

                using (var reader = cmd.ExecuteReader())
                {
                    result = context.Translate<TEntity>(reader).ToList();
                    for (int i = 0; i < result.Count; i++)
                        result[i] = AttachEntityToContext(result[i]);
                }

                return result;
            }
        }
        protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
        {
            //little hack here until Entity Framework really supports stored procedures
            //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
            //var alreadyAttached = Set<TEntity>().Local.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (entity.Id > 0)
            {
                var alreadyAttached = Set<TEntity>().Local.Where(x => x.Id == entity.Id).FirstOrDefault();
                if (alreadyAttached == null)
                {
                    //attach new entity
                    Set<TEntity>().Attach(entity);
                    return entity;
                }
                else
                {
                    //entity is already loaded.
                    return alreadyAttached;
                }
            }
            else
            {
                return entity;
            }
        }

        public int ExecuteStoredProcedureNonQuery(string commandText, params object[] parameters)
        {
            //var connection = context.Connection;
            var connection = Database.Connection;
            //Don't close the connection after command execution

            //open the connection for use
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            //create a command object
            using (var cmd = connection.CreateCommand())
            {
                //command to execute
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.StoredProcedure;

                // move parameters to command object
                if (parameters != null)
                    foreach (var p in parameters)
                    {
                        if (p != null)
                            cmd.Parameters.Add(p);
                    }

                var rowseffected = cmd.ExecuteNonQuery();


                //this.Database.ExecuteSqlCommand(EXEC " + storeProcName + " @Paramtable";)

                //var rowseffected = cmd.ExecuteScalar();
                return int.Parse(rowseffected.ToString());
            }
        }
        public static bool GetLogInfoStatus(string moduleName)
        {
            bool returnVal = false;
            var conn = new SqlConnection();
            var sCmd = new SqlCommand
            {
                CommandText = "GetLogs",
                CommandTimeout = 600,
                CommandType = CommandType.StoredProcedure
            };
            sCmd.Parameters.AddWithValue("ModuleName", moduleName);
            SqlDataReader drResults = null;
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    sCmd.Connection = conn;
                    drResults = sCmd.ExecuteReader(CommandBehavior.CloseConnection);
                    sCmd.Dispose();

                    if (drResults.HasRows)
                    {
                        while (drResults.Read())
                        {
                            int rowsAffected = (drResults["RowsAffected"] != DBNull.Value) ? int.Parse(drResults["RowsAffected"].ToString()) : 0;
                            if (rowsAffected > 0)
                                returnVal = true;
                        }

                        drResults.Close();
                    }
                    drResults = null;
                }
                else
                {
                    throw new Exception("Database connection could not be established.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading Recurly GetLogs from [Framework].[dbo].[GetLogs].", ex);
            }
            finally
            {
                if (drResults != null && !drResults.IsClosed)
                {
                    drResults.Close();
                    drResults.Dispose();
                }

                if (conn.State != ConnectionState.Closed)
                    conn.Close();

            }
            return returnVal;
        }
    }
}
