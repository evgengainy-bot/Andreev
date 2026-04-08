это вставить в CMD. строка создает БД mycust если ее нет в postgressql и копирует конфигурацию БД из файла mycust_0.sql
psql -U postgres -c "CREATE DATABASE mycust" 2>nul & psql -U postgres -d mycust -f C:/ВАШ ПУТЬ/Scripts/mycust_0.sql
Пользователь базы данных: postgres
Пароль для подключения к базе данных: postgres