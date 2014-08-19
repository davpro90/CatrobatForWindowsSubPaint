﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Catrobat.IDE.Core.Utilities;
using Catrobat.IDE.Core.Utilities.Helpers;
using Catrobat.IDE.Core.Utilities.JSON;
using Catrobat.IDE.Core.CatrobatObjects;
using Catrobat.IDE.Core.Resources;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Catrobat.IDE.Core.ViewModels.Main;
using Catrobat.IDE.Core.Xml.VersionConverter;
using Newtonsoft.Json;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.Services.Storage;

namespace Catrobat.IDE.Core.Services.Common
{
    public class WebCommunicationService : IWebCommunicationService
    {
        private static int _uploadCounter = 0;
        public event DownloadProgressUpdatedEventHandler DownloadProgressChanged;
        public async Task<List<OnlineProgramHeader>> LoadOnlineProgramsAsync(
            string filterText, int offset, int count,
            CancellationToken taskCancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                //httpClient.BaseAddress = new Uri(ApplicationResources.API_BASE_ADDRESS);
                httpClient.BaseAddress = new Uri("https://pocketcode.org/api/");
                //httpClient.DefaultRequestHeaders.Accept.Clear();
                //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage httpResponse = null;

                    if (filterText == "")
                    {
                        httpResponse = await httpClient.GetAsync(
                            String.Format(ApplicationResources.API_RECENT_PROJECTS,
                            count, offset), taskCancellationToken);
                    }
                    else
                    {
                        string encoded_filter_text = WebUtility.UrlEncode(filterText);
                        httpResponse = await httpClient.GetAsync(String.Format(
                            ApplicationResources.API_SEARCH_PROJECTS, encoded_filter_text,
                            count, offset), taskCancellationToken);
                    }
                    httpResponse.EnsureSuccessStatusCode();

                    string jsonResult = await httpResponse.Content.ReadAsStringAsync();
                    OnlineProgramOverview recentPrograms = null;

                    //List<OnlineProgramOverview> programs = JsonConvert.DeserializeObject<List<OnlineProgramOverview>>(jsonResult);
                    recentPrograms = await Task.Run(() => JsonConvert.DeserializeObject<OnlineProgramOverview>(jsonResult));

                    return recentPrograms.CatrobatProjects;
                }
                catch (HttpRequestException)
                {
                    return null;
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }


        public async Task<Stream> DownloadAsync(string downloadUrl, string programName, CancellationToken taskCancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ApplicationResources.POCEKTCODE_BASE_ADDRESS);
                try
                {
                    // trigger to header-read to avoid timeouts
                    var httpResponse = await httpClient.GetAsync(downloadUrl/*, HttpCompletionOption.ResponseHeadersRead*/, taskCancellationToken);
                    httpResponse.EnsureSuccessStatusCode();

                    return await httpResponse.Content.ReadAsStreamAsync();
                }
                catch (HttpRequestException)
                {
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public async Task DownloadAsyncAlternativ(string downloadUrl, string programName)
        {
            throw new NotImplementedException();
        }


        public async Task<JSONStatusResponse> CheckTokenAsync(string username, string token, string language = "en")
        {
            var parameters = new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_USERNAME, ((username == null) ? "" : username)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_TOKEN, ((token == null) ? "" : token)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_LANGUAGE, ((language == null) ? "" : language))
            };

