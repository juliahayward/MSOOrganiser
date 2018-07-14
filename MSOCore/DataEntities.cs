using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOCore
{
    public partial class DataEntities : DbContext
    {
        // Alternative constructor passing name
        public DataEntities(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        } 
    }

    public static class DataEntitiesProvider
    {
        public static bool IsProduction { get; set; }

        public static DataEntities Provide()
        {
            return new DataEntities("name=" + ((IsProduction) ? "Production" : "DataEntities"));
        }

        public static string Description()
        {
            return (IsProduction) ? "" : "TEST DATA";
        }
    }
}
