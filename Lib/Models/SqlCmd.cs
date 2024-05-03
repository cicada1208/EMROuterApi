using System.Text;

namespace Lib.Models
{
    public class SqlCmd
    {
        /// <summary>
        /// SQL StringBuilder
        /// </summary>
        public StringBuilder Builder { get; }

        /// <summary>
        /// SQL Command Text
        /// </summary>
        public string Sql => Builder.ToString().TrimEnd();

        /// <summary>
        /// SQL Command Parameters
        /// </summary>
        public object? Param { get; set; }

        ///// <summary>
        ///// SQL Command Parameters
        ///// </summary>
        //public DynamicParameters Params { get; set; } = new DynamicParameters();

        public SqlCmd()
        {
            Builder = new StringBuilder();
        }

    }
}
