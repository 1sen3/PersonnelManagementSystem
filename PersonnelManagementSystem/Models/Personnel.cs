using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonnelManagementSystem.Models
{
    public class Personnel
    {
        public int PersonnelID { get; set; }
        public string StaffID { get; set; }
        public string StaffName { get; set; }
        public string StaffJob { get; set; }
        public string StaffState { get; set; }
        public string Operation { get; set; }
        public string Reason { get; set; }
    }
}
