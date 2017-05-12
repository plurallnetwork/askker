using Askker.App.PortableLibrary.Models;
using Askker.App.PortableLibrary.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Askker.App.PortableLibrary.Business
{
    public class ReportManager
    {
        public async Task<ReportModel> GetOverallResults(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                ReportService reportService = new ReportService();

                var response = await reportService.GetOverallResults(userId, creationDate, authenticationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return ParseReportModel(json);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReportModel> GetResultsByAge(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                ReportService reportService = new ReportService();

                var response = await reportService.GetResultsByAge(userId, creationDate, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return ParseReportModel(json);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReportModel> GetResultsByGender(string userId, string creationDate, string authenticationToken)
        {
            try
            {
                ReportService reportService = new ReportService();

                var response = await reportService.GetResultsByGender(userId, creationDate, authenticationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return ParseReportModel(json);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Unauthorized");
                    }
                    else
                    {
                        throw new Exception(response.StatusCode.ToString() + " - " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ReportModel ParseReportModel(string json)
        {
            JToken jsonReportModel = JToken.Parse(json);

            ReportModel reportModel = new ReportModel();

            if (jsonReportModel is JObject)
            {
                JArray datasets = (JArray) jsonReportModel["data"]["datasets"];
                JArray labels = (JArray) jsonReportModel["data"]["labels"];

                reportModel.dataSets = new List<List<int>>();
                foreach (var reportData in datasets)
                {
                    reportModel.dataSets.Add(reportData["data"].ToObject<List<int>>());
                }

                reportModel.labels = labels.ToObject<List<string>>();

                reportModel.totalVotes = Int32.Parse(jsonReportModel["data"]["totalVotes"].ToString());
            }
            else if (jsonReportModel is JArray)
            {
                JObject objectReportModel = (JObject) jsonReportModel.ToObject<JArray>()[0];

                JArray datasets = (JArray) objectReportModel["data"]["datasets"];
                JArray groups = (JArray) objectReportModel["data"]["labels"];

                reportModel.dataSets = new List<List<int>>();
                reportModel.labels = new List<string>();
                foreach (var reportData in datasets)
                {
                    reportModel.dataSets.Add(reportData["data"].ToObject<List<int>>());
                    reportModel.labels.Add(reportData["label"].ToString());
                }

                reportModel.groups = groups.ToObject<List<string>>();

                reportModel.totalVotes = Int32.Parse(objectReportModel["data"]["totalVotes"].ToString());
            }

            return reportModel;
        }
    }
}
