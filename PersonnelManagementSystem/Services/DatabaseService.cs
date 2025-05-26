using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task<List<Staff>> GetStaffListAsync()
        {
            var Staffs = new List<Staff>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new MySqlCommand("select s.sID,s.sPassword,s.sName,s.sSex,s.sBirthday,d.dName,j.jDescription,\r\ne.eDescription,s.sSpecialty,s.sAddress,s.sTel,s.sEmail,state.stateDescription\r\n from shuyisen_staff s \r\njoin shuyisen_department d on s.dID = d.dID \r\njoin shuyisen_job j on s.jCode = j.jCode\r\njoin shuyisen_edu e on s.eCode = e.eCode\r\njoin shuyisen_staff_state state on s.stateCode = state.stateCode", connection);
                    var reader = command.ExecuteReader();
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
        public async Task<List<Personnel>> GetPersonnelListAsync()
        {
            var personnels = new List<Personnel>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new MySqlCommand("select p.pID,p.sID,s.sName,j.jDescription,state.stateDescription,c.cDescription,p.pDescription from shuyisen_personnel p join shuyisen_staff s on s.sID = p.sID join shuyisen_job j on j.jCode = s.jCode join shuyisen_staff_state state on s.stateCode = state.stateCode join shuyisen_change c on p.cCode = c.cCode order by p.pID asc", connection);
                    var reader = command.ExecuteReader();
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
            }
            catch(Exception ex)
            {
                throw new Exception("获取变更记录失败", ex);
            }
        }
        // 获取所有变更操作
        public async Task<List<String>> GetChangeListAsync()
        {
            var Changes = new List<string>();
            try
            {
                using(var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("select cDescription from shuyisen_kcsj.shuyisen_change where cDescription != '入职' order by cCode asc", connection);
                    var reader = command.ExecuteReader();
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
        public async Task<List<string>> GetJobListAsync()
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
        // 获取所有部门
        public async Task<List<string>> GetDepartmentListAsync()
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
    }
}
