using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class TravelPlanner : MonoBehaviour
{
    private string apiKey = "fc2c0da05554b22676af8ec1bd624537";
    private string city = "Sapporo";

    [SerializeField] private WeatherResponse getData;

    void Start()
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=kr";
        using (WebClient client = new WebClient())
        {
            string json = client.DownloadString(url);
            var data = JsonConvert.DeserializeObject<WeatherResponse>(json);

            getData = data;

            Debug.Log($"현재 {data.name}의 온도: {data.main.temp}°C, 날씨: {data.weather[0].description}");

            GetExchangeRate(data.sys.country);
        }
    }

    [Serializable]
    private class WeatherResponse
    {
        public Coord coord; // 위도, 경도 정보
        public WeatherInfo[] weather; // 날씨 정보 (배열)
        public string @base; // 데이터 기준
        public MainData main; // 주요 기온 정보
        public int visibility; // 가시 거리
        public Wind wind; // 바람 정보
        public Clouds clouds; // 구름량 정보
        public long dt; // 데이터 계산 시간 (유닉스 타임)
        public Sys sys; // 국가, 일출/일몰 정보
        public int timezone; // 시간대 오프셋 (초 단위)
        public int id; // 도시 ID
        public string name; // 도시 이름
        public int cod; // API 응답 코드
    }

    [Serializable]
    private class Coord
    {
        public float lon; // 경도
        public float lat; // 위도
    }

    [Serializable]
    private class WeatherInfo
    {
        public int id; // 날씨 상태 ID
        public string main; // 날씨 상태 (예: "Clear")
        public string description; // 상세 설명 (예: "맑음")
        public string icon; // 아이콘 코드
    }

    [Serializable]
    private class MainData
    {
        public float temp; // 현재 온도
        public float feels_like; // 체감 온도
        public float temp_min; // 최저 온도
        public float temp_max; // 최고 온도
        public int pressure; // 기압 (hPa)
        public int humidity; // 습도 (%)
        public int sea_level; // 해수면 기압 (옵션)
        public int grnd_level; // 지면 기압 (옵션)
    }

    [Serializable]
    private class Wind
    {
        public float speed; // 풍속 (m/s)
        public int deg; // 풍향 (도)
    }

    [Serializable]
    private class Clouds
    {
        public int all; // 구름량 (%)
    }

    [Serializable]
    private class Sys
    {
        public int type; // 시스템 타입 (옵션)
        public int id; // 시스템 ID (옵션)
        public string country; // 국가 코드 (예: "KR")
        public long sunrise; // 일출 시간 (유닉스 타임)
        public long sunset; // 일몰 시간 (유닉스 타임)
    }

    private string API_KEY = "37a6cb4f0bcb6e835470234f"; // 🔹 여기에 본인 API 키 입력
    private string BASE_URL = "https://v6.exchangerate-api.com/v6/";

    public string resultText; // 🔹 결과 표시 UI

    public void GetExchangeRate(string countryCode)
    {
        string getCountryCode = GetCurrencyCode(countryCode);

        if (!string.IsNullOrEmpty(getCountryCode))
        {
            StartCoroutine(GetExchangeRateCoroutine(getCountryCode));
        }
    }

    IEnumerator GetExchangeRateCoroutine(string countryCurrencyCode)
    {
        string url = $"{BASE_URL}{API_KEY}/latest/{countryCurrencyCode}"; // 입력된 국가의 통화를 기준으로 설정

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ExchangeRateData data = JsonConvert.DeserializeObject<ExchangeRateData>(jsonResponse);

                if (data != null && data.conversion_rates.ContainsKey("KRW"))
                {
                    float exchangeRateToKrw = data.conversion_rates["KRW"]; // 기준 통화 -> KRW 환율
                    string symbol = GetCurrencySymbol(countryCurrencyCode); // 해당 통화의 기호 가져오기

                    Debug.Log($"{symbol}1 = {exchangeRateToKrw:F2} 원");
                }
                else
                {
                    Debug.Log("🚨 KRW 환율 정보를 가져올 수 없습니다.");
                }
            }
            else
            {
                Debug.Log("⚠️ 환율 API 요청 실패: " + request.error);
            }
        }
    }

    private string GetCurrencyCode(string countryCode)
    {
        Dictionary<string, string> countryToCurrency = new Dictionary<string, string>
        {
            { "US", "USD" }, { "EU", "EUR" }, { "JP", "JPY" }, { "CN", "CNY" },
            { "ZA", "ZAR" }, { "NZ", "NZD" }, { "TW", "TWD" }, { "RU", "RUB" },
            { "MO", "MOP" }, { "MY", "MYR" }, { "VN", "VND" }, { "BR", "BRL" },
            { "SA", "SAR" }, { "SE", "SEK" }, { "CH", "CHF" }, { "SG", "SGD" },
            { "GB", "GBP" }, { "ID", "IDR" }, { "CA", "CAD" }, { "TH", "THB" },
            { "PH", "PHP" }, { "AU", "AUD" }, { "HK", "HKD" }
        };

        if (countryToCurrency.ContainsKey(countryCode))
        {
            return countryToCurrency[countryCode];
        }

        return null;
    }

    private string GetCurrencySymbol(string currencyCode)
    {
        Dictionary<string, string> currencySymbols = new Dictionary<string, string>
        {
            { "USD", "$" }, { "EUR", "€" }, { "JPY", "¥" }, { "CNY", "¥" },
            { "ZAR", "R" }, { "NZD", "$" }, { "TWD", "NT$" }, { "RUB", "₽" },
            { "MOP", "MOP$" }, { "MYR", "RM" }, { "VND", "₫" }, { "BRL", "R$" },
            { "SAR", "﷼" }, { "SEK", "kr" }, { "CHF", "CHF" }, { "SGD", "S$" },
            { "GBP", "£" }, { "IDR", "Rp" }, { "CAD", "$" }, { "THB", "฿" },
            { "PHP", "₱" }, { "AUD", "$" }, { "HKD", "HK$" }
        };

        return currencySymbols.ContainsKey(currencyCode) ? currencySymbols[currencyCode] : currencyCode;
    }

    [Serializable]
    public class ExchangeRateData
    {
        public string base_code;
        public SerializableDictionary<string, float> conversion_rates;
    }
    
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
    }
}
