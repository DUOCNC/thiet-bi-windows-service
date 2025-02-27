﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Service
{
    public class SmsHelper
    {
        private APIHelper SMSAPI { get; set; }

        public SmsHelper(string APIURL, string User, string Pass)
        {
            this.SMSAPI = new APIHelper(APIURL, User, Pass);
        }

        public bool SendMessage(string PhoneNumber, string Message)
        {
            try
            {
                Task<SMSResponse> task =  SMSAPI.SendSMS(PhoneNumber, Message, "");
                SMSResponse response = task.Result;

                if (response.code != 200)
                {
                    var errMsg = "Error code: " + response.code + "; Message: " + response.msg;
                    ServiceLog.WriteErrorLog(errMsg);
                    return false;
                }
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
