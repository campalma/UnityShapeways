﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
public class OAuth{
	
	private static string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
	
	public static string GenerateNonce() {
        // Just a simple implementation of a random number between 123400 and 9999999
        return UnityEngine.Random.Range(123400, 9999999).ToString();            
    }
	
    public static string GenerateTimeStamp() {
        // Default implementation of UNIX time of the current UTC time
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();            
    }
	
	public static string generateSignature(string url, string method, Dictionary<string, string> parameters, string consumerKeySecret, string accessTokenSecret){
		string urlParams = generateUrlParams(parameters);
		string signingKey = consumerKeySecret+"&"+accessTokenSecret;
		
		string result = method+"&"+urlEncode(url)+"&";
		KeyedHashAlgorithm hmac = new HMACSHA1 (GetBytes (signingKey));
		result = ToBase64 (hmac.ComputeHash (GetBytes (result+urlEncode(urlParams))));
		return result;
	}
	
	public static Dictionary<string, string> generateParams(string consumerKey, string accessToken){
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("oauth_consumer_key", consumerKey);
		parameters.Add("oauth_nonce", OAuth.GenerateNonce());
		parameters.Add("oauth_signature_method", "HMAC-SHA1");
		parameters.Add("oauth_timestamp", OAuth.GenerateTimeStamp());
		parameters.Add("oauth_token", accessToken);
		parameters.Add("oauth_version", "1.0");
		return parameters;
	}
	
	public static void addHeaders(HTTP.Request request, Dictionary<string,string> oauthParams, string url, string consumerSecret, string tokenSecret){
		string oauth_signature = OAuth.urlEncode(OAuth.generateSignature(url, request.method, oauthParams, consumerSecret, tokenSecret));	
		request.SetHeader("Accept", "application/json");
		request.SetHeader("Content-type", "application/x-www-form-urlencoded");
		request.SetHeader("Authorization", "OAuth oauth_consumer_key=\""+ShapewaysKeys.consumerKey+"\", oauth_signature_method=\"HMAC-SHA1\", oauth_nonce=\""+oauthParams["oauth_nonce"]+"\", oauth_timestamp=\""+oauthParams["oauth_timestamp"]+"\", oauth_version=\"1.0\", oauth_token=\""+oauthParams["oauth_token"]+"\", oauth_signature=\""+oauth_signature+"\"");
	}
	
	private static string generateUrlParams(Dictionary<string,string> parameters){
		string urlParams = "";
		
		List<string> sortedKeys = new List<string>();
		foreach (KeyValuePair<string, string> pair in parameters)
	    {
        	sortedKeys.Add(pair.Key);
    	}
		sortedKeys.Sort();
		foreach (string key in sortedKeys){
			urlParams = urlParams+key+"="+parameters[key]+"&";
		}
		
		urlParams = urlParams.Remove(urlParams.Length - 1);
		return urlParams;
	}
	
    public static string urlEncode(string value) {
        StringBuilder result = new StringBuilder();

        foreach (char symbol in value) {
            if (unreservedChars.IndexOf(symbol) != -1) {
                result.Append(symbol);
            } else {
                result.Append('%' + String.Format("{0:X2}", (int)symbol));
            }
        }

        return result.ToString();
    }
	
	public static string ToBase64(byte[] input){
		return Convert.ToBase64String(input);
	}

	public static byte[] GetBytes (string input){
		return UTF8Encoding.UTF8.GetBytes (input);
	}
	
	public static string ToQueryString(Dictionary<string, string> parameters){
		List<string> a = new List<string>();
		foreach(KeyValuePair<string, string> pair in parameters){			
			a.Add(pair.Key+"="+OAuth.urlEncode(pair.Value));
		}
	    return "?" + string.Join("&", a.ToArray());
	}
	
}

