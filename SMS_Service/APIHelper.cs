using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using SMS_Service.Models;

namespace SMS_Service
{
    public class APIHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private string URL { get; set; }
        private string User { get; set; }
        private string Password { get; set; }
        private string EncodeBase64Password { get; set; }

        public APIHelper(string url, string user, string pass)
        {
            this.URL = url;
            this.User = user;
            this.Password = pass;
            this.EncodeBase64Password = Base64Encode(pass);
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public SMSResponse SendSMS(string sdt, string noidung, string ghichu)
        {
            SMSRequest smsRequest = new SMSRequest(sdt, noidung, ghichu, this.User, this.EncodeBase64Password);

            return CallAPIToSendSMS(smsRequest);
        }

        private SMSResponse CallAPIToSendSMS(SMSRequest smsRequest)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = JsonConvert.SerializeObject(smsRequest);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var resultString = streamReader.ReadToEnd();
                var result = JsonConvert.DeserializeObject<SMSResponse>(resultString);

                return result;
            }
        }
    }

    public class SMSResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        /*public string data { get; set; }
        public string error { get; set; }
        public string success { get; set; }*/
        public bool lastPage { get; set; }

    }
    public class SMSRequest
    {
        public string sdt { get; set; }
        public string noidung { get; set; }
        public string ghichu { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public string token { get; set; }
        public SMSRequest(string sdt, string noidung, string ghichu, string user, string encodeBase64Password)
        {
            this.sdt = sdt;
            this.noidung = noidung;
            this.ghichu = ghichu;
            this.user = user;
            this.pass = encodeBase64Password;
            this.token = CreateMD5(sdt, noidung, user, encodeBase64Password);
        }

        private string CreateMD5(string sdt, string noidung, string user, string passwordBase64)
        {
            string input = sdt + noidung + user + passwordBase64;
            // byte array representation of that string
            byte[] encodedInput = new UTF8Encoding().GetBytes(input);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedInput);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash)
               // without dashes
               .Replace("-", string.Empty)
               // make lowercase
               .ToLower();

            return encoded;


            /* // Use input string to calculate MD5 hash
             using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
             {
                 byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                 byte[] hashBytes = md5.ComputeHash(inputBytes);

                 return Convert.ToHexString(hashBytes); // .NET 5 +

                 // Convert the byte array to hexadecimal string prior to .NET 5
                 // StringBuilder sb = new System.Text.StringBuilder();
                 // for (int i = 0; i < hashBytes.Length; i++)
                 // {
                 //     sb.Append(hashBytes[i].ToString("X2"));
                 // }
                 // return sb.ToString();
             }*/
        }

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
