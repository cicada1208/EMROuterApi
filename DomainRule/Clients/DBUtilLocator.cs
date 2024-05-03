using DomainRule.Consts;
using Lib.Utilities;

namespace DomainRule.Clients
{
    public class DBUtilLocator
    {
        private DBUtil? _NIS;
        public DBUtil NIS =>
            _NIS ??= new DBUtil(DBName.NIS, DBType.SYBASE);

        private DBUtil? _SYB1;
        public DBUtil SYB1 =>
            _SYB1 ??= new DBUtil(DBName.SYB1, DBType.SYBASE);

        private DBUtil? _SYB2;
        public DBUtil SYB2 =>
            _SYB2 ??= new DBUtil(DBName.SYB2, DBType.SYBASE);

        private DBUtil? _UAAC;
        public DBUtil UAAC =>
            _UAAC ??= new DBUtil(DBName.UAAC, DBType.SQLSERVER);

        private DBUtil? _EMRDB;
        public DBUtil EMRDB =>
            _EMRDB ??= new DBUtil(DBName.EMRDB, DBType.SQLSERVER);

        private DBUtil? _EMRDB_1522011080;
        public DBUtil EMRDB_1522011080 =>
            _EMRDB_1522011080 ??= new DBUtil(DBName.EMRDB_1522011080, DBType.SQLSERVER);

        private DBUtil? _PeriExam;
        public DBUtil PeriExam =>
            _PeriExam ??= new DBUtil(DBName.PeriExam, DBType.SQLSERVER);

        private DBUtil? _MISSYS;
        public DBUtil MISSYS =>
            _MISSYS ??= new DBUtil(DBName.MISSYS, DBType.SQLSERVER);

    }
}
