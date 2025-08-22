Для запуска docker compose необходимо скопировать файл .env-template -> .env и заполнить его

Чтобы запустить локально код c# нужно настроить недостающие данные в appsettings.json.
Есть 3 способа (на примере строки соединения).
1. (Рекомендуемый)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=well_sensor_analytics;Username=postgres;Password=pass;"
Из официальной документации:
The Secret Manager tool doesn't encrypt the stored secrets and shouldn't be treated as a trusted store. It's for development purposes only. The keys and values are stored in a JSON configuration file in the user profile directory.
2. В файле appsettings.json заполнить строку DefaultConnection.
В этом случае придется смотреть, чтобы случайно не запушить это в репизоторий.
3. Установить переменную среды ConnectionStrings:DefaultConnection со значением строки соединения
