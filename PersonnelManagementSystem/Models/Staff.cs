using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.AtomPub;

namespace PersonnelManagementSystem.Models
{
    public class Staff
    {
        public string ID { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string Birthday { get; set; }
        public string Department { get; set; }
        public string Job { get; set; }
        public string Education { get; set; }
        public string Specialty { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string State { get; set; }
        public string Authority { get; set; }
    }
}
