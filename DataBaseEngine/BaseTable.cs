using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseTable
{
    public class TableMetaInf
    {

    }

    public class TableData
    {

    }

    public class Table
    {
        public Table(TableData tableData, TableMetaInf tableMetaInf)
        {
            TableData = tableData ?? throw new ArgumentNullException(nameof(tableData));
            TableMetaInf = tableMetaInf ?? throw new ArgumentNullException(nameof(tableMetaInf));
        }

        public TableData TableData { get; set; }
        public TableMetaInf TableMetaInf { get; set; }
    }
}
