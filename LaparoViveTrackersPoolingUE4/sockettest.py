import socket

# creates socket object
s = socket.socket(socket.AF_INET,
                  socket.SOCK_STREAM)

host = '192.168.1.21' # or just use (host = '')
port = 8555

s.connect((host, port))

tm = s.recv(1024) # msg can only be 1024 bytes long

s.close()
print("the time we got from the server is %s" % tm.decode('ascii'))