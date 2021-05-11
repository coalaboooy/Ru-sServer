import socket

client_id = input("Enter client ID to start a new client: ")

try:
    conn = socket.socket()
    conn.connect(("127.0.0.1", 9999))
    data = "clinet (" + client_id + ") connected"
    conn.send(data.encode(encoding='ascii', errors='replace'))
    while True:
        message = input()
        data = "(" + client_id + ")" + message
        conn.send(data.encode(encoding='ascii', errors='replace'))
        if message.lower() == "end":
            print("Client closing conection")
            break
except socket.error as err:
    print("An error has occured:\n" + str(err))
    conn.close()

conn.close()
