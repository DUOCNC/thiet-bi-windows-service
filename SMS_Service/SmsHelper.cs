using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Service
{
    public class SmsHelper
    {
        public string User { get; set; }
        public string Pass { get; set; }

        public SmsHelper(string User, string Pass)
        {
            this.User = User;
            this.Pass = Pass;
        }

        public bool SendMessage(string PhoneNumber, string Message, short Type = 0, bool TypeSpecified = false, short Channel = 0, bool ChannelSpecified = false)
        {
            try
            {
                SmsService.Sms a = new SmsService.Sms();
                a.Send(PhoneNumber, Message, Type, TypeSpecified, Channel, ChannelSpecified, this.User, this.Pass);
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                return false;
            }

            return true;
        }
    }
}
