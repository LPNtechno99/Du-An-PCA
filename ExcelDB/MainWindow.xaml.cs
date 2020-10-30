using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;


namespace ExcelDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OleDbDataAdapter dataAdapter;
        private MySqlDataAdapter mySqlDBAdapter;
        private DataTable db;
        private DataTable dbExcel;
        private DataSet ds;
        private IAsyncResult result;
        private DataTable db1;
        MySqlCommandBuilder cb;

        public MainWindow()
        {
            InitializeComponent();
            MySqlConnection conn = DBMySQLUtils.GetDBConnection();
            conn.Open();

            // REad data From Mysql - Student Table

            string sql = "SELECT * FROM student";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            mySqlDBAdapter = new MySqlDataAdapter(cmd);

            db = new DataTable();
            mySqlDBAdapter.Fill(db);

            dbMain.ItemsSource = db.DefaultView;

            // Test REader

            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read()) Console.WriteLine(rdr[0] + "--" + rdr[1]);

            rdr.Close();

            // Test Insert

            sql = "INSERT INTO student(full_name) VALUES ('Ho va ten 1')";
            cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Test Count

            sql = "SELECT COUNT(*) FROM student WHERE full_name LIKE '%1'";
            cmd = new MySqlCommand(sql, conn);
            Object tempResult = cmd.ExecuteScalar();
            if (tempResult != null) MessageBox.Show("Result = " + (tempResult).ToString());

            // Test Update 
            //cb = new MySqlCommandBuilder(mySqlDBAdapter);

            // Test Parameters

            sql = "SELECT * FROM student WHERE full_name LIKE @CONDITIONEDIT";

            cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("CONDITIONEDIT", "%");

            mySqlDBAdapter = new MySqlDataAdapter(cmd);

            db = new DataTable();
            mySqlDBAdapter.Fill(db);
            cb = new MySqlCommandBuilder(mySqlDBAdapter);

            dbMain.ItemsSource = db.DefaultView;

            db1 = db;

            // Test Insert

            sql = "ALTER TABLE student AUTO_INCREMENT = 0";
            cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();

            //// Delete All Data
            //sql = "DELETE FROM student";
            //cmd = new MySqlCommand(sql, conn);
            //cmd.ExecuteNonQuery();
        }

        public OleDbConnection Conn { get; private set; }
        public OleDbCommand Cmd { get; private set; }

        private void showData_buttonClick(object sender, RoutedEventArgs e)
        {
            string ExcelFilePath = @"E:\\excel_DB_students.xlsx";
            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ExcelFilePath + ";Extended Properties=Excel 12.0;Persist Security Info=True";

            Conn = new OleDbConnection(excelConnectionString);

            Cmd = new OleDbCommand();
            Cmd.Connection = Conn;
            Cmd.CommandText = "Select * from [Sheet1$]";

            dataAdapter = new OleDbDataAdapter();
            dataAdapter.SelectCommand = Cmd;
            
            ds = new DataSet();
            dbExcel = new DataTable();

            dataAdapter.Fill(dbExcel);
            dbMain.ItemsSource = dbExcel.DefaultView;

            Conn.Open();

            DataTable dtTablesList = Conn.GetSchema("Tables");
            for (int i = 0; i < dtTablesList.Rows.Count; i++)
            {
                Console.WriteLine(dtTablesList.Rows[i]["TABLE_NAME"].ToString());
            }

            Conn.Close();
        }

        private void dbMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void updateData_Click(object sender, RoutedEventArgs e)
        {
            cb = new MySqlCommandBuilder(mySqlDBAdapter);

            //cb.GetDeleteCommand();
            foreach (DataRow item in db.Rows)
            {
                item.Delete();
            }

            foreach (DataRow item in dbExcel.Rows)
            {
                db.Rows.Add(item.ItemArray);
            }
            cb.GetInsertCommand();
            mySqlDBAdapter.Update(db);
            db.AcceptChanges();
        }
    }
}
