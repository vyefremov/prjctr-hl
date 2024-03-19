import requests

# 1 - Get the currency rates from the Monobank API
USD_CODE = 840
UAH_CODE = 980

currencyRates = requests.get("https://api.monobank.ua/bank/currency").json()

usdToUah = next((x for x in currencyRates if x['currencyCodeA'] == USD_CODE and x['currencyCodeB'] == UAH_CODE), None)

print(usdToUah)

# 2 - Send the currency rates to the Google Analytics
GA_COLLECTOR_URL = "https://www.google-analytics.com/mp/collect"
GA_MEASUREMENT_ID = "G-SMNCRWTMPQ"
GA_API_SECRET = "Mt6_h2t5TGutn7Qeew8Mhg"
EVENT_NAME = "currency_update"
CLIENT_ID = "123456.7654321"

eventPayload = {
    "client_id": CLIENT_ID,
    "events": [
        {
            "name": EVENT_NAME,
            "params": {
                "category": "USD_UAH",
                "value": usdToUah['rateBuy']
            }
        }
    ]
}

headers = {
    'Content-Type': 'application/json'
}

eventTrackResponse = requests.post(
    GA_COLLECTOR_URL + f"?measurement_id={GA_MEASUREMENT_ID}&api_secret={GA_API_SECRET}", 
    json=eventPayload, 
    headers=headers)

print(eventTrackResponse.status_code)
