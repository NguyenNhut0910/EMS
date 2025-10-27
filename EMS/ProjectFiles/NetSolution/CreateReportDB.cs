#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using FTOptix.Store;
using FTOptix.CommunicationDriver;
using FTOptix.Modbus;

#endregion

public class CreateReportDB : BaseNetLogic
{
    readonly struct HTTPResponse
    {
        public HTTPResponse(string payload, int code)
        {
            Payload = payload;
            Code = code;
        }

        public string Payload { get; }
        public int Code { get; }
    };

    public override void Start()
    {
    }
    public class MeasurementRecord
    {
        public int device_id { get; set; }
        public double voltage { get; set; }
        public double current { get; set; }
        public double power { get; set; }
        public double frequency { get; set; }
        public DateTime timestamp { get; set; }
    }

    [ExportMethod]
    public void GetDataCustom()
    {
        try
        {
            // Lấy bảng đích (MeasurementTable)
            var tableVar = LogicObject.GetVariable("Table");
            if (tableVar == null)
                throw new Exception("Không tìm thấy biến Table");

            var tableNodeId = tableVar.Value;
            var table = InformationModel.Get(tableNodeId) as Table;
            if (table == null)
                throw new Exception("Biến Table không trỏ tới Table hợp lệ");

            // Lấy đối tượng Store từ bảng
            var store = table.Owner.Owner as Store;

            // // Xoá dữ liệu cũ
            string deleteQuery = "DELETE FROM MeasurementTable";
            store.Query(deleteQuery, out _, out _);
            // Lấy thiết bị
            string[] NameDevice = { "1", "2", "3", "4" };
            string[] BoolVarNameDevice = { "DeviceSelected1", "DeviceSelected2", "DeviceSelected3", "DeviceSelected4" };
            var selectedColNameDevice = new List<string>();

            for (int i = 0; i < NameDevice.Length; i++)
            {
                var varNode = Project.Current.GetVariable($"Model/History/Select/Device/{BoolVarNameDevice[i]}");
                if (varNode == null)
                {
                    Log.Warning($"Không tìm thấy biến: {BoolVarNameDevice[i]}");
                    continue;
                }

                if ((bool)varNode.Value)
                    selectedColNameDevice.Add($"{NameDevice[i]}");
            }

            string columnStringDevice = selectedColNameDevice.Count > 0 ? string.Join(" ,", selectedColNameDevice) : "";
            Log.Info($"Selected Devices: {columnStringDevice}");

            string fromDate = LogicObject.GetVariable("FromDate").Value;
            string toDate = LogicObject.GetVariable("ToDate").Value;
                // Dùng đúng định dạng có dấu '-' cho ngày tháng
            DateTime fromDateTime = DateTime.ParseExact(fromDate.Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime toDateTime = DateTime.ParseExact(toDate.Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            // Chuyển sang định dạng ISO 8601 (UTC) với hậu tố 'Z'
            string fromDateISO = fromDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string toDateISO = toDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";
            string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";
            string apiUrl = "http://nhatvietindustry.ddns.net:22321/rest/v1/data";
            string query = $"select=*&timestamp=gte.{fromDateISO}&timestamp=lte.{toDateISO}&device_id=in.({columnStringDevice})&order=timestamp.desc";
            // string query = "select=*&order=timestamp.desc";
            string response;
            int statusCode;
            Get(apiUrl, query, bearerToken, out response, out statusCode);
            Log.Info($"GET Status: {statusCode}");

            if (statusCode != 200 || string.IsNullOrEmpty(response))
            {
                Log.Warning("RESTApiClient1", "Không nhận được dữ liệu từ API.");
                return;
            }

            // Parse JSON
            var records = JsonSerializer.Deserialize<List<MeasurementRecord>>(response);
            if (records == null || records.Count == 0)
            {
                Log.Warning("RESTApiClient1", "Không có dữ liệu sau khi parse JSON.");
                return;
            }

            // // Lấy bảng đích (MeasurementTable)
            // var tableVar = LogicObject.GetVariable("Table");
            // if (tableVar == null)
            //     throw new Exception("Không tìm thấy biến Table");

            // var tableNodeId = tableVar.Value;
            // var table = InformationModel.Get(tableNodeId) as Table;
            // if (table == null)
            //     throw new Exception("Biến Table không trỏ tới Table hợp lệ");

            // // Lấy đối tượng Store từ bảng
            // var store = table.Owner.Owner as Store;

            // // // Xoá dữ liệu cũ
            // string deleteQuery = "DELETE FROM MeasurementTable";
            // store.Query(deleteQuery, out _, out _);

            // Ghi dữ liệu mới
            foreach (var rec in records)
            {
                string[] columnNames = new string[] {
                    "device_id", "voltage", "current", "power", "frequency", "timestamp"
                };

                object[,] values = new object[,]
                {
                    {
                        rec.device_id,
                        rec.voltage,
                        rec.current,
                        rec.power,
                        rec.frequency,
                        rec.timestamp
                    }
                };

                table.Insert(columnNames, values);
            }

            Log.Info("RESTApiClient1", $"Ghi {records.Count} bản ghi vào MeasurementTable thành công.");
            // 
            // try
            // {
            //     var triggerVar = LogicObject.GetVariable("AutoGenerateReport").Value;
            //     if (triggerVar != true)
            //     {
            //         // triggerVar.Value = true;
            //         // Thì bấm cái nút này để tạo file pdf TestAppEnerg/UI/Pages/Report/Panel1/Button1
            //         Log.Info("RESTApiClient1", "Đã kích hoạt tạo report.pdf");
                    
            //         // triggerVar = Project.Current.GetVariable("UI/MainWindow/AutoGenerateReport");
            //         LogicObject.GetVariable("AutoGenerateReport").Value = false;
            //         // triggerVar.Value = false;

            //     }
            //     else
            //     {
            //         Log.Warning("RESTApiClient1", "Không tìm thấy biến AutoGenerateReport.");
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Log.Warning("RESTApiClient1", $"Lỗi khi kích hoạt tạo report.pdf: {ex.Message}");
            // }
            
            // 
        }
        catch (Exception ex)
        {
            Log.Error("RESTApiClient1", $"Lỗi trong GetDataCustom(): {ex.Message}");
        }
    }

    public override void Stop()
    {
    }

    private long GetTimeout()
    {
        var timeoutVariable = LogicObject.Get<IUAVariable>("Timeout");
        if (timeoutVariable == null)
            throw new Exception($"Missing Timeout variable under the NetLogic {LogicObject.BrowseName}");

        return timeoutVariable.Value;
    }

    private string GetUserAgent()
    {
        var userAgentVariable = LogicObject.Get<IUAVariable>("UserAgent");
        if (userAgentVariable == null)
            throw new Exception($"Missing UserAgent variable under the NetLogic {LogicObject.BrowseName}");

        return userAgentVariable.Value;
    }

    private bool IsSupportedScheme(string scheme)
    {
        return scheme == "http" || scheme == "https";
    }

    private bool IsSecureScheme(string scheme)
    {
        return scheme == "https";
    }

    private HttpRequestMessage BuildGetMessage(Uri url, string userAgent, string bearerToken)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

        if (!string.IsNullOrWhiteSpace(userAgent))
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue(userAgent)));

        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return request;
    }

    private HttpRequestMessage BuildPostMessage(Uri url, string body, string contentType, string userAgent, string bearerToken)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

        if (!string.IsNullOrWhiteSpace(body))
            request.Content = new StringContent(body, System.Text.Encoding.UTF8, contentType);

        if (!string.IsNullOrWhiteSpace(userAgent))
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue(userAgent)));

        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return request;
    }

    private HttpRequestMessage BuildPutMessage(Uri url, string body, string contentType, string userAgent, string bearerToken)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);

        if (!string.IsNullOrWhiteSpace(body))
            request.Content = new StringContent(body, System.Text.Encoding.UTF8, contentType);

        if (!string.IsNullOrWhiteSpace(userAgent))
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue(userAgent)));

        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return request;
    }

    private async Task<HTTPResponse> PerformRequest(HttpRequestMessage request, TimeSpan timeout)
    {
        HttpClient client = new HttpClient();
        client.Timeout = timeout;

        using HttpResponseMessage httpResponse = await client.SendAsync(request);
        string responseBody = await httpResponse.Content.ReadAsStringAsync();

        return new HTTPResponse(responseBody, (int)httpResponse.StatusCode);
    }

    private HttpRequestMessage BuildMessage(HttpMethod verb, Uri url, string requestBody, string bearerToken, string contentType)
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(GetTimeout());
        string userAgent = GetUserAgent();

        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "application/json";

        if (!IsSupportedScheme(url.Scheme))
            throw new Exception($"The URI scheme {url.Scheme} is not supported");

        if (!IsSecureScheme(url.Scheme) && !string.IsNullOrWhiteSpace(bearerToken))
            Log.Warning("Possible sending of unencrypted confidential information");

        if (verb == HttpMethod.Get)
            return BuildGetMessage(url, userAgent, bearerToken);
        if (verb == HttpMethod.Post)
            return BuildPostMessage(url, requestBody, contentType, userAgent, bearerToken);
        if (verb == HttpMethod.Put)
            return BuildPutMessage(url, requestBody, contentType, userAgent, bearerToken);

        throw new Exception($"Unsupported verb { verb }");
    }

    [ExportMethod]
    public void Get(string apiUrl, string queryString, string bearerToken, out string response, out int code)
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(GetTimeout());
        UriBuilder uriBuilder = new UriBuilder(apiUrl);
        uriBuilder.Query = queryString;

        var requestMessage = BuildMessage(HttpMethod.Get, uriBuilder.Uri, "", bearerToken, "");
        var requestTask = PerformRequest(requestMessage, timeout);
        var httpResponse = requestTask.Result;

        (response, code) = (httpResponse.Payload, httpResponse.Code);
    }

    [ExportMethod]
    public void Post(string apiUrl, string requestBody, string bearerToken, string contentType, out string response, out int code)
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(GetTimeout());
        UriBuilder uriBuilder = new UriBuilder(apiUrl);

        var requestMessage = BuildMessage(HttpMethod.Post, uriBuilder.Uri, requestBody, bearerToken, contentType);
        var requestTask = PerformRequest(requestMessage, timeout);
        var httpResponse = requestTask.Result;

        (response, code) = (httpResponse.Payload, httpResponse.Code);
    }

    [ExportMethod]
    public void Put(string apiUrl, string requestBody, string bearerToken, string contentType, out string response, out int code)
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(GetTimeout());
        UriBuilder uriBuilder = new UriBuilder(apiUrl);

        var requestMessage = BuildMessage(HttpMethod.Put, uriBuilder.Uri, requestBody, bearerToken, contentType);
        var requestTask = PerformRequest(requestMessage, timeout);
        var httpResponse = requestTask.Result;

        (response, code) = (httpResponse.Payload, httpResponse.Code);
    }
}
