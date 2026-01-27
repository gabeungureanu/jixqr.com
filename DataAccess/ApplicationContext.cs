using Microsoft.EntityFrameworkCore;
using System;

namespace DataAccess
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
            {
                base.OnModelCreating(modelBuilder);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
