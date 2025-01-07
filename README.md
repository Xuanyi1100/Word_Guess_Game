# Word Guessing Game 

This project is my practice to design and implement networked applications using modern technologies.

The Word Guessing Game is a fun and interactive application designed to challenge users' vocabulary and quick-thinking skills. The game allows users to connect to a server, receive a character string, and guess words within a time limit.

This repository contains the source code for a word guessing game application built using C# and WPF for the client, C# for the server and mysql for the database. 


## Features

- **Client-Server Architecture:** The application uses a client-server model for game logic and data management.
- **User Authentication:** Users can enter their name before connecting to the server.
- **Configurable Server Settings:** Users can specify the server IP address, port, and client port.
- **Time Limit:** The game has a configurable time limit for each session.
- **Word Guessing:** Users can input guesses and receive feedback on whether their guess is correct or incorrect.
- **Game State Management:** The server manages the game state, including the character string, total words, and words found.
- **Real-time Updates:** The client UI updates in real-time based on server responses.
- **Graceful Shutdown:** The server can be shut down gracefully, notifying all connected clients.
- **Play Again:** Users can choose to play again after winning a game.
- **End Game:** Users can end the game at any time.

## Technologies Used

- **C#:** The primary programming language used for both client and server applications.
- **WPF (Windows Presentation Foundation):** Used for building the client-side user interface.
- **.NET SDK:** The development platform for building the application.
- **TCP Sockets:** Used for network communication between the client and server.
- **MVVM (Model-View-ViewModel):** The architectural pattern used for structuring the client application.
- **Newtonsoft.Json:** Used for serializing and deserializing data.
- **MySQL:** Used for storing and retrieving game data.
- **Entity Framework:** Used for interacting with the MySQL database.

## Repository Contents
- two versions of application, one with mysql and one without
- communication protocol
- mysql DDL
- Entity Relationship Diagram

## How to run

1. For version without mysql, put "gameStrings"folder in server executable file folder, then run server and client.
2. For version with mysql, set up mysql server, run ddlthen run server and client.

## Project Structure

The project is structured as follows:

1. version without mysql
- **`client/`:** Contains the client-side application code.
    - **`MainWindow.xaml` :** The main window of the application.
    - **`Views/`:** Contains the UI views.
        - **`connectPage.xaml` :** The connect page for entering server details.
        - **`gamePage.xaml` :** The game page for playing the word guessing game.
    - **`ViewModels/`:** Contains the view models.
        - **`MainViewModel.cs` :** The main view model for managing navigation.
        - **`ConnectViewModel.cs` :** The view model for the connect page.
        - **`GameViewModel.cs` :** The view model for the game page.
    - **`Models/`:** Contains the data models and services.
        - **`GameModel.cs` :** The data model for the game.
        - **`TcpClientService.cs` :** The service for handling TCP communication.
        - **`ConnectModel.cs` :** The data model for the connect page.
        - **`TcpServerListener.cs` :** The service for listening to server shutdown messages.
    - **`Helpers/`:** Contains helper classes.
        - **`RelayCommand.cs` :** A helper class for implementing commands.
        - **`AppConfigManager.cs` :** A helper class for managing application settings.
        - **`ClientMessage.cs` :** The data structure for client messages.
        - **`ServerMessage.cs` :** The data structure for server messages.
        - **`HeaderHelper.cs` :** A helper class for handling message headers.
        - **`ReceiveResult.cs` :** The data structure for receive results.
- **`server/`:** Contains the server-side application code.
    - **`Program.cs` :** The main entry point for the server application.
    - **`Listener.cs` :** The class for listening to incoming connections.
    - **`HandleClient.cs` :** The class for handling client connections.
    - **`Game.cs` :** The class for managing game logic.
    - **`Helpers/`:** Contains helper classes.
        - **`ClientMessage.cs` :** The data structure for client messages.
        - **`ServerMessage.cs` :** The data structure for server messages.
        - **`HeaderHelper.cs` :** A helper class for handling message headers.
        - **`ReceiveResult.cs` :** The data structure for receive results.

2. version with mysql
- **`client/`:** Contains the client-side application code.
    - **`MainWindow.xaml` :** The main window of the application.
    - **`Views/`:** Contains the UI views.
        - **`connectPage.xaml` :** The connect page for entering server details.
        - **`gamePage.xaml` :** The game page for playing the word guessing game.
    - **`ViewModels/`:** Contains the view models.
        - **`MainViewModel.cs` :** The main view model for managing navigation.
        - **`ConnectViewModel.cs` :** The view model for the connect page.
        - **`GameViewModel.cs` :** The view model for the game page.
    - **`Models/`:** Contains the data models and services.
        - **`GameModel.cs` :** The data model for the game.
        - **`TcpClientService.cs` :** The service for handling TCP communication.
        - **`ConnectModel.cs` :** The data model for the connect page.
        - **`TcpServerListener.cs` :** The service for listening to server shutdown messages.
    - **`Helpers/`:** Contains helper classes.
        - **`RelayCommand.cs` :** A helper class for implementing commands.
        - **`AppConfigManager.cs` :** A helper class for managing application settings.
        - **`ClientMessage.cs` :** The data structure for client messages.
        - **`ServerMessage.cs` :** The data structure for server messages.
        - **`HeaderHelper.cs` :** A helper class for handling message headers.
        - **`ReceiveResult.cs` :** The data structure for receive results.
- **`server/`:** Contains the server-side application code.
    - **`Program.cs` :** The main entry point for the server application.
    - **`Listener.cs` :** The class for listening to incoming connections.
    - **`HandleClient.cs` :** The class for handling client connections.
    - **`DatabaseModels/`:** Contains the database models.
    - **`GameDataContext.cs` :** The data context for the database.
    - **`Helpers/`:** Contains helper classes.
        - **`ClientMessage.cs` :** The data structure for client messages.
        - **`ServerMessage.cs` :** The data structure for server messages.
        - **`HeaderHelper.cs` :** A helper class for handling message headers.
        - **`ReceiveResult.cs` :** The data structure for receive results.
        
       

## Future Improvements

- Implement a leaderboard to track high scores.
- Add more word categories and difficulty levels.
- Enhance the UI with animations and sound effects.
- Add a chat feature to allow players to communicate with each other.
- Add a multiplayer feature to allow players to play with each other.
