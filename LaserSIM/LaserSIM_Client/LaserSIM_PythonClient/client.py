###
### ВНИМАНИЕ! Этот код будет работать только на версиях, поддерживающих
### PEP 634, 635 и 636 (конструкции match/case)
### последняя версия на момент написания (17.05.21) - 3.10.0b1(beta)
### 

import socket
from time import sleep
from threading import Thread
from enum import Enum

def send():
    """Отправляет данные в форме [ID клиента, состояние датчика]"""
    while True:
        conn.send(bytes([client_id, current_state.value]))
        sleep(1)
        if current_state == States.Off:
            print("Closing connection")
            break

class States(Enum):
    """Состояния датчика лазертага.

    1. Inactive - Датчик включен, но не активен.
    2. Hits_0 - По датчику было произведено ноль попаданий.
    3. Hits_1 - По датчику было произведено одно попадание.
    4. Hits_2 - По датчику было произведено два попадания.
    5. Dead - Датчик выведен из игры. Активируется при попадании по датчику, имеющему состояние Hits_2
    6. Off - Датчик выключается, завершая работу. Используется для завершения соединения.
    """
    Inactive = 0
    Hits_0 = 2
    Hits_1 = 4
    Hits_2 = 8
    Dead = 16
    Off = 255

conn = socket.socket()
client_id = int(input("Введите ID клиента (целое число в диапазоне 0-255): "))
current_state = States.Inactive

try:
    #отправка сообщения о подключении клиента
    conn.connect(("127.0.0.1", 9999))
    data = "Client " + str(client_id) + " connected"
    conn.send(data.encode(encoding='ascii', errors='replace'))

    #запуск потока с методом отправки сообщения
    thSend = Thread(target=send)
    thSend.start()

    while True:
        #выбор состояния датчика
        ch = input("Введите символы для получения состояния датчика.\n0 - неактивен\n1 - ноль попаданий\n'+' - увеличение количества попаданий\n'-' - выключение датчика\nЛюбые другие символы не будут изменять значение: ")
        match ch:
            case '0':
                current_state = States.Inactive
            case '1':
                current_state = States.Hits_0
            case '+':
                match current_state:
                    case States.Hits_0:
                        current_state = States.Hits_1
                    case States.Hits_1:
                        current_state = States.Hits_2
                    case States.Hits_2:
                        current_state = States.Dead
                    case _:
                        pass
            case '-':
                current_state = States.Off
            case _:
                pass
        if current_state == States.Off:
            conn.send(bytes([client_id, current_state.value]))
            break
        
except socket.error as err:
    print("An error has occured:\n" + str(err))
    conn.close()

conn.close()
