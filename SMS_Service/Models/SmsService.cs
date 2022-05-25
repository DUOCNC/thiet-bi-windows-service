using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Service.Models
{
    public class SmsService
    {
        public string User { get; set; }
        public string Pass { get; set; }
        public string StringPhoneNumber { get; set; }
        public string PhoneNumberChar { get; set; }
        public string APIEndpoint { get; set; }
        public List<string> ListPeopleByPhoneNumber { get; set; }
    }
}
