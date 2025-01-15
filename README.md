# P2P Chat App

A peer-to-peer (P2P) chat application built in C# as part of the 725G66 course at Linköping University.

## Features

- **Direct peer-to-peer communication**
  - Users connect directly to each other.
- **Support for multiple connections**
  - Connect and communicate with multiple peers simultaneously.
- **User-friendly UI**
  - A simple and intuitive interface.

## Requirements

- **Operating System**: Windows
- **.NET Version**: .NET 9.0 or higher

## Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/leodiazsanchez/725G66.git
   cd 725G66/ChatApp
   ```

2. **Build the application**:
   Open the solution file in Visual Studio or use the .NET CLI:

   ```bash
   dotnet build
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

## Usage

1. Launch the application.
2. Enter your username and the port number you wish to use.
3. Share your IP and port number with other users to connect.
4. Use the interface to send messages.

## Architecture

- **Networking**: Utilizes `TcpListener` and `TcpClient` for socket communication.
- **Multithreading**: Uses `Task` to handle multiple connections and operations simultaneously.
- **Serialization**: Messages are serialized and deserialized using JSON for easy communication.
