using System;
using System.Collections.Generic;
using System.Net.Http;
using Amazon;
using Amazon.Route53;
using Amazon.Route53.Model;
using Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json");
            var config = builder.Build();
            var appConfig = config.GetSection("Settings").Get<AppSettings>();
            //Pain to get more conplex constants from config
            switch (appConfig.EndpointStr)
            {
                case "RegionEndpoint.USWest2":
                    appConfig.Endpoint = RegionEndpoint.USWest2;
                    break;
                // Add others if they come up
            }

            switch (appConfig.TypeStr)
            {
                case "RRType.A":
                    appConfig.Type = RRType.A;
                    break;
                //Add if they come up
            }


            string publicIp;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(appConfig.IpBaseAddress);
                var response = client.GetStringAsync(appConfig.IpPath).Result;
                var result = JsonConvert.DeserializeObject<IpResponse>(response);
                publicIp = result.publicIp;
            }

            //using (var route53Client = new AmazonRoute53Client(appConfig.Route53AppKey, appConfig.Route53AppSecret, appConfig.Endpoint))
            // using the aws profile
            using (var route53Client = new AmazonRoute53Client(appConfig.Endpoint))
            {
                var current = route53Client.TestDNSAnswerAsync(new TestDNSAnswerRequest() {
                    HostedZoneId = appConfig.HostedZoneId,
                    RecordName = appConfig.RecordName,
                    RecordType = appConfig.Type
                }).Result;

                if (current.RecordData.Count == 0)
                {
                    SetARecord(route53Client, publicIp, appConfig);
                }
                else if (current.RecordData.Count == 1 )
                {
                    if (current.RecordData[0] != publicIp)
                    {
                        SetARecord(route53Client, publicIp, appConfig);
                    }
                    //log no change?
                }
                else
                {
                    //log multiple records?
                }
            }
            // Right now this sumbits and rolls with it, no error catch, no validation the change ever went past pending

        }

        private static ChangeResourceRecordSetsResponse SetARecord(AmazonRoute53Client route53Client, string publicIp, AppSettings appConfig)
        {
            var ret = route53Client.ChangeResourceRecordSetsAsync(new ChangeResourceRecordSetsRequest()
            {
                HostedZoneId = appConfig.HostedZoneId,
                ChangeBatch = new ChangeBatch()
                {
                    Changes = new List<Change>()
                    {
                        new Change()
                        {
                            Action = ChangeAction.UPSERT,
                            ResourceRecordSet = new ResourceRecordSet()
                            {
                                Name = appConfig.RecordName,
                                Type = appConfig.Type,
                                TTL = appConfig.TTL,
                                ResourceRecords = new List<ResourceRecord>()
                                {
                                    new ResourceRecord()
                                    {
                                        Value = publicIp
                                    }
                                }
                            }
                        }
                    }
                }
            }).Result;

            return ret;
        }

        public class AppSettings
        { 
            public string IpBaseAddress { get; set; }
            public Uri IpPath { get; set; }
            public string Route53AppKey { get; set; }
            public string Route53AppSecret { get; set; }
            public string EndpointStr { get; set; }
            public RegionEndpoint Endpoint { get; set; }
            public string HostedZoneId { get; set; }
            public string RecordName { get; set; }
            public string TypeStr { get; set; }
            public RRType Type { get; set; }
            public int TTL { get; set; }
        }   
    }
}
