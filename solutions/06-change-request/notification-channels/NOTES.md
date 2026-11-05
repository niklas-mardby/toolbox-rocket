# 📝 NOTES: Notification channels

## 💡 Resonemanget

Problemet har **två dimensioner** som varierar oberoende av varandra:

| | E-post | SMS | Push |
|-|--------|-----|------|
| **Skickad** | ✉️ | ✉️ | ✉️ |
| **Levererad** | ✉️ | ✉️ | ✉️ |
| *(nästa: Försenad)* | | | |

Med den ursprungliga designen blir varje ruta i tabellen kod i `NotificationService`: 3 × 2 = 6 kombinationer, och snart 3 × 3 = 9. Lösningen delar upp dimensionerna:

- **`Notification`** (record med fabriksmetoder) = *vad* som ska sägas, en per händelse.
- **`INotificationChannel`** (Strategy) = *hur* det levereras, en per kanal.
- **`NotificationService`** = *vilka* kanaler som ska användas (preferenser och fallback).

Nu blir det 3 + 2 i stället för 3 × 2. En ny händelse är en fabriksmetod, och en ny kanal är en klass.

- **`CanReach`** låter varje kanal själv avgöra om den har kontaktuppgifter. Tjänsten behöver inte veta att SMS kräver telefonnummer.
- **Fallbacken till e-post** är en *affärsregel* och ligger därför i tjänsten, inte i någon kanal.
- **`Distinct()`** skyddar mot dubbla preferenser i CRM-datan.

## 🔀 Alternativ

- **Observer:** `OrderShipped` och `OrderDelivered` som events, med kanalerna som prenumeranter. Men då måste *varje* kanal själv läsa kundens preferenser och implementera fallbacken. Logiken sprids ut. Strategy plus en väljare är tydligare här.
- **Bridge-mönstret** är det formella namnet på att dela upp två dimensioner så här (abstraktion × implementation). Det är inte ett av repots kärnmönster, men värt att slå upp.
- **Mallar** (`.resx` eller en databas) för texterna om de ska kunna översättas eller ändras utan release.
- **Asynkron leverans:** i produktion skulle kanalerna troligen köa meddelanden (outbox plus bakgrundstjänst), så att ett långsamt SMS-API inte gör orderflödet långsamt.
