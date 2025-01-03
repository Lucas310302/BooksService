# Book Management System

The **Book Management System** is a web-based application designed to manage and access books. The system includes functionalities for users to **search, view, and download books** and for administrators to **add, modify, and delete books**. The application consists of a **frontend** (WPF Admin Panel) and a **backend** (ASP.NET Core API) that handles business logic, book storage, and user authentication.

## Features

### User Features:
- **Search for books** by title or author.
- **Download books** in PDF format.
- **View book details** like title, author, and availability status.

### Admin Features:
- **Add new books** to the database along with a PDF file.
- **Modify book details**, including title, author, and availability status.
- **Delete books** from the database.
- **View and manage users** (Admin functionality).

---

## How to Install

### Prerequisites:
Before setting up the project, make sure you have the following installed:
- **.NET SDK** (version 7.0 or higher) - [Install .NET SDK](https://dotnet.microsoft.com/download)
- **Visual Studio** (or any compatible IDE) - [Download Visual Studio](https://visualstudio.microsoft.com/downloads/)
- **SQL Server** (or any other compatible database server)
- **Swagger** (for testing APIs) - Follows along with the project

### Backend Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-repo/book-management-system.git
   cd book-management-system
2. **Install dependencies**:
   Ensure you have all the necessary NuGet packages installed by restoring dependencies:
   ```bash
   dotnet restore
3. **Configure Database**:
   - Modify the connection string in appsettings.json to point to your local SQL Server instance or the database you are using.
   - There is an .sql file that if run will create the tables needed automatically: SetupTables.sql
   - User signup can be made in swagger which is the networking tool that follows along with the project
4. **Migrate Database**(optional):
   ```bash
   dotnet ef database update
5. **Run the backend**:
   ```bash
   dotnet run

# Dependencies
## Backend (API):
- ASP.NET Core 7.0
- Entity Framework Core (for ORM and database interactions)
- BCrypt.Net-Next (for password hashing and authentication)
- JWT Authentication (for token-based authentication)
- FuzzySharp (for fuzzy search and typo tolerance)

## Frontend (WPF):
- Newtonsoft.Json (for JSON handling)
- System.Net.Http (for making HTTP requests)

# System Architecture
## Backend API
The backend API is developed using ASP.NET Core, providing endpoints to manage books and users. The API interacts with the SQL database using Entity Framework Core and handles user authentication using JWT tokens.

## Key API Controllers:
- BooksController: Manages all book-related functionality, including adding, modifying, deleting, and downloading books.
- UsersController: Handles user registration, login, and authentication using JWT tokens.

## Frontend (WPF)
The WPF Admin Panel interacts with the backend API, allowing administrators to add, update, and delete books. It also enables users to search for and download books from the database. The frontend uses HTTP requests to communicate with the backend and JWT tokens for secure authentication.

## MainWindow (User Experience)
The MainWindow is the central hub for the user experience, providing an intuitive interface for searching, viewing, and downloading books.

**User Interface Layout**:
- Search Bar: Allows users to search for books by title or author.
- Book List: Displays matching books with options to view details and download PDFs.
- Book Details: Shows additional book information when selected.

**User Actions**:
- Search: Type a query into the search bar to filter the list of books.
- View Book Details: Click on a book to see its full details, including availability.
- Download: Click the download button to save the PDF file of the book to your device.

---

# Summary
The Book Management System provides a seamless experience for both users and administrators. Users can search for books and download them, while administrators can manage the entire book collection. The system is built using ASP.NET Core, with a WPF frontend to deliver an efficient, easy-to-use solution for managing and accessing books.
