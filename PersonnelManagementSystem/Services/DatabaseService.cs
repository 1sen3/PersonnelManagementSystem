using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.UI.Xaml.Controls;
using MySql.Data.MySqlClient;
using PersonnelManagementSystem.Models;
using Windows.Data.Text;

namespace PersonnelManagementSystem.Services
{
    public class DatabaseService
    {
        private static readonly string connectionString = "Server=localhost;Database=shuyisen_kcsj;User ID=root;Password=123456";
        public DatabaseService() {}
        // 获取所有员工信息
        public static async Task<List<Staff>> GetStaffListAsync()
        {
            var Staffs = new List<Staff>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("select * from view_all_staff", connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Staffs.Add(new Staff {
                            ID = reader.GetString("sID"),
                            Password = reader.GetString("sPassword"),
                            Name = reader.GetString("sName"),
                            Sex = reader.GetString("sSex"),
                            Birthday = reader.GetDateTime("sBirthday").ToString("yyyy-MM-dd"),
                            Department = reader.GetString("dName"),
                            Job = reader.GetString("jDescription"),
                            Education = reader.GetString("eDescription"),
                            Specialty = reader.GetString("sSpecialty"),
                            Address = reader.GetString("sAddress"),
                            Telephone = reader.GetString("sTel"),
                            Email = reader.GetString("sEmail"),
                            State = reader.GetString("stateDescription")
                        });
                    }
                }
                return Staffs;
            }
            catch(Exception ex)
            {
                throw new Exception("获取员工数据失败", ex);
            }
        }
        // 获取所有变更记录信息
        public static async Task<List<Personnel>> GetPersonnelListAsync()
        {
            var personnels = new List<Personnel>();
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                var command = new MySqlCommand("select p.pID,p.sID,s.sName,j.jDescription,state.stateDescription,c.cDescription,p.pDescription from shuyisen_personnel p join shuyisen_staff s on s.sID = p.sID join shuyisen_job j on j.jCode = s.jCode join shuyisen_staff_state state on s.stateCode = state.stateCode join shuyisen_change c on p.cCode = c.cCode order by p.pID asc", connection);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    personnels.Add(new Personnel
                    {
                        PersonnelID = reader.GetInt32("pID"),
                        StaffID = reader.GetString("sID"),
                        StaffName = reader.GetString("sName"),
                        StaffJob = reader.GetString("jDescription"),
                        StaffState = reader.GetString("stateDescription"),
                        Operation = reader.GetString("cDescription"),
                        Reason = reader.GetString("pDescription")
                    });
                }
                return personnels;
            }
            catch(Exception ex)
            {
                throw new Exception("获取变更记录失败", ex);
            }
        }
        // 获取所有变更操作
        public static async Task<List<String>> GetChangeListAsync()
        {
            var Changes = new List<string>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("select cDescription from shuyisen_kcsj.shuyisen_change where cDescription != '入职' order by cCode asc", connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Changes.Add(reader.GetString("cDescription"));
                    }
                }
                return Changes;
            }
            catch(Exception ex)
            {
                throw new Exception("获取变更操作信息失败",  ex);
            }
        }
        // 获取所有职务
        public static async Task<List<string>> GetJobListAsync()
        {
            var Jobs = new List<string>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("select jDescription from shuyisen_job order by jCode asc", connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Jobs.Add(reader.GetString("jDescription"));
                    }
                }
                return Jobs;
            }
            catch(Exception ex)
            {
                throw new Exception("获取职务信息失败", ex);
            }
        }
        // 获取所有教育程度
        public static async Task<List<string>> GetEduListAsync()
        {
            var Edus = new List<string>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("select eDescription from shuyisen_edu order by eCode asc", connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Edus.Add(reader.GetString("eDescription"));
                    }
                }
                return Edus;
            }
            catch(Exception ex)
            {
                throw new Exception("获取教育程度信息失败", ex);
            }
        }
        // 获取所有部门
        public static async Task<List<string>> GetDepartmentListAsync()
        {
            var Departments = new List<string>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("select dName from shuyisen_department order by dID asc", connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Departments.Add(reader.GetString("dName"));
                    }
                }
                return Departments;
            }
            catch(Exception ex)
            {
                throw new Exception("获取部门信息失败", ex);
            }
        }
        // 员工调动
        public static async Task<bool> ChangeStaffAsync(string sID, string operation, string newDept,string newJob,string reason)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                if ("职务变动".Equals(operation))
                {
                    command.CommandText = "update shuyisen_staff set jCode = (select jCode from shuyisen_job where jDescription = @newJob) where sID = @sID";
                    command.Parameters.AddWithValue("@newJob", newJob);
                    command.Parameters.AddWithValue("@sID", sID);
                }
                else if ("调岗".Equals(operation))
                {
                    command.CommandText = "CALL proc_staff_transfer(@sID, (SELECT dID FROM shuyisen_department WHERE dName = @newDept), " +
                          "(SELECT jCode FROM shuyisen_job WHERE jDescription = @newJob), @reason)";
                    command.Parameters.AddWithValue("@sID", sID);
                    command.Parameters.AddWithValue("@newDept", newDept);
                    command.Parameters.AddWithValue("@newJob", newJob);
                    command.Parameters.AddWithValue("@reason", reason);
                }
                else if ("辞退".Equals(operation))
                {
                    command.CommandText = "call proc_staff_leave(@sID,@reason)";
                    command.Parameters.AddWithValue("@sID", sID);
                    command.Parameters.AddWithValue("@reason", reason);
                }
                else if ("休假".Equals(operation))
                {
                    command.CommandText = "call proc_staff_vacation(@sID,@reason)";
                    command.Parameters.AddWithValue("@sID", sID);
                    command.Parameters.AddWithValue("@reason", reason);
                }
                else if ("返岗".Equals(operation))
                {
                    command.CommandText = "call proc_staff_rework(@sID,@reason)";
                    command.Parameters.AddWithValue("@sID", sID);
                    command.Parameters.AddWithValue("@reason", reason);
                }
                await command.ExecuteNonQueryAsync();

                return true;
            }
            catch(Exception ex)
            {
                throw new Exception("添加人事变更记录出错", ex);
            }
        }
        // 查询员工是否存在
        public static async Task<bool> CheckStaffExistAsync(string sID)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "select count(*) from shuyisen_staff where sID = @sID";
                command.Parameters.AddWithValue("@sID", sID);
                var result = await command.ExecuteScalarAsync();
                int count = Convert.ToInt32(result);

                return count >= 1;
            }
            catch(Exception ex)
            {
                throw new Exception("查询员工是否存在失败"+ex.Message, ex);
            }
        }
        // 验证登录
        public static async Task<Staff> LoginAsync(string id, string password)
        {
            Staff user = null;
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    command.CommandText = "select * from view_all_staff where sID = @id and sPassword = @password";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@password", password);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        user = new Staff
                        {
                            ID = id,
                            Password = password,
                            Name = reader.GetString("sName"),
                            Sex = reader.GetString("sSex"),
                            Birthday = reader.GetDateTime("sBirthday").ToString("yyyy-MM-dd"),
                            Department = reader.GetString("dName"),
                            Job = reader.GetString("jDescription"),
                            Education = reader.GetString("eDescription"),
                            Specialty = reader.GetString("sSpecialty"),
                            Address = reader.GetString("sAddress"),
                            Telephone = reader.GetString("sTel"),
                            Email = reader.GetString("sEmail"),
                            State = reader.GetString("stateDescription"),
                            Authority = reader.GetString("dName") == "人事部" ? "管理员" : "普通员工"
                        };
                        break;
                    }
                }
                return user;
            }
            catch(Exception ex)
            {
                throw new Exception("登录失败", ex);
            }
        }
        // 获取员工数量
        public static async Task<int> GetStaffCountAsync()
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            var command = new MySqlCommand("select count(*) from shuyisen_staff", connection);
            var result = await command.ExecuteScalarAsync();
            int cnt = Convert.ToInt32(result); // 这样写更安全
            return cnt;
        }
        // 获取员工工号与姓名信息
        public static async Task<List<string>> GetStaffIdAndNameAsync()
        {
            List<string> result = [];
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                var command = new MySqlCommand("select sID,sName from view_all_staff", connection);
                var reader = await command.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    string tmp = reader.GetString("sID") + " - " + reader.GetString("sName");
                    result.Add(tmp);
                }
                return result;
            }
            catch(Exception ex)
            {
                throw new Exception("获取员工工号与姓名信息失败", ex);
            }
        }
        // 录入新员工信息
        public static async Task AddNewStaffAsync(Staff NewStaff)
        {
            int StaffCount = await GetStaffCountAsync();
            NewStaff.ID = (100000 + StaffCount + 1).ToString();
            NewStaff.Password = "123456";
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "call proc_add_staff(@sID,@sPassword,@sName,@sSex,@sBirthday,@dName,@jDescription,@eDescription,@sSpecialty,@sAddress,@sTel,@sEmail,@sAutority)";
                command.Parameters.AddWithValue("@sID", NewStaff.ID);
                command.Parameters.AddWithValue("@sPassword", NewStaff.Password);
                command.Parameters.AddWithValue("@sName", NewStaff.Name);
                command.Parameters.AddWithValue("@sSex", NewStaff.Sex);
                command.Parameters.AddWithValue("@sBirthday", NewStaff.Birthday);
                command.Parameters.AddWithValue("@dName", NewStaff.Department);
                command.Parameters.AddWithValue("@jDescription", NewStaff.Job);
                command.Parameters.AddWithValue("@eDescription", NewStaff.Education);
                command.Parameters.AddWithValue("@sSpecialty", NewStaff.Specialty);
                command.Parameters.AddWithValue("@sAddress", NewStaff.Address);
                command.Parameters.AddWithValue("@sTel", NewStaff.Telephone);
                command.Parameters.AddWithValue("@sEmail", NewStaff.Email);
                command.Parameters.AddWithValue("@sAutority", NewStaff.Department == "人事部" ? "管理员" : "员工");

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("录入新员工信息失败" + ex.Message);
                throw new Exception("录入新员工信息失败", ex);
            }
        }
        // 通过工号获取员工信息
        public static async Task<Staff> GetStaffInfoByIDAsync(string id)
        {
            Staff StaffInfo = null;
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "select * from view_all_staff where sID = @id";
            command.Parameters.AddWithValue("@id", id);
            var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                StaffInfo = new Staff
                {
                    ID = reader.GetString("sID"),
                    Password = reader.GetString("sPassword"),
                    Name = reader.GetString("sName"),
                    Sex = reader.GetString("sSex"),
                    Birthday = reader.GetDateTime("sBirthday").ToString("yyyy-MM-dd"),
                    Department = reader.GetString("dName"),
                    Job = reader.GetString("jDescription"),
                    Education = reader.GetString("eDescription"),
                    Specialty = reader.GetString("sSpecialty"),
                    Address = reader.GetString("sAddress"),
                    Telephone = reader.GetString("sTel"),
                    Email = reader.GetString("sEmail"),
                    State = reader.GetString("stateDescription")
                };   
            }
            return StaffInfo;
        }
        // 修改员工信息
        public static async Task EditStaffInfoAsync(Staff staff)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "call proc_edit_staff(@id,@name,@sex,@birthday,@eDescription,@specialty,@address,@tel,@email)";
            command.Parameters.AddWithValue("@id", staff.ID);
            command.Parameters.AddWithValue("@name", staff.Name);
            command.Parameters.AddWithValue("@sex", staff.Sex);
            command.Parameters.AddWithValue("@birthday", staff.Birthday);
            command.Parameters.AddWithValue("@eDescription", staff.Education);
            command.Parameters.AddWithValue("specialty", staff.Specialty);
            command.Parameters.AddWithValue("@address", staff.Address);
            command.Parameters.AddWithValue("@tel", staff.Telephone);
            command.Parameters.AddWithValue("@email", staff.Email);

            await command.ExecuteNonQueryAsync();
        }
        // 修改密码
        public static async Task<bool> EditPasswordAsync(string ID,string NewPassword,string VerifInfo,string Type)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            var commandFind = connection.CreateCommand();
            if(Type == "手机号")
            {
                commandFind.CommandText = "select count(*) from shuyisen_staff where sID = @ID and sTel = @VerifInfo";
            }
            else if (Type == "邮箱")
            {
                commandFind.CommandText = "select count(*) from shuyisen_staff where sID = @ID and sEmail = @VerifInfo";
            }
            commandFind.Parameters.AddWithValue("@ID", ID);
            commandFind.Parameters.AddWithValue("@VerifInfo", VerifInfo);
            bool valid = (Int64)await commandFind.ExecuteScalarAsync() == 1 ? true : false;
            if (valid)
            {
                var commndUpdate = connection.CreateCommand();
                if (Type == "手机号")
                {
                    commndUpdate.CommandText = "update shuyisen_staff set sPassword = @NewPassword where sID = @ID and sTel = @VerifInfo";
                }
                else if(Type == "邮箱")
                {
                    commndUpdate.CommandText = "update shuyisen_staff set sPassword = @NewPassword where sID = @ID and sEmail = @VerifInfo";
                }
                commndUpdate.Parameters.AddWithValue("@NewPassword", NewPassword);
                commndUpdate.Parameters.AddWithValue("@ID", ID);
                commndUpdate.Parameters.AddWithValue("@VerifInfo", VerifInfo);
                await commndUpdate.ExecuteNonQueryAsync();
                return true;
            }
            else return false;
        }
    }
}
