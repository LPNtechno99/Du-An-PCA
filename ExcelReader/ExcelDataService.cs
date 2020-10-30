using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement_ExcelData
{
    public class Student
    {
        public int StudentID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Class { get; set; }
        public string Address { get; set; }
    }
    public class ExcelDataService
    {
        OleDbConnection Conn;
        OleDbCommand Cmd;

        public ExcelDataService()
        {
            string ExcelFilePath = @"E:\\TempData.xlsx";
            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ExcelFilePath + ";Extended Properties=Excel 12.0;Persist Security Info=True";
            Conn = new OleDbConnection(excelConnectionString);
        }

        /// <summary>  
        /// Method to Get All the Records from Excel  
        /// </summary>  
        /// <returns></returns>  
        public async Task<ObservableCollection<Student>> ReadRecordFromEXCELAsync()
        {
            ObservableCollection<Student> Students = new ObservableCollection<Student>();
            await Conn.OpenAsync();
            Cmd = new OleDbCommand();
            Cmd.Connection = Conn;
            Cmd.CommandText = "Select * from [Sheet1$]";
            var Reader = await Cmd.ExecuteReaderAsync();
            while (Reader.Read())
            {
                Students.Add(new Student()
                {
                    StudentID = Convert.ToInt32(Reader["StudentID"]),
                    Name = Reader["Name"].ToString(),
                    Email = Reader["Email"].ToString(),
                    Class = Reader["Class"].ToString(),
                    Address = Reader["Address"].ToString()
                });
            }
            Reader.Close();
            Conn.Close();
            return Students;
        }

        /// <summary>  
        /// Method to Insert Record in the Excel  
        /// S1. If the EmpNo =0, then the Operation is Skipped.  
        /// S2. If the Student is already exist, then it is taken for Update  
        /// </summary>  
        /// <param name="Emp"></param>  
        public async Task<bool> ManageExcelRecordsAsync(Student stud)
        {
            bool IsSave = false;
            if (stud.StudentID != 0)
            {
                await Conn.OpenAsync();
                Cmd = new OleDbCommand();
                Cmd.Connection = Conn;
                Cmd.Parameters.AddWithValue("@StudentID", stud.StudentID);
                Cmd.Parameters.AddWithValue("@Name", stud.Name);
                Cmd.Parameters.AddWithValue("@Email", stud.Email);
                Cmd.Parameters.AddWithValue("@Class", stud.Class);
                Cmd.Parameters.AddWithValue("@Address", stud.Address);

                if (!IsStudentRecordExistAsync(stud).Result)
                {
                    Cmd.CommandText = "Insert into [Sheet1$] values (@StudentID,@Name,@Email,@Class,@Address)";
                }
                else
                {
                    Cmd.CommandText = "Update [Sheet1$] set StudentID=@StudentID,Name=@Name,Email=@Email,Class=@Class,Address=@Address where StudentID=@StudentID";

                }
                int result = await Cmd.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    IsSave = true;
                }
                Conn.Close();
            }
            return IsSave;

        }
        /// <summary>  
        /// The method to check if the record is already available   
        /// in the workgroup  
        /// </summary>  
        /// <param name="emp"></param>  
        /// <returns></returns>  
        private async Task<bool> IsStudentRecordExistAsync(Student stud)
        {
            bool IsRecordExist = false;
            Cmd.CommandText = "Select * from [Sheet1$] where StudentId=@StudentID";
            var Reader = await Cmd.ExecuteReaderAsync();
            if (Reader.HasRows)
            {
                IsRecordExist = true;
            }

            Reader.Close();
            return IsRecordExist;
        }
    }
}