            HttpContent postParameters = new FormUrlEncodedContent(parameters);
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ApplicationResources.API_BASE_ADDRESS);
                JSONStatusResponse statusResponse = null;
                try
                {
                    HttpResponseMessage httpResponse = await httpClient.PostAsync(ApplicationResources.API_CHECK_TOKEN, postParameters);
                    httpResponse.EnsureSuccessStatusCode();

                    string jsonResult = await httpResponse.Content.ReadAsStringAsync();
                    statusResponse = JsonConvert.DeserializeObject<JSONStatusResponse>(jsonResult);
                }
                catch (HttpRequestException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.HTTPRequestFailed;
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.JSONSerializationFailed;
                }
                catch (Exception)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.UnknownError;
                }
                return statusResponse;
            }
        }

        public async Task<JSONStatusResponse> LoginOrRegisterAsync(string username, string password, string userEmail,
                string language = "en", string country = "AT")
        {
            var parameters = new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_REG_USERNAME, ((username == null) ? "" : username)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_REG_PASSWORD, ((password == null) ? "" : password)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_REG_EMAIL, ((userEmail == null) ? "" : userEmail)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_REG_COUNTRY, ((country == null) ? "" : country)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_LANGUAGE, ((language == null) ? "" : language))
            };

            HttpContent postParameters = new FormUrlEncodedContent(parameters);
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ApplicationResources.API_BASE_ADDRESS);
                JSONStatusResponse statusResponse = null;
                try
                {
                    HttpResponseMessage httpResponse = await httpClient.PostAsync(ApplicationResources.API_LOGIN_REGISTER, postParameters);
                    httpResponse.EnsureSuccessStatusCode();

                    string jsonResult = await httpResponse.Content.ReadAsStringAsync();
                    statusResponse = JsonConvert.DeserializeObject<JSONStatusResponse>(jsonResult);
                }
                catch (HttpRequestException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.HTTPRequestFailed;
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.JSONSerializationFailed;
                }
                catch (Exception)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.UnknownError;
                }
                return statusResponse;
            }
        }


        public async Task<JSONStatusResponse> UploadProgramAsync(string programTitle,
            string username, string token, string language = "en")
        {
            var parameters = new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_USERNAME, ((username == null) ? "" : username)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_TOKEN, ((token == null) ? "" : token)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_LANGUAGE, ((language == null) ? "" : language))
            };

            using (var postParameters = new MultipartFormDataContent())
            {
                using (var storage = StorageSystem.GetStorage())
                {
                    JSONStatusResponse statusResponse = null;
                    try
                    {
                        // TODO check if file exists - do not rely on try-catch
                        var stream = await storage.OpenFileAsync(Path.Combine(StorageConstants.TempProgramExportZipPath, programTitle + ApplicationResources.EXTENSION),
                            StorageFileMode.Open, StorageFileAccess.Read);
                        var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);

                        var programData = memoryStream.ToArray();

                        parameters.Add(new KeyValuePair<string, string>(ApplicationResources.API_PARAM_CHECKSUM, UtilTokenHelper.ToHex(MD5Core.GetHash(programData))));
                        string sum = UtilTokenHelper.ToHex(MD5Core.GetHash(programData));

                        // store parameters as MultipartFormDataContent
                        StringContent content = null;
                        foreach (var keyValuePair in parameters)
                        {
                            content = new StringContent(keyValuePair.Value);
                            content.Headers.Remove("Content-Type");
                            postParameters.Add(content, String.Format("\"{0}\"", keyValuePair.Key));
                        }

                        ByteArrayContent fileContent = new ByteArrayContent(programData);
                        //fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        //{
                        //    FileName = programTitle + ".catrobat"
                        //};
                        fileContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/zip");
                        postParameters.Add(fileContent, String.Format("\"{0}\"", ApplicationResources.API_PARAM_UPLOAD), String.Format("\"{0}\"", programTitle + ApplicationResources.EXTENSION));

                        _uploadCounter++;
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.BaseAddress = new Uri(ApplicationResources.API_BASE_ADDRESS);
                            HttpResponseMessage httpResponse = await httpClient.PostAsync(ApplicationResources.API_UPLOAD, postParameters);
                            httpResponse.EnsureSuccessStatusCode();
                            string jsonResult = await httpResponse.Content.ReadAsStringAsync();

                            statusResponse = JsonConvert.DeserializeObject<JSONStatusResponse>(jsonResult);
                        }
                        _uploadCounter--;
                    }
                    catch (HttpRequestException)
                    {
                        statusResponse = new JSONStatusResponse();
                        statusResponse.statusCode = StatusCodes.HTTPRequestFailed;
                    }
                    catch (Newtonsoft.Json.JsonSerializationException)
                    {
                        statusResponse = new JSONStatusResponse();
                        statusResponse.statusCode = StatusCodes.JSONSerializationFailed;
                    }
                    catch (Exception e)
                    {
                        statusResponse = new JSONStatusResponse();
                        statusResponse.statusCode = StatusCodes.UnknownError;
                    }
                    return statusResponse;
                }
            }
        }


        public async Task<JSONStatusResponse> ReportAsInappropriateAsync(string programId, string flagReason, string language = "en")
        {
            var parameters = new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_PROJECTID, ((programId == null) ? "" : programId)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_FLAG_REASON, ((flagReason == null) ? "" : flagReason)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_LANGUAGE, ((language == null) ? "" : language))
            };

            HttpContent postParameters = new FormUrlEncodedContent(parameters);
            using (var httpClient = new HttpClient())
            {
                //httpClient.BaseAddress = new Uri(ApplicationResources.POCEKTCODE_BASE_ADDRESS);
                httpClient.BaseAddress = new Uri("https://catroid-test.catrob.at");
                JSONStatusResponse statusResponse = null;
                try
                {
                    HttpResponseMessage httpResponse = await httpClient.PostAsync(ApplicationResources.CATROWEB_REPORT_AS_INAPPROPRIATE, postParameters);
                    httpResponse.EnsureSuccessStatusCode();

                    string jsonResult = await httpResponse.Content.ReadAsStringAsync();
                    statusResponse = JsonConvert.DeserializeObject<JSONStatusResponse>(jsonResult);
                }
                catch (HttpRequestException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.HTTPRequestFailed;
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.JSONSerializationFailed;
                }
                catch (Exception)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.UnknownError;
                }
                return statusResponse;
            }
        }


        public async Task<JSONStatusResponse> RecoverPasswordAsync(string recoveryUserData, string language = "en")
        {
            var parameters = new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_RECOVER_PWD, ((recoveryUserData == null) ? "" : recoveryUserData)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_LANGUAGE, ((language == null) ? "" : language))
            };

            HttpContent postParameters = new FormUrlEncodedContent(parameters);
            using (var httpClient = new HttpClient())
            {
                //httpClient.BaseAddress = new Uri(ApplicationResources.POCEKTCODE_BASE_ADDRESS);
                httpClient.BaseAddress = new Uri("https://catroid-test.catrob.at");
                JSONStatusResponse statusResponse = null;
                try
                {
                    HttpResponseMessage httpResponse = await httpClient.PostAsync(ApplicationResources.CATROWEB_RECOVER_PWD, postParameters);
                    httpResponse.EnsureSuccessStatusCode();

                    string jsonResult = await httpResponse.Content.ReadAsStringAsync();
                    statusResponse = JsonConvert.DeserializeObject<JSONStatusResponse>(jsonResult);
                }
                catch (HttpRequestException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.HTTPRequestFailed;
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.JSONSerializationFailed;
                }
                catch (Exception)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.UnknownError;
                }
                return statusResponse;
            }
        }


        public async Task<JSONStatusResponse> ChangePasswordAsync(string newPassword, string newPasswortRepeated, string hash, string language = "en")
        {
            var parameters = new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_HASH, ((hash == null) ? "" : hash)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_NEW_PWD, ((newPassword == null) ? "" : newPassword)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_NEW_PWD_REPEAT, ((newPasswortRepeated == null) ? "" : newPasswortRepeated)),
                new KeyValuePair<string, string>(ApplicationResources.API_PARAM_LANGUAGE, ((language == null) ? "" : language))
            };

            HttpContent postParameters = new FormUrlEncodedContent(parameters);
            using (var httpClient = new HttpClient())
            {
                //httpClient.BaseAddress = new Uri(ApplicationResources.POCEKTCODE_BASE_ADDRESS);
                httpClient.BaseAddress = new Uri("https://catroid-test.catrob.at");
                JSONStatusResponse statusResponse = null;
                try
                {
                    HttpResponseMessage httpResponse = await httpClient.PostAsync(ApplicationResources.CATROWEB_CHANGE_PWD, postParameters);
                    httpResponse.EnsureSuccessStatusCode();

                    string jsonResult = await httpResponse.Content.ReadAsStringAsync();
                    statusResponse = JsonConvert.DeserializeObject<JSONStatusResponse>(jsonResult);
                }
                catch (HttpRequestException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.HTTPRequestFailed;
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.JSONSerializationFailed;
                }
                catch (Exception)
                {
                    statusResponse = new JSONStatusResponse();
                    statusResponse.statusCode = StatusCodes.UnknownError;
                }
                return statusResponse;
            }
        }


        public bool NoUploadsPending()
        {
            return _uploadCounter == 0;
        }

        public DateTime ConvertUnixTimeStamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }
}