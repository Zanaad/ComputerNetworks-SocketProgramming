# ComputerNetworks: C# Socket Programming project

## Overview

This project is TCP-based client-server application developed in C# using socket programming.
The server listens for the client over TCP on a specified port and assigns different access permissions (either full or read only)
based on the order of connection. Clients with full access can create, read, write and delete files on the server, while the clients with read only access can just view the content.

## Features

### Server

- The server listens on a user-specified IP address and port number.
- The server handles multiple client connections, up to a specified maximum. If the number of connections exceeds this limit, new connections are either rejected or queued.
- The server processes client requests and logs them for auditing purposes, including the IP address and timestamp.
- The server reads messages from clients and stores them for monitoring.
- Clients that do not send messages within a specified time frame are disconnected automatically. If the client reconnects, the server can recover the connection.
- One client is granted full access, while others are granted read-only access. Full access clients can create, read, write, and delete files. Read-only clients can only read files.

### Client

- Clients connect to the server using a specified IP address and port.
- Clients receive an access level (full or read-only) from the server based on the order of connection.
- Clients with full access can perform file operations such as create, read, write, and delete. Clients with read-only access can only read files.

## Requirements for running the application:

- Visual Studio with .NET desktop development workload.
- .NET Frameowork and appropriate SDK.
- For easy access to the project use the lattest Visual Studio.

## Setup

1. Clone the Repository

```bash
git clone https://github.com/Zanaad/ComputerNetworks-SocketProgramming.git
cd ComputerNetworks-SocketProgramming
devenv ComputerNetworks-SocketProgramming.sln
```

- If the devenv command does not work for you, you will need to specify the path to devenv.exe. Example: C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe

```bash
"The/Path/To/devenv.exe" ComputerNetworks-SocketProgramming.sln
```

- Alternative method should be openning .sln file on Visual Studio.

2. Run The server.
3. Run Multiple instances of the Client to simulate multiple client connections.

## Folder Structure

```bash
ComputerNetworks-SocketProgramming/
├── Client/
│       ├── Client.csproj
│       └── Program.cs                  # Client logic
│
│
├── Files/
│       ├── Client_Messeges_log.txt     #Client messages log
│       ├── Server_log.txt              #Server log
│
│
├── Server/
│       ├── Program.cs                  #Server Logic
│       └── Server.csproj
│
│
├── README.md                           #Your are here
│
│
└── ComputerNetworks-SocketProgramming.sln # Solution file
```

## Usage

### Running the Server:

1. Run the Server: The server listens to the port 5000 by default.
2. Logs:
   - Logs are saved in the files directory
     - server_log.txt records client connections and requests.
     - client_messages_log.txt records messages sent by clients for monitoring purposes.

### Running the Client

1. Connect to the Server:
   - Enter the ip adress and PORT(default: 5000).
   - The Server will confirm the access level.
2. Send Commands:
   - Full access clients:
     - INFO: Display a list of all available commands
     - LIST: List all files in the Files folder
     - CREATE [filename]: creates a file,
     - WRITE [filename] [content]: writes in the file specified,
     - READ [filename]: reads from the file specified,
     - DELETE [filename]: deletes the file specified,
     - EXIT: exits the application.
   - Read only access clients:
     - LIST: List all files created by clients in the Files folder
     - READ[filename]: reads from the file specified,
     - EXIT: exits the application.

## Example Commands

Full access clients:

```bash
INFO
LIST
CREATE testfile.txt
WRITE testfile.txt This is a test.
READ testfile.txt
DELETE testfile.txt
EXIT
```

Read only access clients:

```bash
LIST
READ testfile.txt
EXIT
```

## Notes

- Ensure the server is running before connecting with any clients.
- Only the first client gets full access, while others have read-only access.
- The server handles up to the maximum number of connections set. If the limit is exceeded, new connections are either rejected or queued.
- Clients that do not send messages within a timeout period will be disconnected, and the server will handle their reconnection if attempted.

## Credits

### Zana Ademi, Zana Shabani, Yllkë Berisha, Urtim Shehi.

## Have FUN.
