using System;
using System.Diagnostics;
using System.Text;
using DataBaseEngine;
using IntegrationTests.TestApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SunflowerDB;
using TransactionManagement;




namespace IntegrationTests
{
    [TestClass]
    class Insert_Delete : BaseSQLTest
    {
        public Insert_Delete (bool fixtests) : base(fixtests)
        {
        }

        public Insert_Delete () : base(false)
        {
        }

        [TestMethod]
        public void InsertTest ()
        {
            DelFiles();
            var _core = new DataBase(20, new DataBaseEngineMain(_testPath), new TransactionScheduler());
            var expected = GetTestData();
            var cl1 = new TestClient("cl1", _core);
            var cl2 = new TestClient("cl1", _core);


            SendSQLQuery(cl1, $"CREATE TABLE UnitMeasure (Name CHAR(50),UnitMeasureCode CHAR(50), ModifiedDate INT);", expected);
            SendSQLQuery(cl1, $"CREATE TABLE UnitMeasure2 (Name CHAR(50),UnitMeasureCode CHAR(50), ModifiedDate INT);", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (Name, UnitMeasureCode, ModifiedDate) VALUES ('FT2', 'Square Feet ', 20080923), ('Y', 'Yards', '20080923'), ('Y3', 'Cubic Yards', '20080923');", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (UnitMeasureCode, Name, ModifiedDate) VALUES ('Square Feet ', 'FT2', '20080923'), ('Yards', 'Y', '20080923'), ('Cubic Yards', 'Y3', '20080923');", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (UnitMeasureCode, Name, ModifiedDate) VALUES ('Square Feet ', 'FT2', '20080923'), ('Yards', 'Y', '20080923'), ('Cubic Yards', 'Y3', '20080923');", expected);
            
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (Name, UnitMeasureCode, ModifiedDate) VALUES ('FT2', 'Square Feet ', 2+2*10), ('Y', 'Yards', (2+2)*10), ('Y3', 'Cubic Yards', 22+2);", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (Name, UnitMeasureCode, ModifiedDate) VALUES ('FT2', 'Square Feet ', 12), ('Y', 'Yards', 11), ('Y3', 'Cubic Yards', 22/0);", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure(Name, UnitMeasureCode, ModifiedDate) VALUES('FT2', 'Square Feet ', 55), ('Y', 'Yards', 55), ('Y3', 'Cubic Yards', 55); ", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure2 (Name, UnitMeasureCode, ModifiedDate) VALUES ('FT2', 'Square Feet ', 77), ('Y', 'Yards', 77), ('Y3', 'Cubic Yards', 77);", expected);

            SendSQLQuery(cl1, $"DELETE FROM UnitMeasure WHERE ModifiedDate = 40", expected);
            SendSQLQuery(cl1, $"DELETE FROM UnitMeasure WHERE ModifiedDate = 20 + 2", expected);
            SendSQLQuery(cl1, $"DELETE FROM UnitMeasure WHERE RandomVar = 20 + 2", expected);
            SendSQLQuery(cl1, $"DELETE FROM UnitMeasure WHERE ModifiedDate = (ModifiedDate - ModifiedDate) + 20080923", expected);
            SendSQLQuery(cl1, $"DELETE FROM UnitMeasure WHERE ModifiedDate = 40 / 0", expected);

            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (Name, UnitMeasureCode, ModifiedDate) VALUES ('FT2', 'Square Feet ', 2+2*10), ('Y', 'Yards', (2+2)*10), ('Y3', 'Cubic Yards', 22+2);", expected);
            SendSQLQuery(cl1, $"UPDATE UnitMeasure SET ModifiedDate = ModifiedDate*2 WHERE ModifiedDate = 22;", expected);
            SendSQLQuery(cl1, $"SELECT Name FROM UnitMeasure WHERE ModifiedDate = 22", expected);
            SendSQLQuery(cl1, $"SELECT Name FROM UnitMeasure WHERE ModifiedDate < 22", expected);
            SendSQLQuery(cl1, $"SELECT Name FROM UnitMeasure WHERE ModifiedDate > 22", expected);
            SendSQLQuery(cl1, $"SELECT UnitMeasure.Name, UnitMeasure.ModifiedDate FROM (UnitMeasure INNER JOIN UnitMeasure2 ON UnitMeasure.name = UnitMeasure2.name) WHERE ModifiedDate = 22", expected);
            
            
            /*
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (UnitMeasureCode, Name, ModifiedDate) VALUES ('Square Feet ', 'FT2', '20080923'), ('Yards', 'Y', '20080923'), ('Cubic Yards', 'Y3', '20080923');;", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (UnitMeasureCode, Name, ModifiedDate) VALUES ('Square Feet ', 'FT2', '20080923'), ('Yards', 'Y', '20080923'), ('Cubic Yards', 'Y3', '20080923');;", expected);
            SendSQLQuery(cl1, $"INSERT INTO UnitMeasure (UnitMeasureCode, Name, ModifiedDate) VALUES ('Square Feet ', 'FT2', '20080923'), ('Yards', 'Y', '20080923'), ('Cubic Yards', 'Y3', '20080923');;", expected);
            */

        }
    }
}
