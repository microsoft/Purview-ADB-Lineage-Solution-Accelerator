using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace Function.Domain.Providers
{
	public interface IHttpClientManager
	{
		/// <summary>
		/// PostAsync with Token
		/// </summary>
		/// <param name="url"></param>
		/// <param name="content"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content, AuthenticationHeaderValue accessToken, IDictionary<string, string>? headers = null);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="content"></param>
		/// <param name="accessToken"></param>
		/// <param name="headers"></param>
		/// <returns></returns>
		Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content, string accessToken, IDictionary<string, string>? headers = null);

		/// <summary>
		/// GetAsync
		/// </summary>
		/// <param name="url"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		Task<HttpResponseMessage> GetAsync(Uri url, AuthenticationHeaderValue accessToken);

		/// <summary>
		/// DeleteAsync
		/// </summary>
		/// <param name="url"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		Task<HttpResponseMessage?> DeleteAsync(Uri url, string accessToken);

		/// <summary>
		/// GetAsync
		/// </summary>
		/// <param name="url"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		Task<HttpResponseMessage> GetAsync(Uri url, string accessToken);

		/// <summary>
		/// GetAsync
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="accessToken"></param>
		/// <param name="headers"></param>
		/// <returns></returns>
		Task<T?> GetAsync<T>(string url, string accessToken, IDictionary<string, string>? headers = null);

		/// <summary>
		/// GetAsync
		/// </summary>
		/// <param name="url"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		Task<T?> GetAsync<T>(string url, AuthenticationHeaderValue accessToken, IDictionary<string, string>? headers = null);

	}
}
