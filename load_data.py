import argparse
import subprocess
import sys
import os
from datetime import datetime, timedelta

def get_time_delta(time_range_str):
    """Преобразует строку '1 week' или '1 day' в объект timedelta."""
    parts = time_range_str.split()
    if len(parts) != 2:
        return None
    
    amount = int(parts[0])
    unit = parts[1].lower()

    if unit in ('day', 'days'):
        return timedelta(days=amount)
    elif unit in ('week', 'weeks'):
        return timedelta(weeks=amount)
    elif unit in ('month', 'months'):
        # Приблизительный расчет, так как месяцы имеют разную длину
        return timedelta(days=amount * 30)
    elif unit in ('year', 'years'):
        return timedelta(days=amount * 365)
    return None

def main():
    parser = argparse.ArgumentParser(description='Скачать данные сенсора с удалённого сервера.')
    parser.add_argument('server_user', help='Имя пользователя на сервере')
    parser.add_argument('server_host', help='Хост сервера')
    parser.add_argument('channel_id', type=int, help='ID канала для выгрузки данных')
    parser.add_argument('time_range', help="Промежуток времени для выгрузки (например, '1 day', '1 week')")
    parser.add_argument('--date-time', action='store_true', help='Использовать форматированный вывод времени')
    
    args = parser.parse_args()

    # Вычисление дат
    end_date = datetime.now()
    time_delta = get_time_delta(args.time_range)
    if not time_delta:
        print("Ошибка: Неверный формат промежутка времени. Используйте '1 day', '1 week' и т.д.")
        sys.exit(1)
        
    start_date = end_date - time_delta

    # Формирование локального имени файла
    local_filename = f"data/dump_{args.channel_id}_{start_date.strftime('%d.%m')}-{end_date.strftime('%d.%m')}.csv"
    server_address = f"{args.server_user}@{args.server_host}"
    print(server_address)

    # Формирование и выполнение команды на сервере
    server_script_path = "/home/common/dumps/dump_to_csv.sh"
    # Combine the script path and its arguments into a single string

    ssh_command = ["ssh", server_address, server_script_path, str(args.channel_id), f"'{args.time_range}'"]

    if args.date_time:
        ssh_command += "--date-time"

    try:
        print(f"Выполнение удалённого скрипта на {server_address}...")
        # Add `capture_output=True` to get stdout and stderr
        result = subprocess.run(ssh_command, check=True, text=True, capture_output=True)

        # The rest of your code seems to be correct after this point.
        remote_filename = os.path.basename(result.stdout.strip())
        
        if not remote_filename:
            print("Ошибка: Не удалось получить имя файла с сервера. Проверьте путь к скрипту и права доступа.")
            print(f"Вывод stderr: {result.stderr.strip()}")
            sys.exit(1)

        print(f"Временный файл создан на сервере: {remote_filename}")

        # Скачивание файла с помощью SCP
        print(f"Загрузка файла и сохранение как {local_filename}...")
        scp_command = ["scp", f"{server_address}:~/{remote_filename}", local_filename]
        subprocess.run(scp_command, check=True)

        # Удаление временного файла на сервере
        print(f"Удаление временного файла на сервере: {remote_filename}")
        rm_command = ["ssh", server_address, f"rm ~/{remote_filename}"]
        subprocess.run(rm_command, check=True)

        print("Готово! Файл успешно сохранён.")

    except FileNotFoundError:
        print("Ошибка: Убедитесь, что команды 'ssh' и 'scp' установлены и доступны в PATH.")
        sys.exit(1)
    except subprocess.CalledProcessError as e:
        print(f"Произошла ошибка при выполнении внешней команды: {e.cmd}")
        print(f"Код возврата: {e.returncode}")
        print(f"Ошибка: {e.stderr}")
        sys.exit(1)

if __name__ == '__main__':
    main()
