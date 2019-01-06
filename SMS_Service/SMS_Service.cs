using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using SMS_Service.Models;
using System.Linq;

namespace SMS_Service
{
    public partial class SMS_Service : ServiceBase
    {
        private Timer timeDelay = null;
        private Config config = null;
        MySqlHelper mySql = null;
        SmsHelper sms = null;

        public SMS_Service()
        {
            InitializeComponent();

            // timeDelay = new System.Timers.Timer();
            // timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);

            this.ServiceName = "SMS Service";
            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Threading.Thread newThread_Warning = new System.Threading.Thread(SendSMS_Warning);
            newThread_Warning.Start();

            System.Threading.Thread newThread_ConnectionIssue = new System.Threading.Thread(SendSMS_ConnectionIssue);
            newThread_ConnectionIssue.Start();
        }

        public void SendSMS_Warning()
        {
            // Try to connect to DB
            var mySql = new MySqlHelper(config.MySql.Server, config.MySql.User, config.MySql.Pass, config.MySql.DBName, config.MySql.SSL);

            // Get data and send message
            // Try to send a message
            // get all people to send sms
            var lstPeople = mySql.Select_PeopleToSend();
            // ServiceLog.WriteErrorLog("lstPeople:" + lstPeople.Count);
            if (lstPeople != null && lstPeople.Count > 0)
            {
                var lstSms = mySql.Select_SMSPending_Type_Warning();
                // ServiceLog.WriteErrorLog("lstSms:" + lstSms.Count);
                if (lstSms != null && lstSms.Count > 0)
                {
                    //Sending message...
                    foreach (var itemSms in lstSms)
                    {
                        var people_Filter = lstPeople.Where(o => o.hostId == itemSms.hostId).ToList();
                        var people_Filter_phone_number = new List<string>();

                        if (people_Filter != null && people_Filter.Count > 0)
                            people_Filter_phone_number = people_Filter.Select(o => o.phone).ToList();

                        // List default
                        if (config.SmsApi.ListPeopleByPhoneNumber != null && config.SmsApi.ListPeopleByPhoneNumber.Count > 0)
                            people_Filter_phone_number.AddRange(config.SmsApi.ListPeopleByPhoneNumber);

                        // Distinct
                        people_Filter_phone_number = people_Filter_phone_number.Distinct().ToList();

                        if (people_Filter_phone_number != null && people_Filter_phone_number.Count > 0)
                        {
                            itemSms.message = convertToUnSign(itemSms.hostname + ": " + itemSms.message) + " - luc " + itemSms.createddate;

                            // List phone
                            var listPhone = string.Join(",", people_Filter_phone_number);
                            ServiceLog.WriteErrorLog(string.Format("[GROUP=" + SMSGroup.GROUP_WARNING + "] SMS sending...: HostId: {0}; SMSId: {1}; List Phone number: {2}", itemSms.hostId, itemSms.id, listPhone));

                            // send each person
                            foreach (var phoneNumber in people_Filter_phone_number)
                            {
                                // Send message
                                if (sms.SendMessage(phoneNumber, itemSms.message))
                                {
                                    // Update message record to sent
                                    mySql.UpdateSmsStatusToSent(itemSms.id);
                                }
                            }
                        } // End if (people_Filter != null && people_Filter.Count > 0)
                    } // End foreach (var itemSms in lstSms)

                } // End if (lstSms != null && lstSms.Count > 0)
            }
        }

        public void SendSMS_ConnectionIssue()
        {
            // Try to connect to DB
            var mySql = new MySqlHelper(config.MySql.Server, config.MySql.User, config.MySql.Pass, config.MySql.DBName, config.MySql.SSL);

            // Get data and send message
            // Try to send a message
            // get all people to send sms
            var lstPeople = mySql.Select_PeopleToSend();
            // ServiceLog.WriteErrorLog("lstPeople:" + lstPeople.Count);
            if (lstPeople != null && lstPeople.Count > 0)
            {
                var lstSms = mySql.Select_SMSPending_Type_ConnectionIssue();
                // ServiceLog.WriteErrorLog("lstSms:" + lstSms.Count);
                if (lstSms != null && lstSms.Count > 0)
                {
                    //Sending message...
                    foreach (var itemSms in lstSms)
                    {
                        var people_Filter = lstPeople.Where(o => o.hostId == itemSms.hostId).ToList();
                        var people_Filter_phone_number = new List<string>();

                        if (people_Filter != null && people_Filter.Count > 0)
                            people_Filter_phone_number = people_Filter.Select(o => o.phone).ToList();

                        // List default
                        if (config.SmsApi.ListPeopleByPhoneNumber != null && config.SmsApi.ListPeopleByPhoneNumber.Count > 0)
                            people_Filter_phone_number.AddRange(config.SmsApi.ListPeopleByPhoneNumber);

                        // Distinct
                        people_Filter_phone_number = people_Filter_phone_number.Distinct().ToList();

                        if (people_Filter_phone_number != null && people_Filter_phone_number.Count > 0)
                        {
                            itemSms.message = convertToUnSign(itemSms.hostname + ": " + itemSms.message) + " - luc " + itemSms.createddate;

                            // List phone
                            var listPhone = string.Join(",", people_Filter_phone_number);
                            ServiceLog.WriteErrorLog(string.Format("[GROUP=" + SMSGroup.GROUP_CONNECTION_ISSUE + "] SMS sending...: HostId: {0}; SMSId: {1}; List Phone number: {2}", itemSms.hostId, itemSms.id, listPhone));

                            // send each person
                            foreach (var phoneNumber in people_Filter_phone_number)
                            {
                                // Send message
                                if (sms.SendMessage(phoneNumber, itemSms.message))
                                {
                                    // Update message record to sent
                                    mySql.UpdateSmsStatusToSent(itemSms.id);
                                }
                            }
                        } // End if (people_Filter != null && people_Filter.Count > 0)
                    } // End foreach (var itemSms in lstSms)

                } // End if (lstSms != null && lstSms.Count > 0)
            }
        }

        protected override void OnStart(string[] args)
        {
            // Load config
            config = FileHelper.ReadConfigFile();
            if (config.Exception != null)
            {
                ServiceLog.WriteErrorLog(config.Exception);
                // Stop service
                OnStop();
            }

            // Try to connect to DB
            mySql = new MySqlHelper(config.MySql.Server, config.MySql.User, config.MySql.Pass, config.MySql.DBName, config.MySql.SSL);

            sms = new SmsHelper(config.SmsApi.User, config.SmsApi.Pass);

            timeDelay = new Timer();
            this.timeDelay.Interval = config.Interval;
            this.timeDelay.Elapsed += new ElapsedEventHandler(WorkProcess);
            timeDelay.Enabled = true;
            ServiceLog.WriteErrorLog("SMS service started");
        }
        protected override void OnStop()
        {
            LogService("SMS service Stoped");
            timeDelay.Enabled = false;
        }
        private void LogService(string content)
        {
            ServiceLog.WriteErrorLog(content);
        }
        public string convertToUnSign(string s)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
    }
}
