import socket

client_id = input("Enter client ID to start a new client: ")

try:
    conn = socket.socket()
    conn.connect(("127.0.0.1", 9999))
    while True:
        message = input()
        data = "(" + client_id + ")" + message
        conn.send(data.encode(encoding='ascii', errors='replace'))
        if message.lower() == "end":
            print("Client closing conection")
            break
except socket.error:
    print("An error has occured")
    conn.close()

conn.close()
