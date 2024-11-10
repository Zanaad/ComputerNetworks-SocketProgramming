# ComputerNetworks: C# Socket Programming project
## Overview
This project is TCP-based client-server application developed in C# using socket programming.
The server listens for the client over TCP on a specified port and assigns different access permissions (either full or read only)
based on the order of connection. Clients with full access can create, read, write and delete files on the server, while the clients with read only access can just view the content.
## Features
### Server
- Listens for incomming connections,
- Greants access to the first connected client and read only access to the others,
- Handles varius file operations(create, read, write, delete) based on the access,
- Logs connection, requests and client messeges for monitoring purposes.
### Client
- Connects to the server with a specified IP and port,
- Receives an access level from the server,
- Sends file operation commands to the server according to its access.
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
``` bash
ComputerNetworks-SocketProgramming/
├── Client/
│       ├── Client.csproj              
│       └── Program.cs                  # Client logic
│       
│
├── Files/
│       ├── Client_Messeges_log.txt     #Client messages log
│       ├── Server_log.txt              #Server log
│       └── zana.txt.txt
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
└── ComputerNetworks-SocketProgramming.sln #YOU WILL OPEN THIS
```
## Usage
### Running the Server:
1. Run the Server: The server listens to the port 5000 by default.
2. Logs:
    - Logs are saved in the files directory
        - server_log.txt records client connections and requests.
        - Client_Messeges_log records messeges from clients for monitoring.
### Running the Client
1. Connect to the Server:
    - Enter the ip adress and PORT(default: 5000).
    - The Server will confirm the access level.
2. Send Commands:
    - Full access clients:
        - CREATE [filename]: creates a file,
        - WRITE [filename] [content]: writes in the file specified,
        - READ [filename]: reads from the file specified,
        - DELETE [filename]: deletes the file specified,
        - EXIT: exits the application.
    - Read only access clients:
        - READ[filename]: reads from the file specified,
        - EXIT: exits the application.
## Example Commands
Full access clients: 
``` bash
CREATE testfile.txt
WRITE testfile.txt This is a test.
READ testfile.txt
DELETE testfile.txt
EXIT
```
Read only access clients: 
``` bash
READ testfile.txt
EXIT
```
## Notes 
- Make sure the Server is running before you try to connect with any clients.
- Only the first client gets full access not the others.

## Credits
### Zana Ademi, Zana Shabani, Yllkë Berisha, Urtim Shehi.

## Have FUN.